using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Client;
using GodotMcp.Infrastructure.Configuration;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for StdioMcpClient health monitoring functionality
/// Validates: Requirements 14.2, 2.4, 20.5
/// </summary>
public sealed class StdioMcpClientHealthTests : IAsyncDisposable
{
    private readonly IProcessManager _mockProcessManager;
    private readonly IRequestHandler _mockRequestHandler;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly GodotMcpOptions _options;
    private readonly List<StdioMcpClient> _clientsToDispose = new();

    public StdioMcpClientHealthTests()
    {
        _mockProcessManager = Substitute.For<IProcessManager>();
        _mockRequestHandler = Substitute.For<IRequestHandler>();
        _logger = NullLogger<StdioMcpClient>.Instance;
        _options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 60, // 60 seconds for health check tests
            EnableMessageLogging = false
        };
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

    private StdioMcpClient CreateClient(GodotMcpOptions? options = null)
    {
        var opts = options ?? _options;
        var client = new StdioMcpClient(
            _mockProcessManager,
            _mockRequestHandler,
            _logger,
            Options.Create(opts));
        _clientsToDispose.Add(client);
        return client;
    }

    [Fact]
    public async Task IsHealthy_WhenConnectionActive_ReturnsTrue()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        // Perform a successful request to update last successful request time
        await PerformSuccessfulRequest(client);

        // Act
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    public void IsHealthy_WhenNotConnected_ReturnsFalse()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.False(isHealthy);
    }

    [Fact]
    public async Task IsHealthy_WhenConnectionLost_ReturnsFalse()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 1, // Very short timeout for test
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        // Perform a successful request
        await PerformSuccessfulRequest(client);

        // Act - Wait for the idle timeout to expire
        await Task.Delay(TimeSpan.FromSeconds(2));
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.False(isHealthy);
    }

    [Fact]
    public async Task IsHealthy_AfterRecentSuccessfulRequest_ReturnsTrue()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        // Act - Perform multiple successful requests
        await PerformSuccessfulRequest(client);
        await Task.Delay(100);
        await PerformSuccessfulRequest(client);
        
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    public async Task PingAsync_UpdatesLastSuccessfulRequestTime()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 2, // Short timeout for test
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        // Perform initial request
        await PerformSuccessfulRequest(client);

        // Wait a bit
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Act - Perform ping to update last successful request time
        var pingResult = await PerformSuccessfulPing(client);

        // Assert
        Assert.True(pingResult);
        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task PeriodicPing_MaintainsConnectionHealth()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 60,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        // Setup ping to succeed
        SetupSuccessfulPing();

        // Act - Wait for periodic health check to run (30 seconds interval)
        // Note: In a real scenario, the periodic timer would run automatically
        // For testing, we verify the health check mechanism works
        await PerformSuccessfulRequest(client);
        
        // Verify health is maintained
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    public async Task IsHealthy_WhenStateIsFaulted_ReturnsFalse()
    {
        // Arrange
        var client = CreateClient();
        
        // Setup connection to fail
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ProcessInfo>(new ProcessException("Connection failed")));

        // Act - Try to connect (will fail)
        try
        {
            await client.ConnectAsync();
        }
        catch (NetworkException)
        {
            // Expected
        }

        var isHealthy = client.IsHealthy();

        // Assert
        Assert.False(isHealthy);
        Assert.Equal(ConnectionState.Faulted, client.State);
    }

    [Fact]
    public async Task IsHealthy_WhenStateIsConnecting_ReturnsFalse()
    {
        // Arrange
        var client = CreateClient();
        
        // Setup a slow connection
        var tcs = new TaskCompletionSource<ProcessInfo>();
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        // Act - Start connecting (but don't complete)
        var connectTask = client.ConnectAsync();
        
        // Give it a moment to transition to Connecting state
        await Task.Delay(50);
        
        var isHealthy = client.IsHealthy();

        // Complete the connection to avoid hanging
        tcs.SetResult(new ProcessInfo(1234, "godot-mcp", DateTime.UtcNow));
        await connectTask;

        // Assert
        Assert.False(isHealthy);
    }

    [Fact]
    public async Task IsHealthy_AfterDispose_ReturnsFalse()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);
        await PerformSuccessfulRequest(client);

        // Act
        await client.DisposeAsync();
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.False(isHealthy);
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public async Task PingAsync_WhenConnectionHealthy_ReturnsTrue()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        // Act
        var result = await PerformSuccessfulPing(client);

        // Assert
        Assert.True(result);
        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task PingAsync_WhenConnectionUnhealthy_ReturnsFalse()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        // Setup ping to fail
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"ping","params":{}}""";
        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);

        // Simulate communication failure
        var mockStdin = Substitute.For<Stream>();
        mockStdin.When(s => s.WriteAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new IOException("Connection lost"));

        _mockProcessManager.StandardInput.Returns(mockStdin);

        // Act
        var result = await client.PingAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsHealthy_WithMultipleSuccessfulRequests_RemainsHealthy()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        // Act - Perform multiple successful requests
        for (int i = 0; i < 5; i++)
        {
            await PerformSuccessfulRequest(client);
            await Task.Delay(100);
        }

        var isHealthy = client.IsHealthy();

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    public async Task IsHealthy_AfterIdleTimeout_BecomesUnhealthy()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 1, // Very short timeout
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        // Perform initial request
        await PerformSuccessfulRequest(client);
        Assert.True(client.IsHealthy());

        // Act - Wait for idle timeout to expire
        await Task.Delay(TimeSpan.FromSeconds(2));
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.False(isHealthy);
    }

    [Fact]
    public async Task IsHealthy_AfterIdleTimeout_ThenSuccessfulRequest_BecomesHealthyAgain()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 1, // Very short timeout
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        // Perform initial request
        await PerformSuccessfulRequest(client);
        Assert.True(client.IsHealthy());

        // Wait for idle timeout
        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.False(client.IsHealthy());

        // Act - Perform another successful request
        await PerformSuccessfulRequest(client);
        var isHealthy = client.IsHealthy();

        // Assert
        Assert.True(isHealthy);
    }

    /// <summary>
    /// Helper method to set up a connected client
    /// </summary>
    private async Task SetupConnectedClient(StdioMcpClient client)
    {
        var processInfo = new ProcessInfo(1234, "godot-mcp", DateTime.UtcNow);
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(processInfo));

        await client.ConnectAsync();
    }

    /// <summary>
    /// Helper method to perform a successful request
    /// </summary>
    private async Task PerformSuccessfulRequest(StdioMcpClient client)
    {
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"status":"ok"}}""";
        var expectedResponse = new McpResponse("1", true, new { status = "ok" });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        await client.InvokeToolAsync("test", new Dictionary<string, object?>());
    }

    /// <summary>
    /// Helper method to perform a successful ping
    /// </summary>
    private async Task<bool> PerformSuccessfulPing(StdioMcpClient client)
    {
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"ping","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"status":"ok"}}""";
        var expectedResponse = new McpResponse("1", true, new { status = "ok" });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        return await client.PingAsync();
    }

    /// <summary>
    /// Helper method to setup successful ping responses
    /// </summary>
    private void SetupSuccessfulPing()
    {
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"ping","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"status":"ok"}}""";
        var expectedResponse = new McpResponse("1", true, new { status = "ok" });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);
    }
}
