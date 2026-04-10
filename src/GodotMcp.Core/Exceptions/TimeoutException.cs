namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when an operation exceeds its configured timeout period.
/// </summary>
/// <remarks>
/// This exception is raised when an operation (such as connection establishment or request processing)
/// does not complete within the specified timeout duration. The <see cref="Timeout"/> property indicates
/// the timeout duration that was exceeded, and the <see cref="Operation"/> property identifies which
/// operation timed out.
/// </remarks>
public class TimeoutException : GodotMcpException
{
    /// <summary>
    /// Gets the timeout duration that was exceeded.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the configured timeout duration.
    /// </value>
    public TimeSpan Timeout { get; init; }

    /// <summary>
    /// Gets the name of the operation that timed out.
    /// </summary>
    /// <value>
    /// A string identifying the operation (e.g., "Connect", "InvokeTool"), or null if not specified.
    /// </value>
    public string? Operation { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutException"/> class with a specified error message,
    /// timeout duration, and optional operation name.
    /// </summary>
    /// <param name="message">The message that describes the timeout error.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    /// <param name="operation">The name of the operation that timed out, or null if not specified.</param>
    public TimeoutException(string message, TimeSpan timeout, string? operation = null)
        : base(message)
    {
        Timeout = timeout;
        Operation = operation;
    }
}
