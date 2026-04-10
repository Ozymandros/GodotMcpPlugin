namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot Component definition with type and properties
/// </summary>
/// <param name="Type">The component type name (e.g., "Rigidbody", "MeshRenderer")</param>
/// <param name="Properties">Optional dictionary of component properties and their values</param>
public sealed record ComponentDefinition(
    string Type,
    IReadOnlyDictionary<string, object?>? Properties = null);
