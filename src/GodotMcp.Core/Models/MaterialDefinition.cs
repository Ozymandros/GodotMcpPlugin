namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot Material definition with visual properties
/// </summary>
/// <param name="Name">The name of the material</param>
/// <param name="Color">Optional base color of the material</param>
/// <param name="Metallic">Metallic property (0.0 to 1.0), defaults to 0.0 (non-metallic)</param>
/// <param name="Smoothness">Smoothness property (0.0 to 1.0), defaults to 0.5</param>
/// <param name="EmissionColor">Optional emission color for glowing materials</param>
public sealed record MaterialDefinition(
    string Name,
    Color? Color = null,
    float Metallic = 0.0f,
    float Smoothness = 0.5f,
    Color? EmissionColor = null);
