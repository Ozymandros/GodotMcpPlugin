namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a physics body node returned by Physics MCP commands.
/// </summary>
/// <param name="Name">Physics body node name.</param>
/// <param name="Path">Physics body node path.</param>
/// <param name="BodyType">Godot body type name.</param>
/// <param name="Enabled">Whether the body is enabled.</param>
public sealed record PhysicsBodyInfo(
    string Name,
    string Path,
    string BodyType,
    bool Enabled = true);
