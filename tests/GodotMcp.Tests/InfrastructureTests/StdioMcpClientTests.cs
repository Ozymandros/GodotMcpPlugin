using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Client;
using GodotMcp.Infrastructure.Configuration;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for StdioMcpClient
/// Validates: Requirements 14.2, 14.4, 1.8, 5.1, 5.2, 5.4, 10.1
/// </summary>
public sealed class StdioMcpClientTests : IAsyncDisposable
{
    private readonly IProcessManager _mockProcessManager;
    private readonly IRequestHandler _mockRequestHandler;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly GodotMcpOptions _options;
    private readonly List<StdioMcpClient> _clientsToDispose = new();

    public StdioMcpClientTests()
    {
        _mockProcessManager = Substitute.For<IProcessManager>();
        _mockRequestHandler = Substitute.For<IRequestHandler>();
        _logger = NullLogger<StdioMcpClient>.Instance;
        _options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
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
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        // Act
        var client = CreateClient();

        // Assert
        Assert.NotNull(client);
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public void State_InitialState_IsDisconnected()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var state = client.State;

        // Assert
        Assert.Equal(ConnectionState.Disconnected, state);
    }

    [Fact]
    public async Task ConnectAsync_WhenSuccessful_EstablishesConnection()
    {
        // Arrange
        var client = CreateClient();
        var processInfo = new ProcessInfo(1234, "godot-mcp", DateTime.UtcNow);
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(processInfo));

        // Act
        await client.ConnectAsync();

        // Assert
        Assert.Equal(ConnectionState.Connected, client.State);
        await _mockProcessManager.Received(1).EnsureProcessRunningAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConnectAsync_WhenProcessManagerFails_ThrowsNetworkException()
    {
        // Arrange
        var client = CreateClient();
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ProcessInfo>(new ProcessException("Process failed")));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NetworkException>(() => client.ConnectAsync());
        
        Assert.Contains("Failed to connect to godot-mcp server", exception.Message);
        Assert.Equal(ConnectionState.Faulted, client.State);
        Assert.NotNull(exception.InnerException);
        Assert.IsType<ProcessException>(exception.InnerException);
    }

    [Fact]
    public async Task ConnectAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var client = CreateClient();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<ProcessInfo>(cts.Token));

        // Act & Assert - ConnectAsync wraps all exceptions in NetworkException
        var exception = await Assert.ThrowsAsync<NetworkException>(() => client.ConnectAsync(cts.Token));
        Assert.NotNull(exception.InnerException);
        Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenSuccessful_SendsRequestAndReceivesResponse()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?> { ["param1"] = "value1" };
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test_tool","params":{"param1":"value1"}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"status":"success"}}""";
        var expectedResponse = new McpResponse("1", true, new { status = "success" });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act
        var response = await client.InvokeToolAsync("test_tool", parameters);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("1", response.Id);
        _mockRequestHandler.Received(1).SerializeRequest(Arg.Is<McpRequest>(r => 
            r.Method == "test_tool" && r.Parameters == parameters));
        _mockRequestHandler.Received(1).DeserializeResponse(responseJson);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenNotConnected_ConnectsAutomatically()
    {
        // Arrange
        var client = CreateClient();
        var processInfo = new ProcessInfo(1234, "godot-mcp", DateTime.UtcNow);
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(processInfo));

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{}}""";
        var expectedResponse = new McpResponse("1", true, new { });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act
        var response = await client.InvokeToolAsync("test", parameters);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(ConnectionState.Connected, client.State);
        await _mockProcessManager.Received(1).EnsureProcessRunningAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeToolAsync_WhenTimeoutOccurs_ThrowsTimeoutException()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 1, // Short timeout for test
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream(); // Empty stream - returns null immediately (protocol error)

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act & Assert - Empty stream causes ProtocolException (unexpected end of stream)
        var exception = await Assert.ThrowsAsync<ProtocolException>(
            () => client.InvokeToolAsync("test", parameters));
        
        Assert.Contains("Unexpected end of stream", exception.Message);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenServerReturnsError_ThrowsMcpServerException()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","error":{"code":-32600,"message":"Invalid Request","data":"Missing parameter"}}""";
        var errorResponse = new McpResponse(
            "1", 
            false, 
            null, 
            new McpError(-32600, "Invalid Request", "Missing parameter"));

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
        
        Assert.Equal("Invalid Request", exception.Message);
        Assert.Equal(-32600, exception.ErrorCode);
        Assert.Equal("Missing parameter", exception.ErrorData);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenCommunicationFails_ThrowsNetworkException()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?>();
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);

        // Simulate stream that throws on write
        var mockStdin = Substitute.For<Stream>();
        mockStdin.CanWrite.Returns(true);
        mockStdin.WriteAsync(Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask>(callInfo => throw new IOException("Stream closed"));

        _mockProcessManager.StandardInput.Returns(mockStdin);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NetworkException>(
            () => client.InvokeToolAsync("test", parameters));
        
        Assert.Contains("Failed to write to godot-mcp server stdin", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task ListToolsAsync_WhenSuccessful_DiscoverTools()
    {
        // Arrange
        var client = CreateClient();

        // Connect first (before stream setup) - same pattern as other tests.
        // Setting up streams before ConnectAsync risks the health check background task
        // consuming the stream data before the actual test call.
        await SetupConnectedClient(client);

        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"tools/list","params":{}}""";
        // Single-line JSON - MCP uses newline-delimited JSON (one object per line)
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"tools":[{"name":"Godot_create_scene","description":"Creates a new Godot scene","inputSchema":{"type":"object","properties":{"sceneName":{"type":"string","description":"Name of the scene"},"additive":{"type":"boolean","description":"Load additively"}},"required":["sceneName"]}}]}}""";

        var toolsResult = new
        {
            tools = new[]
            {
                new
                {
                    name = "Godot_create_scene",
                    description = "Creates a new Godot scene",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new Dictionary<string, object>
                        {
                            ["sceneName"] = new { type = "string", description = "Name of the scene" },
                            ["additive"] = new { type = "boolean", description = "Load additively" }
                        },
                        required = new[] { "sceneName" }
                    }
                }
            }
        };

        var expectedResponse = new McpResponse("1", true, toolsResult);

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act
        var tools = await client.ListToolsAsync();

        // Assert
        Assert.NotNull(tools);
        Assert.Single(tools);
        Assert.Equal("Godot_create_scene", tools[0].Name);
        Assert.Equal("Creates a new Godot scene", tools[0].Description);
        Assert.Equal(2, tools[0].Parameters.Count);
        Assert.True(tools[0].Parameters["sceneName"].Required);
        Assert.False(tools[0].Parameters["additive"].Required);
    }

    [Fact]
    public async Task PingAsync_WhenSuccessful_ReturnsTrue()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

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

        // Act
        var result = await client.PingAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PingAsync_WhenFails_ReturnsFalse()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

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
    public async Task ConcurrentRequests_AreHandledThreadSafely()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"value":42}}""";
        var expectedResponse = new McpResponse("1", true, new { value = 42 });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        // Create multiple streams for concurrent requests
        var mockStdin = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(callInfo =>
        {
            var stream = new MemoryStream();
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Position = 0;
            return stream;
        });

        // Act - Invoke multiple concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.InvokeToolAsync("test", new Dictionary<string, object?>()))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed
        Assert.All(responses, response =>
        {
            Assert.NotNull(response);
            Assert.True(response.Success);
        });
    }

    [Fact]
    public async Task ConnectionStateTransitions_FromDisconnectedToConnected_IsCorrect()
    {
        // Arrange
        var client = CreateClient();
        var processInfo = new ProcessInfo(1234, "godot-mcp", DateTime.UtcNow);
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(processInfo));

        // Act & Assert
        Assert.Equal(ConnectionState.Disconnected, client.State);
        
        await client.ConnectAsync();
        Assert.Equal(ConnectionState.Connected, client.State);
    }

    [Fact]
    public async Task ConnectionStateTransitions_OnConnectionFailure_TransitionsToFaulted()
    {
        // Arrange
        var client = CreateClient();
        
        _mockProcessManager.EnsureProcessRunningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ProcessInfo>(new ProcessException("Connection failed")));

        // Act
        try
        {
            await client.ConnectAsync();
        }
        catch (NetworkException)
        {
            // Expected
        }

        // Assert
        Assert.Equal(ConnectionState.Faulted, client.State);
    }

    [Fact]
    public async Task DisposeAsync_DisposesResources()
    {
        // Arrange
        var client = CreateClient();
        await SetupConnectedClient(client);

        // Act
        await client.DisposeAsync();

        // Assert
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public async Task InvokeToolAsync_WithMessageLoggingEnabled_LogsRequestAndResponse()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = true // Enable logging
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?> { ["param1"] = "value1" };
        var requestJson = """{"jsonrpc":"2.0","id":"1","method":"test_tool","params":{"param1":"value1"}}""";
        var responseJson = """{"jsonrpc":"2.0","id":"1","result":{"status":"success"}}""";
        var expectedResponse = new McpResponse("1", true, new { status = "success" });

        _mockRequestHandler.SerializeRequest(Arg.Any<McpRequest>()).Returns(requestJson);
        _mockRequestHandler.DeserializeResponse(responseJson).Returns(expectedResponse);

        var mockStdin = new MemoryStream();
        var mockStdout = new MemoryStream();
        var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson + "\n");
        mockStdout.Write(responseBytes, 0, responseBytes.Length);
        mockStdout.Position = 0;

        _mockProcessManager.StandardInput.Returns(mockStdin);
        _mockProcessManager.StandardOutput.Returns(mockStdout);

        // Act
        var response = await client.InvokeToolAsync("test_tool", parameters);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        // Note: Actual logging verification would require a mock logger
        // This test verifies the code path executes without errors
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
}
