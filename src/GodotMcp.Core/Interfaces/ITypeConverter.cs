namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Custom type converter for specific types
/// </summary>
/// <typeparam name="T">The type this converter handles</typeparam>
public interface ITypeConverter<T>
{
    /// <summary>
    /// Converts value to MCP format
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The value in MCP format, or null if conversion fails</returns>
    object? ToMcp(T value);
    
    /// <summary>
    /// Converts from MCP format to typed value
    /// </summary>
    /// <param name="mcpValue">The MCP value to convert</param>
    /// <returns>The converted typed value, or null if conversion fails</returns>
    T? FromMcp(object? mcpValue);
}
