namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot Vector3 with X, Y, and Z components
/// </summary>
/// <param name="X">The X component of the vector</param>
/// <param name="Y">The Y component of the vector</param>
/// <param name="Z">The Z component of the vector</param>
public readonly record struct Vector3(float X, float Y, float Z);
