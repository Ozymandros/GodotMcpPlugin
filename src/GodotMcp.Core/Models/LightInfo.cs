namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a light node returned by Lighting MCP commands.
/// </summary>
/// <param name="Name">Light node name.</param>
/// <param name="Path">Light node path.</param>
/// <param name="LightType">Godot light type name.</param>
/// <param name="Enabled">Whether the light is enabled.</param>
public sealed record LightInfo(
    string Name,
    string Path,
    string LightType,
    bool Enabled = true);
