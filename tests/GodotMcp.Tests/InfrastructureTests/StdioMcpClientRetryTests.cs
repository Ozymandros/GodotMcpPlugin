using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Client;
using GodotMcp.Infrastructure.Configuration;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for StdioMcpClient retry logic
/// Validates: Requirements 14.4, 2.1, 6.5, 8.5
/// </summary>
public sealed class StdioMcpClientRetryTests : IAsyncDisposable
{
    private readonly IProcessManager _mockProcessManager;
    private readonly IRequestHandler _mockRequestHandler;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly List<StdioMcpClient> _clientsToDispose = new();

    public StdioMcpClientRetryTests()
    {
        _mockProcessManager = Substitute.For<IProcessManager>();
        _mockRequestHandler = Substitute.For<IRequestHandler>();
        _logger = NullLogger<StdioMcpClient>.Instance;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clientsToDispose)
        {
            try
            {
                await client.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors in tests
            }
        }
    }

    private StdioMcpClient CreateClient(
        GodotMcpOptions options,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        var client = new StdioMcpClient(
            _mockProcessManager,
            _mockRequestHandler,
            _logger,
            Options.Create(options),
            delayAsync);
        _clientsToDispose.Add(client);
        return client;
    }

    private async Task SetupConnectedClient(StdioMcpClient client)
    {
        var processInfo = new ProcessInfo(1234, "godot-mcp", DateTime.UtcNow);
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(processInfo));

        await client.ConnectAsync();
    }

    [Fact]
    public async Task InvokeToolAsync_OnNetworkException_RetriesUpToMaxAttempts()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10, // Short delay for test
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        var callCount = 0;
        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
            .Returns(_ =>
            {
                callCount++;
                return requestJson;
            });

        // Simulate stream that always throws IOException (NetworkException)
        var mockStdin = new MemoryStream();
        mockStdin.Close(); // Closed stream will throw when written to

        _mockProcessManager.StandardInput.Returns(mockStdin);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NetworkException>(
            () => client.InvokeToolAsync("test", parameters));

        // Should have tried initial attempt + 3 retries = 4 total attempts
        Assert.Equal(4, callCount);
    }

    [Fact]
    public async Task InvokeToolAsync_OnTimeoutException_RetriesUpToMaxAttempts()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 1, // Short timeout
            MaxRetryAttempts = 2,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10, // Short delay for test
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        var callCount = 0;
        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
            .Returns(_ =>
            {
                callCount++;
                return requestJson;
            });

        var mockStdin = new MemoryStream();
        
        // Create a custom stream that blocks on read to simulate timeout
        var mockStdout = new BlockingStream();

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Core.Exceptions.TimeoutException>(
            () => client.InvokeToolAsync("test", parameters));

        // Should have tried initial attempt + 2 retries = 3 total attempts
        Assert.Equal(3, callCount);
    }

    /// <summary>
    /// Custom stream that blocks indefinitely on read operations to simulate timeout
    /// </summary>
    private class BlockingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => 0; set => throw new NotSupportedException(); }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Block indefinitely
            Thread.Sleep(Timeout.Infinite);
            return 0;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // Wait until cancelled (simulates timeout)
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    [Fact]
    public async Task InvokeToolAsync_OnProtocolException_DoesNotRetry()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var invalidResponseJson = """invalid json""";

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(invalidResponseJson)
            .Returns(_ => throw new ProtocolException("Invalid JSON"));

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(invalidResponseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProtocolException>(
            () => client.InvokeToolAsync("test", parameters));

        // Should only try once (no retries for protocol errors)
        _mockRequestHandler.Received(1).SerializeRequest(Arg.Any<McpRequest>());
        _mockRequestHandler.Received(1).DeserializeResponse(invalidResponseJson);
    }

    [Fact]
    public async Task InvokeToolAsync_OnMcpServerException_DoesNotRetry()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","error":{"code":-32600,"message":"Invalid Request"}}""";
        var errorResponse = new McpResponse(
            "1", 
            false, 
            null, 
            new McpError(-32600, "Invalid Request"));

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(errorResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpServerException>(
            () => client.InvokeToolAsync("test", parameters));

        // Should only try once (no retries for server errors)
        _mockRequestHandler.Received(1).SerializeRequest(Arg.Any<McpRequest>());
        _mockRequestHandler.Received(1).DeserializeResponse(responseJson);
    }

    [Fact]
    public async Task InvokeToolAsync_RespectsMaxRetryAttempts()
    {
        // Arrange - Test with different MaxRetryAttempts values
        var testCases = new[] { 0, 1, 2, 5 };

        foreach (var maxRetries in testCases)
        {
            var options = new GodotMcpOptions
            {
                ExecutablePath = "godot-mcp",
                ConnectionTimeoutSeconds = 5,
                RequestTimeoutSeconds = 10,
                MaxRetryAttempts = maxRetries,
                BackoffStrategy = BackoffStrategy.Linear,
                InitialRetryDelayMs = 10,
                EnableProcessPooling = false,
                MaxIdleTimeSeconds = 300,
                EnableMessageLogging = false
            };
            var client = CreateClient(options);
            await SetupConnectedClient(client);

            var parameters = new Dictionary<string, object?>();
            var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

            var callCount = 0;
            _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
                .Returns(_ =>
                {
                    callCount++;
                    return requestJson;
                });

            var mockStdin = new MemoryStream();
            mockStdin.Close(); // Closed stream will throw

            _mockProcessManager.StandardInput.Returns(mockStdin);

            // Act & Assert
            await Assert.ThrowsAsync<NetworkException>(
                () => client.InvokeToolAsync("test", parameters));

            // Should have tried initial attempt + maxRetries
            var expectedAttempts = maxRetries + 1;
            Assert.Equal(expectedAttempts, callCount);

            // Reset for next iteration
            callCount = 0;
        }
    }

    [Fact]
    public async Task InvokeToolAsync_LinearBackoff_CalculatesCorrectDelays()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 100,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var observedDelays = new List<TimeSpan>();
        Task DelayRecorder(TimeSpan delay, CancellationToken ct)
        {
            observedDelays.Add(delay);
            return Task.CompletedTask;
        }

        var client = CreateClient(options, DelayRecorder);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
            .Returns(_ =>
            {
                return requestJson;
            });

        var mockStdin = new MemoryStream();
        mockStdin.Close(); // Closed stream will throw

        _mockProcessManager.StandardInput.Returns(mockStdin);

        // Act
        await Assert.ThrowsAsync<NetworkException>(
            () => client.InvokeToolAsync("test", parameters));

        // Assert - Linear backoff: delay = InitialRetryDelayMs * attempt
        // Attempt 1: immediate
        // Attempt 2: after 100ms (1 * 100)
        // Attempt 3: after 200ms (2 * 100)
        // Attempt 4: after 300ms (3 * 100)
        Assert.Equal(3, observedDelays.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(100), observedDelays[0]);
        Assert.Equal(TimeSpan.FromMilliseconds(200), observedDelays[1]);
        Assert.Equal(TimeSpan.FromMilliseconds(300), observedDelays[2]);
    }

    [Fact]
    public async Task InvokeToolAsync_ExponentialBackoff_CalculatesCorrectDelays()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 100,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var observedDelays = new List<TimeSpan>();
        Task DelayRecorder(TimeSpan delay, CancellationToken ct)
        {
            observedDelays.Add(delay);
            return Task.CompletedTask;
        }

        var client = CreateClient(options, DelayRecorder);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
            .Returns(_ =>
            {
                return requestJson;
            });

        var mockStdin = new MemoryStream();
        mockStdin.Close(); // Closed stream will throw

        _mockProcessManager.StandardInput.Returns(mockStdin);

        // Act
        await Assert.ThrowsAsync<NetworkException>(
            () => client.InvokeToolAsync("test", parameters));

        // Assert - Exponential backoff: delay = InitialRetryDelayMs * 2^(attempt-1)
        // Attempt 1: immediate
        // Attempt 2: after 100ms (100 * 2^0 = 100)
        // Attempt 3: after 200ms (100 * 2^1 = 200)
        // Attempt 4: after 400ms (100 * 2^2 = 400)
        Assert.Equal(3, observedDelays.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(100), observedDelays[0]);
        Assert.Equal(TimeSpan.FromMilliseconds(200), observedDelays[1]);
        Assert.Equal(TimeSpan.FromMilliseconds(400), observedDelays[2]);
    }

    [Fact]
    public async Task InvokeToolAsync_SucceedsAfterRetry_ReturnsSuccessfully()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"status":"success"}}""";
        var expectedResponse = new McpResponse("1", true, new { status = "success" });

        var attemptCount = 0;
        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
            .Returns(_ =>
            {
                attemptCount++;
                return requestJson;
            });
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        // Create streams that fail first 2 times, succeed on 3rd
        var successStdin = new MemoryStream();
        var successStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        successStdout.Write(responseBytes, 0, responseBytes.Length);
        successStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(_ =>
        {
            if (attemptCount < 3)
            {
                // Return closed stream for first 2 attempts
                var failStream = new MemoryStream();
                failStream.Close();
                return failStream;
            }
            // Return working stream on 3rd attempt
            return successStdin;
        });

        _mockProcessManager.StandardOutput.Returns(successStdout);

        // Act
        var response = await client.InvokeToolAsync("test", parameters);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(3, attemptCount); // Should have tried 3 times before succeeding
    }

    [Fact]
    public async Task InvokeToolAsync_WithZeroRetries_FailsImmediately()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0, // No retries
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        var callCount = 0;
        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>())
            .Returns(_ =>
            {
                callCount++;
                return requestJson;
            });

        var mockStdin = new MemoryStream();
        mockStdin.Close(); // Closed stream will throw

        _mockProcessManager.StandardInput.Returns(mockStdin);

        // Act & Assert
        await Assert.ThrowsAsync<NetworkException>(
            () => client.InvokeToolAsync("test", parameters));

        // Should only try once (no retries)
        Assert.Equal(1, callCount);
    }
}
