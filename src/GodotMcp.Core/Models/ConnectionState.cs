namespace GodotMcp.Core.Models;

/// <summary>
/// Connection state enumeration
/// </summary>
public enum ConnectionState
{
    /// <summary>Not connected to the godot-mcp server</summary>
    Disconnected,
    /// <summary>Connection attempt is in progress</summary>
    Connecting,
    /// <summary>Successfully connected to the godot-mcp server</summary>
    Connected,
    /// <summary>Attempting to re-establish a lost connection</summary>
    Reconnecting,
    /// <summary>Connection is in an error state and cannot be used</summary>
    Faulted
}
