namespace GodotMcp.Core.Models;

/// <summary>
/// Process state enumeration
/// </summary>
public enum ProcessState
{
    /// <summary>The process has not been started yet</summary>
    NotStarted,
    /// <summary>The process is in the process of starting up</summary>
    Starting,
    /// <summary>The process is running and ready to accept requests</summary>
    Running,
    /// <summary>The process is in the process of shutting down</summary>
    Stopping,
    /// <summary>The process has been stopped</summary>
    Stopped,
    /// <summary>The process encountered an error and is in a fault state</summary>
    Faulted
}
