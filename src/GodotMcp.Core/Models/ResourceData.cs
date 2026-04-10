namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a resource payload with typed metadata and property values.
/// </summary>
/// <param name="Path">Resource path in the Godot project.</param>
/// <param name="Type">Godot resource type name.</param>
/// <param name="Properties">Resource property bag.</param>
public sealed record ResourceData(
    string Path,
    string Type,
    IReadOnlyDictionary<string, object?> Properties);
