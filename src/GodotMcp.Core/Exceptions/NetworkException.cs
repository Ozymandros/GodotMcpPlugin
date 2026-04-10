namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when network communication with the godot-mcp server fails.
/// </summary>
/// <remarks>
/// This exception is raised when network-level errors occur during communication with the godot-mcp server,
/// such as connection failures, socket errors, or I/O exceptions. The <see cref="Endpoint"/> property
/// provides information about the connection endpoint that failed.
/// </remarks>
public class NetworkException : GodotMcpException
{
    /// <summary>
    /// Gets the endpoint that failed to connect or communicate.
    /// </summary>
    /// <value>
    /// A string representing the network endpoint (e.g., "stdio://godot-mcp"), or null if not applicable.
    /// </value>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class with a specified error message
    /// and optional endpoint information.
    /// </summary>
    /// <param name="message">The message that describes the network error.</param>
    /// <param name="endpoint">The endpoint that failed, or null if not applicable.</param>
    public NetworkException(string message, string? endpoint = null)
        : base(message)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class with a specified error message,
    /// inner exception, and optional endpoint information.
    /// </summary>
    /// <param name="message">The message that describes the network error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="endpoint">The endpoint that failed, or null if not applicable.</param>
    public NetworkException(string message, Exception innerException, string? endpoint = null)
        : base(message, innerException)
    {
        Endpoint = endpoint;
    }
}
