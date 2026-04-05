using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// MCP client using stdio transport and the official Model Context Protocol .NET SDK (compatible with GodotMCP.Server 1.2.x).
/// </summary>
public sealed partial class StdioMcpClient : IMcpClient
{
    private readonly IMcpProtocolClientFactory _protocolFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly GodotMcpOptions _options;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private IMcpProtocolSession? _session;
    private ConnectionState _state = ConnectionState.Disconnected;
    private DateTime _lastSuccessfulRequestTime = DateTime.MinValue;
    private bool _lastHealthStatus;
    private PeriodicTimer? _healthCheckTimer;
    private Task? _healthCheckTask;
    private CancellationTokenSource? _healthCheckCts;

    /// <inheritdoc />
    public ConnectionState State => _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="StdioMcpClient"/> class.
    /// </summary>
    public StdioMcpClient(
        IMcpProtocolClientFactory protocolFactory,
        ILoggerFactory loggerFactory,
        ILogger<StdioMcpClient> logger,
        IOptions<GodotMcpOptions> options,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        _protocolFactory = protocolFactory;
        _loggerFactory = loggerFactory;
        _logger = logger;
        _options = options.Value;
        _delayAsync = delayAsync ?? Task.Delay;
    }

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _state = ConnectionState.Connecting;

        try
        {
            LogConnecting();
            await DisposeSessionAsync().ConfigureAwait(false);

            _session = await _protocolFactory
                .ConnectAsync(_options, _loggerFactory, cancellationToken)
                .ConfigureAwait(false);

            _state = ConnectionState.Connected;
            _lastSuccessfulRequestTime = DateTime.UtcNow;
            StartHealthCheckMonitoring();
            LogConnected();
        }
        catch (Exception ex)
        {
            _state = ConnectionState.Faulted;
            LogConnectionFailed(ex);
            throw new NetworkException("Failed to connect to godot-mcp server", ex, "stdio://godot-mcp");
        }
    }

    /// <inheritdoc />
    public async Task<McpResponse> InvokeToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= _options.MaxRetryAttempts)
        {
            try
            {
                return await InvokeToolInternalAsync(toolName, parameters, cancellationToken).ConfigureAwait(false);
            }
            catch (NetworkException ex) when (attempt < _options.MaxRetryAttempts)
            {
                lastException = ex;
                attempt++;
                var delay = CalculateRetryDelay(attempt);
                LogRetryAttempt(toolName, attempt, _options.MaxRetryAttempts, ex.Message, (int)delay.TotalMilliseconds);
                await _delayAsync(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (Core.Exceptions.TimeoutException ex) when (attempt < _options.MaxRetryAttempts)
            {
                lastException = ex;
                attempt++;
                var delay = CalculateRetryDelay(attempt);
                LogRetryAttempt(toolName, attempt, _options.MaxRetryAttempts, ex.Message, (int)delay.TotalMilliseconds);
                await _delayAsync(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (ProtocolException)
            {
                throw;
            }
            catch (McpServerException)
            {
                throw;
            }
        }

        throw lastException ?? new NetworkException($"Failed to invoke tool: {toolName} after {_options.MaxRetryAttempts} retries");
    }

    private async Task<McpResponse> InvokeToolInternalAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        if (_session is null)
        {
            throw new NetworkException("MCP session is not connected", "stdio://godot-mcp");
        }

        await _requestLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var requestId = Guid.NewGuid().ToString("N");
            LogInvokingTool(toolName, requestId);
            var startTime = DateTime.UtcNow;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

            CallToolResult callResult;
            try
            {
                callResult = await _session.CallToolAsync(toolName, parameters, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                LogReadTimeout();
                throw new Core.Exceptions.TimeoutException(
                    "Request timed out",
                    TimeSpan.FromSeconds(_options.RequestTimeoutSeconds),
                    "CallTool");
            }
            catch (McpException ex)
            {
                LogToolInvocationError(ex, toolName);
                throw new NetworkException($"Error invoking tool: {toolName}", ex, "stdio://godot-mcp");
            }

            var duration = DateTime.UtcNow - startTime;
            if (callResult.IsError == true)
            {
                var message = ExtractToolErrorMessage(callResult);
                LogToolInvocationCompleted(toolName, false, duration.TotalMilliseconds);
                throw new McpServerException(message, -32000, null);
            }

            var resultObject = CallToolResultToObject(callResult);
            LogToolInvocationCompleted(toolName, true, duration.TotalMilliseconds);
            _lastSuccessfulRequestTime = DateTime.UtcNow;
            return new McpResponse(requestId, true, resultObject);
        }
        catch (Exception ex) when (ex is not GodotMcpException)
        {
            LogToolInvocationError(ex, toolName);
            throw new NetworkException($"Error invoking tool: {toolName}", ex, "stdio://godot-mcp");
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private static object? CallToolResultToObject(CallToolResult result)
    {
        if (result.StructuredContent is JsonElement structured
            && structured.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
        {
            return JsonSerializer.Deserialize<object>(structured.GetRawText());
        }

        if (result.Content is null || result.Content.Count == 0)
        {
            return null;
        }

        foreach (var block in result.Content)
        {
            if (block is TextContentBlock text && !string.IsNullOrEmpty(text.Text))
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(text.Text);
                }
                catch (JsonException)
                {
                    return text.Text;
                }
            }
        }

        return null;
    }

    private static string ExtractToolErrorMessage(CallToolResult result)
    {
        if (result.Content is not null)
        {
            foreach (var block in result.Content)
            {
                if (block is TextContentBlock text && !string.IsNullOrWhiteSpace(text.Text))
                {
                    return text.Text;
                }
            }
        }

        return "Tool execution failed";
    }

    private TimeSpan CalculateRetryDelay(int attempt)
    {
        var delayMs = _options.BackoffStrategy switch
        {
            BackoffStrategy.Linear => _options.InitialRetryDelayMs * attempt,
            BackoffStrategy.Exponential => _options.InitialRetryDelayMs * (int)Math.Pow(2, attempt - 1),
            _ => _options.InitialRetryDelayMs
        };

        return TimeSpan.FromMilliseconds(delayMs);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<McpToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

        if (_session is null)
        {
            return Array.Empty<McpToolDefinition>();
        }

        await _requestLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

            try
            {
                var tools = await _session.ListToolsAsync(cts.Token).ConfigureAwait(false);
                LogToolsDiscovered(tools.Count);
                _lastSuccessfulRequestTime = DateTime.UtcNow;
                return tools;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                LogReadTimeout();
                throw new Core.Exceptions.TimeoutException(
                    "Request timed out",
                    TimeSpan.FromSeconds(_options.RequestTimeoutSeconds),
                    "ListTools");
            }
            catch (McpException ex)
            {
                throw new NetworkException("Failed to list tools from godot-mcp server", ex, "stdio://godot-mcp");
            }
        }
        finally
        {
            _requestLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            if (_session is null)
            {
                return false;
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

            await _session.PingAsync(cts.Token).ConfigureAwait(false);
            _lastSuccessfulRequestTime = DateTime.UtcNow;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool IsHealthy()
    {
        if (_state != ConnectionState.Connected)
        {
            return false;
        }

        var timeSinceLastSuccess = DateTime.UtcNow - _lastSuccessfulRequestTime;
        var maxIdleTime = TimeSpan.FromSeconds(_options.MaxIdleTimeSeconds);
        return timeSinceLastSuccess < maxIdleTime;
    }

    private void StartHealthCheckMonitoring()
    {
        StopHealthCheckMonitoring();
        _healthCheckCts = new CancellationTokenSource();
        var interval = TimeSpan.FromSeconds(30);
        _healthCheckTimer = new PeriodicTimer(interval);
        _healthCheckTask = Task.Run(async () => await HealthCheckLoopAsync(_healthCheckCts.Token).ConfigureAwait(false));
        LogHealthCheckStarted((int)interval.TotalSeconds);
    }

    private void StopHealthCheckMonitoring()
    {
        _healthCheckCts?.Cancel();
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;
        _healthCheckTask = null;
        _healthCheckCts?.Dispose();
        _healthCheckCts = null;
    }

    private async Task HealthCheckLoopAsync(CancellationToken cancellationToken)
    {
        if (_healthCheckTimer is null)
        {
            return;
        }

        try
        {
            while (await _healthCheckTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                var isHealthy = await PingAsync(cancellationToken).ConfigureAwait(false);
                if (isHealthy != _lastHealthStatus)
                {
                    _lastHealthStatus = isHealthy;
                    if (isHealthy)
                    {
                        LogHealthStatusChanged("healthy");
                    }
                    else
                    {
                        LogHealthStatusChanged("unhealthy");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            LogHealthCheckStopped();
        }
        catch (Exception ex)
        {
            LogHealthCheckError(ex);
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_state != ConnectionState.Connected || _session is null)
        {
            await ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DisposeSessionAsync()
    {
        if (_session is null)
        {
            return;
        }

        try
        {
            await _session.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Best-effort cleanup
        }
        finally
        {
            _session = null;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        StopHealthCheckMonitoring();
        _state = ConnectionState.Disconnected;
        await DisposeSessionAsync().ConfigureAwait(false);
        _requestLock.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Connecting to godot-mcp server via stdio")]
    partial void LogConnecting();

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected to godot-mcp server via stdio")]
    partial void LogConnected();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to connect to godot-mcp server")]
    partial void LogConnectionFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invoking tool: {ToolName} with request ID: {RequestId}")]
    partial void LogInvokingTool(string toolName, string requestId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tool invocation completed: {ToolName}, Success: {Success}, Duration: {Duration}ms")]
    partial void LogToolInvocationCompleted(string toolName, bool success, double duration);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error invoking tool: {ToolName}")]
    partial void LogToolInvocationError(Exception ex, string toolName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Discovered {ToolCount} tools from godot-mcp server")]
    partial void LogToolsDiscovered(int toolCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request timed out while reading from godot-mcp server stdout")]
    partial void LogReadTimeout();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Retrying tool invocation: {ToolName}, Attempt {Attempt}/{MaxAttempts}, Reason: {Reason}, Delay: {DelayMs}ms")]
    partial void LogRetryAttempt(string toolName, int attempt, int maxAttempts, string reason, int delayMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Health check monitoring started with interval: {IntervalSeconds} seconds")]
    partial void LogHealthCheckStarted(int intervalSeconds);

    [LoggerMessage(Level = LogLevel.Information, Message = "Health check monitoring stopped")]
    partial void LogHealthCheckStopped();

    [LoggerMessage(Level = LogLevel.Information, Message = "Connection health status changed to: {Status}")]
    partial void LogHealthStatusChanged(string status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error in health check loop")]
    partial void LogHealthCheckError(Exception ex);
}
