namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a JSON-RPC 2.0 MCP response received from the godot-mcp server
/// </summary>
/// <param name="Id">The request identifier this response corresponds to</param>
/// <param name="Success">Whether the request completed successfully</param>
/// <param name="Result">The result payload when <paramref name="Success"/> is <c>true</c>, otherwise <c>null</c></param>
/// <param name="Error">The error details when <paramref name="Success"/> is <c>false</c>, otherwise <c>null</c></param>
public sealed record McpResponse(
    string Id,
    bool Success,
    object? Result = null,
    McpError? Error = null);
