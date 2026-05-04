namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a generic Scene Graph command result payload.
/// </summary>
/// <param name="Success">Whether the command succeeded.</param>
/// <param name="Message">Optional command message.</param>
public sealed record SceneCommandResult(
    bool Success,
    string? Message = null);

/// <summary>
/// Represents a signal connection between nodes in a scene.
/// </summary>
public sealed record SceneConnectionInfo(
    string SourceNodePath,
    string Signal,
    string TargetNodePath,
    string Method,
    int Flags);
