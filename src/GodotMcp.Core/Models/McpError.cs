namespace GodotMcp.Core.Models;

/// <summary>
/// Represents an MCP error returned in a JSON-RPC error response
/// </summary>
/// <param name="Code">The JSON-RPC error code</param>
/// <param name="Message">A short description of the error</param>
/// <param name="Data">Optional additional data providing more detail about the error</param>
public sealed record McpError(
    int Code,
    string Message,
    object? Data = null);
