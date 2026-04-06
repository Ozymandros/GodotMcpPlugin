namespace GodotMcp.Core.Models;

/// <summary>
/// Represents information about a running godot-mcp server process
/// </summary>
/// <param name="ProcessId">The OS process identifier</param>
/// <param name="ExecutablePath">The path or name of the executable that was launched</param>
/// <param name="StartTime">The UTC time at which the process was started</param>
public sealed record ProcessInfo(
    int ProcessId,
    string ExecutablePath,
    DateTime StartTime);
