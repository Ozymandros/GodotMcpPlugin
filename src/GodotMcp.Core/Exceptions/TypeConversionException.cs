namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when type conversion between C# types and MCP parameter formats fails.
/// </summary>
/// <remarks>
/// This exception is raised when the parameter converter cannot convert a value from one type to another,
/// such as when converting C# objects to MCP JSON format or vice versa. The <see cref="SourceType"/> and
/// <see cref="TargetType"/> properties identify the types involved in the failed conversion.
/// </remarks>
public class TypeConversionException : GodotMcpException
{
    /// <summary>
    /// Gets the source type that was being converted from.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the source value, or null if not available.
    /// </value>
    public Type? SourceType { get; init; }
    
    /// <summary>
    /// Gets the target type that was being converted to.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the target value, or null if not available.
    /// </value>
    public Type? TargetType { get; init; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConversionException"/> class with a specified error message
    /// and optional source and target type information.
    /// </summary>
    /// <param name="message">The message that describes the conversion error.</param>
    /// <param name="sourceType">The type being converted from, or null if not available.</param>
    /// <param name="targetType">The type being converted to, or null if not available.</param>
    public TypeConversionException(string message, Type? sourceType = null, Type? targetType = null)
        : base(message)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }
}
