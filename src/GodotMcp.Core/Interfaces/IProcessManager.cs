namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Manages the lifecycle of the godot-mcp server process
/// </summary>
public interface IProcessManager : IDisposable
{
    /// <summary>
    /// Starts the godot-mcp process if not already running
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation with process information</returns>
    Task<ProcessInfo> EnsureProcessRunningAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the godot-mcp process gracefully
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous stop operation</returns>
    Task StopProcessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current process state
    /// </summary>
    ProcessState State { get; }

    /// <summary>
    /// Gets the stdin stream for writing requests
    /// </summary>
    Stream StandardInput { get; }

    /// <summary>
    /// Gets the stdout stream for reading responses
    /// </summary>
    Stream StandardOutput { get; }
}
