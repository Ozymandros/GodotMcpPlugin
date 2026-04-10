namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a scene graph node returned by Scene Graph MCP commands.
/// </summary>
/// <param name="Name">Node name.</param>
/// <param name="Path">Node path within the scene.</param>
/// <param name="Type">Godot node type.</param>
/// <param name="ParentPath">Optional parent node path.</param>
/// <param name="IsInternal">Whether the node is internal/hidden in hierarchy operations.</param>
public sealed record NodeInfo(
    string Name,
    string Path,
    string Type,
    string? ParentPath = null,
    bool IsInternal = false);
