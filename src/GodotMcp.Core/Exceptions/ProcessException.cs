namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when godot-mcp server process management operations fail.
/// </summary>
/// <remarks>
/// This exception is raised when operations related to the godot-mcp server process fail,
/// such as process startup, shutdown, or communication with the process. The <see cref="ProcessId"/>
/// property identifies the process that encountered the error, if available.
/// </remarks>
public class ProcessException : GodotMcpException
{
    /// <summary>
    /// Gets the process ID of the godot-mcp server process that encountered an error.
    /// </summary>
    /// <value>
    /// An integer representing the process ID, or null if the process was not started or the ID is not available.
    /// </value>
    public int? ProcessId { get; init; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessException"/> class with a specified error message
    /// and optional process ID.
    /// </summary>
    /// <param name="message">The message that describes the process error.</param>
    /// <param name="processId">The process ID, or null if not available.</param>
    public ProcessException(string message, int? processId = null)
        : base(message)
    {
        ProcessId = processId;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessException"/> class with a specified error message,
    /// inner exception, and optional process ID.
    /// </summary>
    /// <param name="message">The message that describes the process error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="processId">The process ID, or null if not available.</param>
    public ProcessException(string message, Exception innerException, int? processId = null)
        : base(message, innerException)
    {
        ProcessId = processId;
    }
}
