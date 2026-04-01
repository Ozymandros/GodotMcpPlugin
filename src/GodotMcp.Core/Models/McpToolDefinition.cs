namespace GodotMcp.Core.Models;

/// <summary>
/// Represents an MCP tool definition discovered from the godot-mcp server
/// </summary>
/// <param name="Name">The unique tool name used to invoke it</param>
/// <param name="Description">Human-readable description of what the tool does</param>
/// <param name="Parameters">Dictionary of parameter definitions keyed by parameter name</param>
/// <param name="ReturnType">Optional description of the tool's return type</param>
public sealed record McpToolDefinition(
    string Name,
    string Description,
    IReadOnlyDictionary<string, McpParameterDefinition> Parameters,
    McpReturnType? ReturnType = null);
