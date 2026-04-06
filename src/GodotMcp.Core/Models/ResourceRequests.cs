namespace GodotMcp.Core.Models;

/// <summary>
/// Resource command request for listing resources.
/// </summary>
/// <param name="Directory">Optional directory path filter.</param>
/// <param name="ResourceType">Optional resource type filter.</param>
public sealed record ResourceListRequest(
    string? Directory = null,
    string? ResourceType = null);

/// <summary>
/// Resource command request for reading a resource.
/// </summary>
/// <param name="ResourcePath">Resource path to read.</param>
public sealed record ResourceReadRequest(string ResourcePath);

/// <summary>
/// Resource command request for updating a resource.
/// </summary>
/// <param name="ResourcePath">Resource path to update.</param>
/// <param name="Properties">Properties to set on the resource.</param>
public sealed record ResourceUpdateRequest(
    string ResourcePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Resource command request for creating a resource.
/// </summary>
/// <param name="ResourcePath">Resource path to create.</param>
/// <param name="ResourceType">Godot resource type to create.</param>
/// <param name="Properties">Initial resource properties.</param>
public sealed record ResourceCreateRequest(
    string ResourcePath,
    string ResourceType,
    IReadOnlyDictionary<string, object?> Properties);
