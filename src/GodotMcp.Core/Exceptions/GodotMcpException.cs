namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Base exception for all Godot MCP plugin errors.
/// </summary>
/// <remarks>
/// This exception serves as the base class for all exceptions thrown by the Godot MCP plugin.
/// It provides a common exception type that can be caught to handle any plugin-related error.
/// Specific error scenarios are represented by derived exception types that provide additional context.
/// </remarks>
public class GodotMcpException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GodotMcpException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GodotMcpException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GodotMcpException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public GodotMcpException(string message, Exception innerException) 
        : base(message, innerException) { }
}
