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
/// Unit tests for <see cref="StdioMcpClient"/> health monitoring with mocked MCP session.
/// </summary>
public sealed class StdioMcpClientHealthTests : IAsyncDisposable
{
    private readonly IMcpProtocolClientFactory _mockFactory;
    private readonly IMcpProtocolSession _mockSession;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly GodotMcpOptions _options;
    private readonly List<StdioMcpClient> _clientsToDispose = new();

    public StdioMcpClientHealthTests()
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
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 60,
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

    private static CallToolResult JsonResult(object payload) => new()
    {
        Content = [new TextContentBlock { Text = JsonSerializer.Serialize(payload) }]
    };

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

    private async Task SetupConnectedClient(StdioMcpClient client)
    {
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_mockSession));
        await client.ConnectAsync();
    }

    private void SetupSuccessfulToolCall()
    {
        _mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(JsonResult(new { status = "ok" })));
    }

    private void SetupSuccessfulPing()
    {
        _mockSession.PingAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    private async Task PerformSuccessfulRequest(StdioMcpClient client)
    {
        SetupSuccessfulToolCall();
        await client.InvokeToolAsync("test", new Dictionary<string, object?>());
    }

    private async Task<bool> PerformSuccessfulPing(StdioMcpClient client)
    {
        SetupSuccessfulPing();
        return await client.PingAsync();
    }

    [Fact]
    public async Task IsHealthy_WhenConnectionActive_ReturnsTrue()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);

        Assert.True(client.IsHealthy());
    }

    [Fact]
    public void IsHealthy_WhenNotConnected_ReturnsFalse()
    {
        var client = CreateClient();
        Assert.False(client.IsHealthy());
    }

    [Fact]
    public async Task IsHealthy_WhenConnectionLost_ReturnsFalse()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 1,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.False(client.IsHealthy());
    }

    [Fact]
    public async Task IsHealthy_AfterRecentSuccessfulRequest_ReturnsTrue()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);
        await Task.Delay(100);
        await PerformSuccessfulRequest(client);

        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task PingAsync_UpdatesLastSuccessfulRequestTime()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 2,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);
        await Task.Delay(TimeSpan.FromSeconds(1));

        var pingResult = await PerformSuccessfulPing(client);

        Assert.True(pingResult);
        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task PeriodicPing_MaintainsConnectionHealth()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        SetupSuccessfulPing();
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);

        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task IsHealthy_WhenStateIsFaulted_ReturnsFalse()
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

        Assert.False(client.IsHealthy());
        Assert.Equal(ConnectionState.Faulted, client.State);
    }

    [Fact]
    public async Task IsHealthy_WhenStateIsConnecting_ReturnsFalse()
    {
        var client = CreateClient();
        var tcs = new TaskCompletionSource<IMcpProtocolSession>();
        _mockFactory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        var connectTask = client.ConnectAsync();
        await Task.Delay(50);
        Assert.False(client.IsHealthy());

        tcs.SetResult(_mockSession);
        await connectTask;
    }

    [Fact]
    public async Task IsHealthy_AfterDispose_ReturnsFalse()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);

        await client.DisposeAsync();

        Assert.False(client.IsHealthy());
        Assert.Equal(ConnectionState.Disconnected, client.State);
    }

    [Fact]
    public async Task PingAsync_WhenConnectionHealthy_ReturnsTrue()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        SetupSuccessfulPing();

        var result = await PerformSuccessfulPing(client);

        Assert.True(result);
        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task PingAsync_WhenConnectionUnhealthy_ReturnsFalse()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        _mockSession.PingAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new McpException("ping failed"));

        var result = await client.PingAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task IsHealthy_WithMultipleSuccessfulRequests_RemainsHealthy()
    {
        var client = CreateClient();
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();

        for (var i = 0; i < 5; i++)
        {
            await PerformSuccessfulRequest(client);
            await Task.Delay(100);
        }

        Assert.True(client.IsHealthy());
    }

    [Fact]
    public async Task IsHealthy_AfterIdleTimeout_BecomesUnhealthy()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 1,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);
        Assert.True(client.IsHealthy());

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.False(client.IsHealthy());
    }

    [Fact]
    public async Task IsHealthy_AfterIdleTimeout_ThenSuccessfulRequest_BecomesHealthyAgain()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 1,
            EnableMessageLogging = false
        };
        var client = CreateClient(options);
        await SetupConnectedClient(client);
        SetupSuccessfulToolCall();
        await PerformSuccessfulRequest(client);
        Assert.True(client.IsHealthy());

        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.False(client.IsHealthy());

        await PerformSuccessfulRequest(client);
        Assert.True(client.IsHealthy());
    }
}
