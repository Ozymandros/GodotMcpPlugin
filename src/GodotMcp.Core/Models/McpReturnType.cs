namespace GodotMcp.Core.Models;

/// <summary>
/// Represents the return type metadata for an MCP tool
/// </summary>
/// <param name="Type">The MCP type string (e.g., "string", "object", "array")</param>
/// <param name="Description">Optional human-readable description of the return value</param>
public sealed record McpReturnType(
    string Type,
    string? Description = null);
