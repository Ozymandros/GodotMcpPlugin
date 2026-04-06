using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Client;
using GodotMcp.Infrastructure.Configuration;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using NSubstitute.ExceptionExtensions;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for <see cref="StdioMcpClient"/> using mocked MCP protocol session/factory (ModelContextProtocol SDK path).
/// </summary>
public sealed class StdioMcpClientTests : IAsyncDisposable
{
    private readonly IMcpProtocolClientFactory _mockFactory;
    private readonly IMcpProtocolSession _mockSession;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly GodotMcpOptions _options;
    private readonly List<StdioMcpClient> _clientsToDispose = new();

    public StdioMcpClientTests()
    {
        _mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        _mockSession = Substitute.For<IMcpProtocolSession>();
        _logger = NullLogger<StdioMcpClient>.Instance;
        _loggerFactory = NullLoggerFactory.Instance;
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
        var client = new StdioMcpClient(
            _mockFactory,
            _loggerFactory,
            _logger,
            Options.Create(options ?? _options));
        _clientsToDispose.Add(client);
        return client;
    }

    private static CallToolResult JsonResult(object payload) => new()
    {
        Content = [new TextContentBlock { Text = JsonSerializer.Serialize(payload) }]
    };

    [Fact]
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        var client = CreateClient();
        Assert.NotNull(client);
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public void State_InitialState_IsDisconnected()
    {
        var client = CreateClient();
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public async Task ConnectAsync_WhenSuccessful_EstablishesConnection()
    {
        var client = CreateClient();
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_mockSession));

        await client.ConnectAsync();

        Assert.Equal(ConnectionState.Connected, client.State);
        await _mockFactory.Received(1).ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConnectAsync_WhenFactoryFails_ThrowsNetworkException()
    {
        var client = CreateClient();
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns<Task<IMcpProtocolSession>>(_ => throw new InvalidOperationException("spawn failed"));

        var exception = await Assert.ThrowsAsync<NetworkException>(() => client.ConnectAsync());
        Assert.Contains("Failed to connect to godot-mcp server", exception.Message);
        Assert.Equal(ConnectionState.Faulted, client.State);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task ConnectAsync_WithCancellation_ThrowsNetworkExceptionWithCanceledInner()
    {
        var client = CreateClient();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), cts.Token)
            .Returns<Task<IMcpProtocolSession>>(_ => Task.FromCanceled<IMcpProtocolSession>(cts.Token));

        var exception = await Assert.ThrowsAsync<NetworkException>(() => client.ConnectAsync(cts.Token));
        Assert.NotNull(exception.InnerException);
        Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenSuccessful_ReturnsDeserializedResult()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        var parameters = new Dictionary<string, object?> { ["param1"] = "value1" };
        _mockSession
            .CallToolAsync("test_tool", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new { status = "success" })));

        var response = await client.InvokeToolAsync("test_tool", parameters);

        Assert.NotNull(response);
        Assert.True(response.Success);
        await _mockSession.Received(1).CallToolAsync(
            "test_tool",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => d["param1"]!.Equals("value1")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeToolAsync_WhenNotConnected_ConnectsAutomatically()
    {
        var client = CreateClient();
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_mockSession));

        _mockSession
            .CallToolAsync("test", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new { })));

        var response = await client.InvokeToolAsync("test", new Dictionary<string, object?>());

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(ConnectionState.Connected, client.State);
        await _mockFactory.Received(1).ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeToolAsync_WhenTimeoutOccurs_ThrowsTimeoutException()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 1,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns<Task<CallToolResult>>(async call =>
            {
                var ct = call.Arg<CancellationToken>();
                await Task.Delay(TimeSpan.FromHours(1), ct);
                return new CallToolResult();
            });

        var exception = await Assert.ThrowsAsync<GodotMcp.Core.Exceptions.TimeoutException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));
        Assert.Contains("Request timed out", exception.Message);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenToolReturnsError_ThrowsMcpServerException()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = "Invalid Request" }]
            }));

        var exception = await Assert.ThrowsAsync<McpServerException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));
        Assert.Equal("Invalid Request", exception.Message);
        Assert.Equal(-32000, exception.ErrorCode);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenMcpExceptionThrown_ThrowsNetworkException()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new McpException("rpc failed", new IOException("Stream closed")));

        var exception = await Assert.ThrowsAsync<NetworkException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));
        Assert.Contains("Error invoking tool", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task ListToolsAsync_WhenSuccessful_DiscoverTools()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        var definitions = new List<McpToolDefinition>
        {
            new(
                "Godot_create_scene",
                "Creates a new Godot scene",
                new Dictionary<string, McpParameterDefinition>(StringComparer.Ordinal)
                {
                    ["sceneName"] = new McpParameterDefinition("sceneName", "string", "Name of the scene", true),
                    ["additive"] = new McpParameterDefinition("additive", "boolean", "Load additively", false)
                })
        };

        _mockSession.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IReadOnlyList<McpToolDefinition>>(definitions));

        var tools = await client.ListToolsAsync();

        Assert.NotNull(tools);
        Assert.Single(tools);
        Assert.Equal("Godot_create_scene", tools[0].Name);
        Assert.Equal("Creates a new Godot scene", tools[0].Description);
        Assert.Equal(2, tools[0].Parameters.Count);
        Assert.True(tools[0].Parameters["sceneName"].Required);
        Assert.False(tools[0].Parameters["additive"].Required);
    }

    [Fact]
    public async Task SceneListNodesAsync_WhenSuccessful_ReturnsTypedNodes()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync("scene.list_nodes", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new[]
            {
                new { name = "Root", path = ".", type = "Node3D", parentPath = (string?)null, isInternal = false },
                new { name = "Camera", path = "./Camera", type = "Camera3D", parentPath = (string?)".", isInternal = false }
            })));

        var result = await client.SceneListNodesAsync(new SceneListNodesRequest("res://scenes/main.tscn"));

        Assert.Equal(2, result.Count);
        Assert.Equal("Root", result[0].Name);
        Assert.Equal("Camera3D", result[1].Type);
        await _mockSession.Received(1).CallToolAsync(
            "scene.list_nodes",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                d.ContainsKey("scenePath") &&
                string.Equals(d["scenePath"] as string, "res://scenes/main.tscn", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SceneAddNodeAsync_WhenSuccessful_ReturnsTypedNode()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync("scene.add_node", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new
            {
                name = "Player",
                path = "./Player",
                type = "CharacterBody3D",
                parentPath = ".",
                isInternal = false
            })));

        var request = new SceneAddNodeRequest(
            "res://scenes/main.tscn",
            ".",
            "Player",
            "CharacterBody3D");

        var result = await client.SceneAddNodeAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Player", result!.Name);
        Assert.Equal("CharacterBody3D", result.Type);
        await _mockSession.Received(1).CallToolAsync(
            "scene.add_node",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["scenePath"], "res://scenes/main.tscn") &&
                Equals(d["parentPath"], ".") &&
                Equals(d["nodeName"], "Player") &&
                Equals(d["nodeType"], "CharacterBody3D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SceneGetNodePropertiesAsync_WhenSuccessful_ReturnsTypedProperties()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync("scene.get_node_properties", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = "[{\"name\":\"fov\",\"type\":\"float\",\"value\":70,\"readOnly\":false},{\"name\":\"current\",\"type\":\"bool\",\"value\":true,\"readOnly\":false}]"
                }]
            }));

        var result = await client.SceneGetNodePropertiesAsync(
            new SceneGetNodePropertiesRequest("res://scenes/main.tscn", "./Camera"));

        Assert.Equal(2, result.Count);
        Assert.Equal("fov", result[0].Name);
        Assert.Equal("float", result[0].Type);
        Assert.True(result[0].Value.TryGetInt32(out var fov));
        Assert.Equal(70, fov);
        Assert.Equal("current", result[1].Name);
        Assert.Equal(JsonValueKind.True, result[1].Value.ValueKind);
    }

    [Fact]
    public async Task SceneMoveNodeAsync_WhenSuccessful_ReturnsCommandResult()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync("scene.move_node", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new { success = true, message = "Moved" })));

        var result = await client.SceneMoveNodeAsync(
            new SceneMoveNodeRequest("res://scenes/main.tscn", "./Camera", "./Rig", 0));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Moved", result.Message);
    }

    [Fact]
    public async Task PingAsync_WhenSuccessful_ReturnsTrue()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        _mockSession.PingAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await client.PingAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task PingAsync_WhenFails_ReturnsFalse()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        _mockSession.PingAsync(Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new McpException("ping failed"));

        var result = await client.PingAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task ConcurrentRequests_AreHandledThreadSafely()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        _mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new { value = 42 })));

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.InvokeToolAsync("test", new Dictionary<string, object?>()))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        Assert.All(responses, response =>
        {
            Assert.NotNull(response);
            Assert.True(response.Success);
        });
    }

    [Fact]
    public async Task ConnectionStateTransitions_FromDisconnectedToConnected_IsCorrect()
    {
        var client = CreateClient();
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_mockSession));

        Assert.Equal(ConnectionState.Disconnected, client.State);
        await client.ConnectAsync();
        Assert.Equal(ConnectionState.Connected, client.State);
    }

    [Fact]
    public async Task ConnectionStateTransitions_OnConnectionFailure_TransitionsToFaulted()
    {
        var client = CreateClient();
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns<Task<IMcpProtocolSession>>(_ => throw new InvalidOperationException("Connection failed"));

        try
        {
            await client.ConnectAsync();
        }
        catch (NetworkException)
        {
            // Expected
        }

        Assert.Equal(ConnectionState.Faulted, client.State);
    }

    [Fact]
    public async Task DisposeAsync_DisposesResources()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);

        await client.DisposeAsync();

        Assert.Equal(ConnectionState.Disconnected, client.State);
        await _mockSession.Received(1).DisposeAsync();
    }

    private async Task SetupConnectedClient(StdioMcpClient client)
    {
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_mockSession));

        await client.ConnectAsync();
    }
}
