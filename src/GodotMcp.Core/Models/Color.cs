namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot Color with RGBA components
/// </summary>
/// <param name="R">The red component (0.0 to 1.0)</param>
/// <param name="G">The green component (0.0 to 1.0)</param>
/// <param name="B">The blue component (0.0 to 1.0)</param>
/// <param name="A">The alpha component (0.0 to 1.0), defaults to 1.0 (fully opaque)</param>
public readonly record struct Color(float R, float G, float B, float A = 1.0f);
