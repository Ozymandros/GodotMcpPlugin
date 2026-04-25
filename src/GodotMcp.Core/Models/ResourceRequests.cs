namespace GodotMcp.Core.Models;

/// <summary>
/// Resource command request for listing resources.
/// </summary>
public sealed record ResourceListRequest(
    string? Directory = null,
    string? ResourceType = null);

/// <summary>
/// Resource command request for reading a resource.
/// </summary>
public sealed record ResourceReadRequest(McpProjectFile Resource);

/// <summary>
/// Resource command request for updating a resource.
/// </summary>
public sealed record ResourceUpdateRequest(
    McpProjectFile Resource,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Resource command request for creating a resource.
/// </summary>
public sealed record ResourceCreateRequest(
    McpProjectFile Resource,
    string ResourceType,
    IReadOnlyDictionary<string, object?> Properties);
