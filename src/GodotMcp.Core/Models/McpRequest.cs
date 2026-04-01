namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a JSON-RPC 2.0 MCP request sent to the godot-mcp server
/// </summary>
/// <param name="Id">The unique request identifier used to correlate responses</param>
/// <param name="Method">The MCP method or tool name to invoke</param>
/// <param name="Parameters">The input parameters for the method or tool</param>
public sealed record McpRequest(
    string Id,
    string Method,
    IReadOnlyDictionary<string, object?> Parameters);
