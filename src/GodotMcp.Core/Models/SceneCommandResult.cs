namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a generic Scene Graph command result payload.
/// </summary>
/// <param name="Success">Whether the command succeeded.</param>
/// <param name="Message">Optional command message.</param>
public sealed record SceneCommandResult(
    bool Success,
    string? Message = null);
