namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot resource reference by path.
/// </summary>
/// <param name="Path">Filesystem or project-relative resource path (for example: C:\MyGame\Scenes\Main.tscn).</param>
public readonly record struct ResourceReference(string Path);
