namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when the MCP protocol is violated or malformed data is received.
/// </summary>
/// <remarks>
/// This exception is raised when the plugin receives data that does not conform to the MCP protocol specification,
/// such as invalid JSON, missing required fields, or unexpected message formats. The <see cref="MalformedData"/>
/// property may contain a sample of the problematic data for diagnostic purposes.
/// </remarks>
public class ProtocolException : GodotMcpException
{
    /// <summary>
    /// Gets a sample of the malformed data that caused the protocol violation.
    /// </summary>
    /// <value>
    /// A string containing the malformed data, or null if not available.
    /// </value>
    public string? MalformedData { get; init; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolException"/> class with a specified error message
    /// and optional malformed data sample.
    /// </summary>
    /// <param name="message">The message that describes the protocol violation.</param>
    /// <param name="malformedData">A sample of the malformed data, or null if not available.</param>
    public ProtocolException(string message, string? malformedData = null)
        : base(message)
    {
        MalformedData = malformedData;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolException"/> class with a specified error message,
    /// inner exception, and optional malformed data sample.
    /// </summary>
    /// <param name="message">The message that describes the protocol violation.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="malformedData">A sample of the malformed data, or null if not available.</param>
    public ProtocolException(string message, Exception innerException, string? malformedData = null)
        : base(message, innerException)
    {
        MalformedData = malformedData;
    }
}
