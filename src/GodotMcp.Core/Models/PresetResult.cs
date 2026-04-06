namespace GodotMcp.Core.Models;

/// <summary>
/// Represents the result of applying a node preset.
/// </summary>
/// <param name="Success">Whether the preset was applied successfully.</param>
/// <param name="Message">Optional result message.</param>
/// <param name="AppliedToPath">Optional target node path where preset was applied.</param>
public sealed record PresetResult(
    bool Success,
    string? Message = null,
    string? AppliedToPath = null);
