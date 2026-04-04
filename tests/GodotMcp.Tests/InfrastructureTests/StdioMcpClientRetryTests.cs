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
/// Unit tests for <see cref="StdioMcpClient"/> retry logic with mocked MCP session.
/// </summary>
public sealed class StdioMcpClientRetryTests : IAsyncDisposable
{
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly List<StdioMcpClient> _clientsToDispose = new();

    public StdioMcpClientRetryTests()
    {
        _logger = NullLogger<StdioMcpClient>.Instance;
        _loggerFactory = NullLoggerFactory.Instance;
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

    private StdioMcpClient CreateClient(
        IMcpProtocolClientFactory factory,
        GodotMcpOptions options,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        var client = delayAsync is null
            ? new StdioMcpClient(factory, _loggerFactory, _logger, Options.Create(options))
            : new StdioMcpClient(factory, _loggerFactory, _logger, Options.Create(options), delayAsync);
        _clientsToDispose.Add(client);
        return client;
    }

    private static async Task SetupConnectedClient(
        StdioMcpClient client,
        IMcpProtocolClientFactory factory,
        IMcpProtocolSession session)
    {
        factory
            .ConnectAsync(Arg.Any<GodotMcpOptions>(), Arg.Any<ILoggerFactory>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(session));
        await client.ConnectAsync();
    }

    [Fact]
    public async Task InvokeToolAsync_OnNetworkException_RetriesUpToMaxAttempts()
    {
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

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options);
        await SetupConnectedClient(client, mockFactory, mockSession);

        var callCount = 0;
        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return Task.FromException<CallToolResult>(new McpException("transient", new IOException("io")));
            });

        await Assert.ThrowsAsync<NetworkException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

        Assert.Equal(4, callCount);
    }

    [Fact]
    public async Task InvokeToolAsync_OnTimeoutException_RetriesUpToMaxAttempts()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 1,
            MaxRetryAttempts = 2,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options);
        await SetupConnectedClient(client, mockFactory, mockSession);

        var callCount = 0;
        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns<Task<CallToolResult>>(async call =>
            {
                callCount++;
                var ct = call.Arg<CancellationToken>();
                await Task.Delay(TimeSpan.FromHours(1), ct);
                return new CallToolResult();
            });

        await Assert.ThrowsAsync<GodotMcp.Core.Exceptions.TimeoutException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task InvokeToolAsync_OnMcpServerException_DoesNotRetry()
    {
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

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options);
        await SetupConnectedClient(client, mockFactory, mockSession);

        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = "Invalid Request" }]
            }));

        await Assert.ThrowsAsync<McpServerException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

        await mockSession.Received(1).CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeToolAsync_RespectsMaxRetryAttempts()
    {
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

            var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
            var mockSession = Substitute.For<IMcpProtocolSession>();
            var client = CreateClient(mockFactory, options);
            await SetupConnectedClient(client, mockFactory, mockSession);

            var callCount = 0;
            mockSession
                .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    callCount++;
                    return Task.FromException<CallToolResult>(new McpException("transient", new IOException("io")));
                });

            await Assert.ThrowsAsync<NetworkException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

            Assert.Equal(maxRetries + 1, callCount);
        }
    }

    [Fact]
    public async Task InvokeToolAsync_LinearBackoff_CalculatesCorrectDelays()
    {
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

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options, DelayRecorder);
        await SetupConnectedClient(client, mockFactory, mockSession);

        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CallToolResult>(new McpException("x", new IOException("io"))));

        await Assert.ThrowsAsync<NetworkException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

        Assert.Equal(3, observedDelays.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(100), observedDelays[0]);
        Assert.Equal(TimeSpan.FromMilliseconds(200), observedDelays[1]);
        Assert.Equal(TimeSpan.FromMilliseconds(300), observedDelays[2]);
    }

    [Fact]
    public async Task InvokeToolAsync_ExponentialBackoff_CalculatesCorrectDelays()
    {
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

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options, DelayRecorder);
        await SetupConnectedClient(client, mockFactory, mockSession);

        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CallToolResult>(new McpException("x", new IOException("io"))));

        await Assert.ThrowsAsync<NetworkException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

        Assert.Equal(3, observedDelays.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(100), observedDelays[0]);
        Assert.Equal(TimeSpan.FromMilliseconds(200), observedDelays[1]);
        Assert.Equal(TimeSpan.FromMilliseconds(400), observedDelays[2]);
    }

    [Fact]
    public async Task InvokeToolAsync_SucceedsAfterRetry_ReturnsSuccessfully()
    {
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

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options);
        await SetupConnectedClient(client, mockFactory, mockSession);

        var attemptCount = 0;
        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                attemptCount++;
                if (attemptCount < 3)
                {
                    return Task.FromException<CallToolResult>(new McpException("transient", new IOException("io")));
                }

                return Task.FromResult(JsonResult(new { status = "success" }));
            });

        var response = await client.InvokeToolAsync("test", new Dictionary<string, object?>());

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public async Task InvokeToolAsync_WithZeroRetries_FailsImmediately()
    {
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 5,
            RequestTimeoutSeconds = 10,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 10,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        var mockFactory = Substitute.For<IMcpProtocolClientFactory>();
        var mockSession = Substitute.For<IMcpProtocolSession>();
        var client = CreateClient(mockFactory, options);
        await SetupConnectedClient(client, mockFactory, mockSession);

        var callCount = 0;
        mockSession
            .CallToolAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return Task.FromException<CallToolResult>(new McpException("transient", new IOException("io")));
            });

        await Assert.ThrowsAsync<NetworkException>(() => client.InvokeToolAsync("test", new Dictionary<string, object?>()));

        Assert.Equal(1, callCount);
    }
}
