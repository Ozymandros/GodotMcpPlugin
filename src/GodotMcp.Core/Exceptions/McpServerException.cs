namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when the godot-mcp server returns an error response.
/// </summary>
/// <remarks>
/// This exception is raised when the godot-mcp server successfully receives and processes a request
/// but returns an error response. The <see cref="ErrorCode"/> property contains the MCP error code,
/// and the <see cref="ErrorData"/> property may contain additional error details from the server.
/// </remarks>
public class McpServerException : GodotMcpException
{
    /// <summary>
    /// Gets the MCP error code returned by the server.
    /// </summary>
    /// <value>
    /// An integer representing the MCP protocol error code.
    /// </value>
    public int ErrorCode { get; init; }

    /// <summary>
    /// Gets additional error data returned by the server.
    /// </summary>
    /// <value>
    /// An object containing additional error details, or null if no additional data was provided.
    /// </value>
    public object? ErrorData { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerException"/> class with a specified error message,
    /// error code, and optional error data.
    /// </summary>
    /// <param name="message">The error message returned by the server.</param>
    /// <param name="errorCode">The MCP error code.</param>
    /// <param name="errorData">Additional error data, or null if not provided.</param>
    public McpServerException(string message, int errorCode, object? errorData = null)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorData = errorData;
    }
}
