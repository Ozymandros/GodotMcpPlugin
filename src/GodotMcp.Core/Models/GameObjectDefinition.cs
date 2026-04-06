namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot GameObject definition with transform and components
/// </summary>
/// <param name="Name">The name of the GameObject</param>
/// <param name="Position">Optional position in world space (defaults to origin if not specified)</param>
/// <param name="Rotation">Optional rotation in Euler angles (defaults to identity if not specified)</param>
/// <param name="Scale">Optional scale (defaults to (1,1,1) if not specified)</param>
/// <param name="Components">Optional collection of components to attach to the GameObject</param>
public sealed record GameObjectDefinition(
    string Name,
    Vector3? Position = null,
    Vector3? Rotation = null,
    Vector3? Scale = null,
    IReadOnlyList<ComponentDefinition>? Components = null);
