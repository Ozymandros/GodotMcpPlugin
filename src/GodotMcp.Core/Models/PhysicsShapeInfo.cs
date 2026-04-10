namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a physics shape returned by Physics MCP commands.
/// </summary>
/// <param name="Name">Shape name.</param>
/// <param name="Path">Shape node path.</param>
/// <param name="ShapeType">Godot shape type name.</param>
/// <param name="Properties">Shape properties.</param>
public sealed record PhysicsShapeInfo(
    string Name,
    string Path,
    string ShapeType,
    IReadOnlyDictionary<string, object?> Properties);
