using System.IO.Pipelines;
using GodotMcp.Core.Utilities;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Implements MCP client using stdio transport
/// </summary>
public sealed partial class StdioMcpClient : IMcpClient
{
    private readonly IProcessManager _processManager;
    private readonly IRequestHandler _requestHandler;
    private readonly ILogger<StdioMcpClient> _logger;
    private readonly GodotMcpOptions _options;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private ConnectionState _state = ConnectionState.Disconnected;
    private int _requestIdCounter = 0;
    private DateTime _lastSuccessfulRequestTime = DateTime.MinValue;
    private bool _lastHealthStatus = false;
    private PeriodicTimer? _healthCheckTimer;
    private Task? _healthCheckTask;
    private CancellationTokenSource? _healthCheckCts;

    /// <summary>
    /// Gets the current connection state
    /// </summary>
    public ConnectionState State => _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="StdioMcpClient"/> class
    /// </summary>
    /// <param name="processManager">The process manager for managing the godot-mcp process</param>
    /// <param name="requestHandler">The request handler for serialization/deserialization</param>
    /// <param name="logger">The logger for diagnostic output</param>
    /// <param name="options">The configuration options</param>
    public StdioMcpClient(
        IProcessManager processManager,
        IRequestHandler requestHandler,
        ILogger<StdioMcpClient> logger,
        IOptions<GodotMcpOptions> options,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        _processManager = processManager;
        _requestHandler = requestHandler;
        _logger = logger;
        _options = options.Value;
        _delayAsync = delayAsync ?? Task.Delay;
    }

    /// <summary>
    /// Connects to the godot-mcp server and starts health monitoring.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _state = ConnectionState.Connecting;
        
        try
        {
            LogConnecting();
            await _processManager.EnsureProcessRunningAsync(cancellationToken);
            _state = ConnectionState.Connected;
            _lastSuccessfulRequestTime = DateTime.UtcNow;
            
            // Start health check monitoring
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

    /// <summary>
    /// Invokes an MCP tool with retry logic and returns the response.
    /// </summary>
    public async Task<McpResponse> InvokeToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);

        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= _options.MaxRetryAttempts)
        {
            try
            {
                return await InvokeToolInternalAsync(toolName, parameters, cancellationToken);
            }
            catch (NetworkException ex) when (attempt < _options.MaxRetryAttempts)
            {
                lastException = ex;
                attempt++;
                var delay = CalculateRetryDelay(attempt);
                LogRetryAttempt(toolName, attempt, _options.MaxRetryAttempts, ex.Message, (int)delay.TotalMilliseconds);
                await _delayAsync(delay, cancellationToken);
            }
            catch (Core.Exceptions.TimeoutException ex) when (attempt < _options.MaxRetryAttempts)
            {
                lastException = ex;
                attempt++;
                var delay = CalculateRetryDelay(attempt);
                LogRetryAttempt(toolName, attempt, _options.MaxRetryAttempts, ex.Message, (int)delay.TotalMilliseconds);
                await _delayAsync(delay, cancellationToken);
            }
            catch (ProtocolException)
            {
                // Protocol errors are not transient - do not retry
                throw;
            }
            catch (McpServerException)
            {
                // Server errors are not transient - do not retry
                throw;
            }
        }

        // If we exhausted all retries, throw the last exception
        throw lastException ?? new NetworkException($"Failed to invoke tool: {toolName} after {_options.MaxRetryAttempts} retries");
    }

    /// <summary>
    /// Internal method that performs the actual tool invocation without retry logic
    /// </summary>
    private async Task<McpResponse> InvokeToolInternalAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        var requestId = Interlocked.Increment(ref _requestIdCounter).ToString();
        var request = new McpRequest(requestId, toolName, parameters);

        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            LogInvokingTool(toolName, requestId);

            var startTime = DateTime.UtcNow;

            // Serialize and send request
            var requestJson = _requestHandler.SerializeRequest(request);

            if (_options.EnableMessageLogging)
            {
                // Sanitize request before logging
                var sanitizedRequest = LogSanitizer.SanitizeString(requestJson);
                LogRequest(sanitizedRequest);
            }

            await WriteLineAsync(requestJson, cancellationToken);

            // Read and deserialize response
            var responseJson = await ReadLineAsync(cancellationToken);

            if (_options.EnableMessageLogging)
            {
                // Sanitize response before logging
                var sanitizedResponse = LogSanitizer.SanitizeString(responseJson);
                LogResponse(sanitizedResponse);
            }

            var response = _requestHandler.DeserializeResponse(responseJson);

            var duration = DateTime.UtcNow - startTime;

            LogToolInvocationCompleted(toolName, response.Success, duration.TotalMilliseconds);

            if (!response.Success && response.Error != null)
            {
                throw new McpServerException(
                    response.Error.Message,
                    response.Error.Code,
                    response.Error.Data);
            }

            // Track successful request time
            _lastSuccessfulRequestTime = DateTime.UtcNow;

            return response;
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

    /// <summary>
    /// Calculates the retry delay based on the configured backoff strategy
    /// </summary>
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


    /// <summary>
    /// Lists available MCP tools from the server.
    /// </summary>
    public async Task<IReadOnlyList<McpToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);

        var response = await InvokeToolAsync(
            "tools/list",
            new Dictionary<string, object?>(),
            cancellationToken);

        // Parse tool definitions from response
        var tools = ParseToolDefinitions(response.Result);
        
        LogToolsDiscovered(tools.Count);
        
        return tools;
    }

    /// <summary>
    /// Pings the MCP server to check connectivity.
    /// </summary>
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await InvokeToolAsync(
                "ping",
                new Dictionary<string, object?>(),
                cancellationToken);
            
            if (response.Success)
            {
                _lastSuccessfulRequestTime = DateTime.UtcNow;
            }
            
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks the health of the connection
    /// </summary>
    /// <returns>True if the connection is healthy, false otherwise</returns>
    public bool IsHealthy()
    {
        if (_state != ConnectionState.Connected)
        {
            return false;
        }

        // Check if we've had a successful request recently
        var timeSinceLastSuccess = DateTime.UtcNow - _lastSuccessfulRequestTime;
        var maxIdleTime = TimeSpan.FromSeconds(_options.MaxIdleTimeSeconds);

        return timeSinceLastSuccess < maxIdleTime;
    }

    /// <summary>
    /// Starts periodic health check monitoring
    /// </summary>
    private void StartHealthCheckMonitoring()
    {
        // Stop any existing health check
        StopHealthCheckMonitoring();

        // Create a new cancellation token source for the health check
        _healthCheckCts = new CancellationTokenSource();

        // Start periodic health check (every 30 seconds)
        var interval = TimeSpan.FromSeconds(30);
        _healthCheckTimer = new PeriodicTimer(interval);
        _healthCheckTask = Task.Run(async () => await HealthCheckLoopAsync(_healthCheckCts.Token));

        LogHealthCheckStarted((int)interval.TotalSeconds);
    }

    /// <summary>
    /// Stops health check monitoring
    /// </summary>
    private void StopHealthCheckMonitoring()
    {
        _healthCheckCts?.Cancel();
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;
        _healthCheckTask = null;
        _healthCheckCts?.Dispose();
        _healthCheckCts = null;
    }

    /// <summary>
    /// Health check loop that runs periodically
    /// </summary>
    private async Task HealthCheckLoopAsync(CancellationToken cancellationToken)
    {
        if (_healthCheckTimer == null)
        {
            return;
        }

        try
        {
            while (await _healthCheckTimer.WaitForNextTickAsync(cancellationToken))
            {
                var isHealthy = await PingAsync(cancellationToken);
                
                // Log health status changes
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
            // Expected when cancellation is requested
            LogHealthCheckStopped();
        }
        catch (Exception ex)
        {
            LogHealthCheckError(ex);
        }
    }

    /// <summary>
    /// Ensures the client is connected, connecting if necessary
    /// </summary>
    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_state != ConnectionState.Connected)
        {
            await ConnectAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Writes a JSON line to the process stdin
    /// </summary>
    private async Task WriteLineAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var stdin = _processManager.StandardInput;
            // Use explicit \n (not Environment.NewLine) for cross-platform MCP protocol compatibility.
            // StreamWriter.WriteLineAsync uses Environment.NewLine which writes \r\n on Windows,
            // but the MCP stdio protocol expects newline-delimited JSON with \n only.
            var bytes = System.Text.Encoding.UTF8.GetBytes(message + "\n");
            await stdin.WriteAsync(bytes, cancellationToken);
            await stdin.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogWriteError(ex);
            throw new NetworkException("Failed to write to godot-mcp server stdin", ex, "stdio://godot-mcp");
        }
    }

    /// <summary>
    /// Reads a JSON line from the process stdout with timeout
    /// </summary>
    private async Task<string> ReadLineAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

        try
        {
            var stdout = _processManager.StandardOutput;
            // leaveOpen: true so we don't close the underlying process stream
            var reader = new StreamReader(stdout, leaveOpen: true);
            
            var line = await reader.ReadLineAsync(cts.Token);
            
            if (line == null)
            {
                throw new ProtocolException("Unexpected end of stream from godot-mcp server");
            }

            return line;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            LogReadTimeout();
            throw new Core.Exceptions.TimeoutException(
                "Request timed out",
                TimeSpan.FromSeconds(_options.RequestTimeoutSeconds),
                "ReadResponse");
        }
        catch (Exception ex) when (ex is not GodotMcpException)
        {
            LogReadError(ex);
            throw new NetworkException("Failed to read from godot-mcp server stdout", ex, "stdio://godot-mcp");
        }
    }

    /// <summary>
    /// Parses tool definitions from the MCP response
    /// </summary>
    private IReadOnlyList<McpToolDefinition> ParseToolDefinitions(object? result)
    {
        if (result == null)
        {
            return Array.Empty<McpToolDefinition>();
        }

        try
        {
            // The result should be a JSON element containing an array of tools
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            var document = System.Text.Json.JsonDocument.Parse(json);
            
            if (!document.RootElement.TryGetProperty("tools", out var toolsElement))
            {
                LogToolsParseWarning("Missing 'tools' property in response");
                return Array.Empty<McpToolDefinition>();
            }

            var tools = new List<McpToolDefinition>();
            
            foreach (var toolElement in toolsElement.EnumerateArray())
            {
                var name = toolElement.GetProperty("name").GetString() ?? string.Empty;
                var description = toolElement.TryGetProperty("description", out var descProp) 
                    ? descProp.GetString() ?? string.Empty 
                    : string.Empty;

                var parameters = new Dictionary<string, McpParameterDefinition>();
                
                if (toolElement.TryGetProperty("inputSchema", out var schemaElement) &&
                    schemaElement.TryGetProperty("properties", out var propsElement))
                {
                    foreach (var prop in propsElement.EnumerateObject())
                    {
                        var paramName = prop.Name;
                        var paramType = prop.Value.TryGetProperty("type", out var typeProp)
                            ? typeProp.GetString() ?? "string"
                            : "string";
                        var paramDesc = prop.Value.TryGetProperty("description", out var paramDescProp)
                            ? paramDescProp.GetString()
                            : null;

                        var required = false;
                        if (schemaElement.TryGetProperty("required", out var requiredElement))
                        {
                            foreach (var reqName in requiredElement.EnumerateArray())
                            {
                                if (reqName.GetString() == paramName)
                                {
                                    required = true;
                                    break;
                                }
                            }
                        }

                        parameters[paramName] = new McpParameterDefinition(
                            paramName,
                            paramType,
                            paramDesc,
                            required);
                    }
                }

                tools.Add(new McpToolDefinition(name, description, parameters));
            }

            return tools;
        }
        catch (Exception ex)
        {
            LogToolsParseError(ex);
            throw new ProtocolException("Failed to parse tool definitions", ex);
        }
    }

    /// <summary>
    /// Disposes client resources and stops health monitoring.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        StopHealthCheckMonitoring();
        _state = ConnectionState.Disconnected;
        _requestLock.Dispose();
        await Task.CompletedTask;
    }

    // Logging methods using LoggerMessage source generator
    [LoggerMessage(Level = LogLevel.Information, Message = "Connecting to godot-mcp server via stdio")]
    partial void LogConnecting();

    [LoggerMessage(Level = LogLevel.Information, Message = "Connected to godot-mcp server via stdio")]
    partial void LogConnected();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to connect to godot-mcp server")]
    partial void LogConnectionFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invoking tool: {ToolName} with request ID: {RequestId}")]
    partial void LogInvokingTool(string toolName, string requestId);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Request: {Request}")]
    partial void LogRequest(string request);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Response: {Response}")]
    partial void LogResponse(string response);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tool invocation completed: {ToolName}, Success: {Success}, Duration: {Duration}ms")]
    partial void LogToolInvocationCompleted(string toolName, bool success, double duration);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error invoking tool: {ToolName}")]
    partial void LogToolInvocationError(Exception ex, string toolName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Discovered {ToolCount} tools from godot-mcp server")]
    partial void LogToolsDiscovered(int toolCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to write to godot-mcp server stdin")]
    partial void LogWriteError(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request timed out while reading from godot-mcp server stdout")]
    partial void LogReadTimeout();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to read from godot-mcp server stdout")]
    partial void LogReadError(Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to parse tool definitions: {Reason}")]
    partial void LogToolsParseWarning(string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to parse tool definitions")]
    partial void LogToolsParseError(Exception ex);

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
