using System.Diagnostics;
using System.Runtime.InteropServices;
using GodotMcp.Core.Utilities;

namespace GodotMcp.Infrastructure.Process;

/// <summary>
/// Manages the godot-mcp process lifecycle
/// </summary>
public sealed partial class ProcessManager : IProcessManager, IAsyncDisposable
{
    private const string GodotMcpPathEnvironmentVariable = "GODOT_MCP_PATH";
    private const string GodotPathEnvironmentVariable = "GODOT_PATH";
    private readonly ILogger<ProcessManager> _logger;
    private readonly GodotMcpOptions _options;
    private readonly SemaphoreSlim _processLock = new(1, 1);
    private System.Diagnostics.Process? _process;
    private ProcessState _state = ProcessState.NotStarted;
    private DateTime _lastActivityTime;

    /// <summary>
    /// Gets the current process state
    /// </summary>
    public ProcessState State => _state;
    
    /// <summary>
    /// Gets the stdin stream for writing requests
    /// </summary>
    public Stream StandardInput => _process?.StandardInput.BaseStream 
        ?? throw new ProcessException("Process not started");
    
    /// <summary>
    /// Gets the stdout stream for reading responses
    /// </summary>
    public Stream StandardOutput => _process?.StandardOutput.BaseStream 
        ?? throw new ProcessException("Process not started");

    /// <summary>
    /// Initializes a new instance of the ProcessManager class
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    /// <param name="options">Configuration options for the Godot MCP plugin</param>
    public ProcessManager(
        ILogger<ProcessManager> logger,
        IOptions<GodotMcpOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        var executableOverride = Environment.GetEnvironmentVariable(GodotMcpPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(executableOverride))
        {
            _options.ExecutablePath = executableOverride;
        }
    }

    /// <summary>
    /// Ensures the godot-mcp process is running, starting it if necessary
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Process information for the running process</returns>
    public async Task<ProcessInfo> EnsureProcessRunningAsync(
        CancellationToken cancellationToken = default)
    {
        await _processLock.WaitAsync(cancellationToken);
        try
        {
            if (_state == ProcessState.Running && _process != null && !_process.HasExited)
            {
                _lastActivityTime = DateTime.UtcNow;
                LogProcessAlreadyRunning(_process.Id);
                
                var startTime = DateTime.UtcNow;
                try { startTime = _process.StartTime; } catch { /* use UtcNow fallback */ }

                return new ProcessInfo(
                    _process.Id,
                    _options.ExecutablePath,
                    startTime);
            }

            return await StartProcessInternalAsync(cancellationToken);
        }
        finally
        {
            _processLock.Release();
        }
    }

    /// <summary>
    /// Starts the godot-mcp process
    /// </summary>
    private async Task<ProcessInfo> StartProcessInternalAsync(
        CancellationToken cancellationToken)
    {
        _state = ProcessState.Starting;
        
        // Sanitize executable path before logging (may contain sensitive paths)
        var sanitizedPath = SanitizeExecutablePath(_options.ExecutablePath);
        LogStartingProcess(sanitizedPath);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _options.ExecutablePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                StandardOutputEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            };

            if (!string.IsNullOrWhiteSpace(_options.GodotExecutablePath)
                && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(GodotPathEnvironmentVariable)))
            {
                startInfo.Environment[GodotPathEnvironmentVariable] = _options.GodotExecutablePath;
            }

            _process = System.Diagnostics.Process.Start(startInfo)
                ?? throw new ProcessException("Failed to start godot-mcp process");

            // Wait for process to be ready (with timeout)
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds));
            
            // Give process time to initialize
            await Task.Delay(500, cts.Token);

            _state = ProcessState.Running;
            _lastActivityTime = DateTime.UtcNow;

            LogProcessStarted(_process.Id);

            // StartTime may throw on some Linux configurations
            var startTime = DateTime.UtcNow;
            try { startTime = _process.StartTime; } catch { /* use UtcNow fallback */ }

            return new ProcessInfo(
                _process.Id,
                _options.ExecutablePath,
                startTime);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _state = ProcessState.Faulted;
            LogProcessStartTimeout(_options.ConnectionTimeoutSeconds);
            throw new Core.Exceptions.TimeoutException(
                "Process start timed out",
                TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds),
                "StartProcess");
        }
        catch (Exception ex)
        {
            _state = ProcessState.Faulted;
            LogProcessStartFailed(ex);
            throw new ProcessException("Failed to start godot-mcp process", ex);
        }
    }

    /// <summary>
    /// Sanitizes executable path for logging by removing potentially sensitive directory information
    /// </summary>
    private static string SanitizeExecutablePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        // If it's just a command name (no path separators), return as-is
        if (!path.Contains(Path.DirectorySeparatorChar) && !path.Contains(Path.AltDirectorySeparatorChar))
        {
            return path;
        }

        // Otherwise, return just the filename to avoid exposing directory structure
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Stops the godot-mcp process gracefully
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    public async Task StopProcessAsync(CancellationToken cancellationToken = default)
    {
        await _processLock.WaitAsync(cancellationToken);
        try
        {
            if (_process == null || _state == ProcessState.Stopped)
            {
                return;
            }

            _state = ProcessState.Stopping;
            
            LogStoppingProcess(_process.Id);

            try
            {
                // Close stdin to signal graceful shutdown
                _process.StandardInput.Close();
                
                // Wait for graceful exit with timeout
                var exitedGracefully = await Task.Run(() => _process.WaitForExit(5000), cancellationToken);
                
                if (!exitedGracefully)
                {
                    LogProcessForceKill(_process.Id);
                    _process.Kill();
                    await Task.Run(() => _process.WaitForExit(), cancellationToken);
                }

                _state = ProcessState.Stopped;
                LogProcessStopped(_process.Id);
            }
            catch (Exception ex)
            {
                _state = ProcessState.Faulted;
                LogProcessStopFailed(_process.Id, ex);
                throw new ProcessException("Failed to stop godot-mcp process", ex, _process.Id);
            }
        }
        finally
        {
            _processLock.Release();
        }
    }

    /// <summary>
    /// Disposes the ProcessManager and cleans up resources
    /// </summary>
    public void Dispose()
    {
        _processLock.Wait();
        try
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.Kill();
                    _process.WaitForExit();
                }
                catch
                {
                    // Ignore errors during disposal
                }
            }
            
            _process?.Dispose();
        }
        finally
        {
            _processLock.Release();
            _processLock.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously disposes the ProcessManager and cleans up resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _processLock.WaitAsync();
        try
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    await StopProcessAsync();
                }
                catch
                {
                    // Ignore errors during disposal
                }
            }
            
            _process?.Dispose();
        }
        finally
        {
            _processLock.Release();
            _processLock.Dispose();
        }
    }

    // LoggerMessage source generator methods for high-performance structured logging
    
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Process already running with PID: {ProcessId}")]
    partial void LogProcessAlreadyRunning(int processId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Starting godot-mcp process: {ExecutablePath}")]
    partial void LogStartingProcess(string executablePath);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "godot-mcp process started successfully. PID: {ProcessId}")]
    partial void LogProcessStarted(int processId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Process start timed out after {TimeoutSeconds} seconds")]
    partial void LogProcessStartTimeout(int timeoutSeconds);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Failed to start godot-mcp process")]
    partial void LogProcessStartFailed(Exception ex);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Stopping godot-mcp process. PID: {ProcessId}")]
    partial void LogStoppingProcess(int processId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Warning,
        Message = "Process did not exit gracefully, forcing termination. PID: {ProcessId}")]
    partial void LogProcessForceKill(int processId);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Information,
        Message = "godot-mcp process stopped successfully. PID: {ProcessId}")]
    partial void LogProcessStopped(int processId);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Error,
        Message = "Error stopping godot-mcp process. PID: {ProcessId}")]
    partial void LogProcessStopFailed(int processId, Exception ex);
}
