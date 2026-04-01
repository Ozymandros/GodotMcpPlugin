namespace GodotMcp.Core.Models;

/// <summary>
/// Represents an MCP parameter definition describing a tool's input parameter
/// </summary>
/// <param name="Name">The parameter name as defined by the MCP tool</param>
/// <param name="Type">The MCP type string (e.g., "string", "integer", "boolean", "number", "object", "array")</param>
/// <param name="Description">Optional human-readable description of the parameter</param>
/// <param name="Required">Whether this parameter must be provided when invoking the tool</param>
/// <param name="DefaultValue">Optional default value used when the parameter is not provided</param>
public sealed record McpParameterDefinition(
    string Name,
    string Type,
    string? Description = null,
    bool Required = false,
    object? DefaultValue = null);
