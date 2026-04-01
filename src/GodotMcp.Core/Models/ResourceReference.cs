namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot resource reference by path.
/// </summary>
/// <param name="Path">The Godot resource path (for example: res://Scenes/Main.tscn).</param>
public readonly record struct ResourceReference(string Path);
