namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a UI control node returned by UI MCP commands.
/// </summary>
/// <param name="Name">Control name.</param>
/// <param name="Path">Control node path.</param>
/// <param name="ControlType">Godot control type name.</param>
/// <param name="ParentPath">Optional parent node path.</param>
public sealed record ControlInfo(
    string Name,
    string Path,
    string ControlType,
    string? ParentPath = null);
