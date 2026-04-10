namespace GodotMcp.Core.Models;

/// <summary>
/// Preset command request for applying a node preset.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="NodePath">The target node path.</param>
/// <param name="PresetName">The preset name to apply.</param>
public sealed record PresetApplyRequest(
    string ScenePath,
    string NodePath,
    string PresetName);
