namespace GodotMcp.Core.Models;

/// <summary>
/// Preset command request for applying a node preset.
/// </summary>
public sealed record PresetApplyRequest(
    McpProjectFile Scene,
    string NodePath,
    string PresetName);
