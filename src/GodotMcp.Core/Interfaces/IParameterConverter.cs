namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Converts between C# types and MCP parameter formats
/// </summary>
public interface IParameterConverter
{
    /// <summary>
    /// Converts C# parameters to MCP format
    /// </summary>
    /// <param name="parameters">The parameters to convert</param>
    /// <param name="toolDefinition">The tool definition providing parameter metadata</param>
    /// <returns>A dictionary of parameters in MCP format</returns>
    IReadOnlyDictionary<string, object?> ConvertToMcp(
        IReadOnlyDictionary<string, object?> parameters,
        McpToolDefinition toolDefinition);
    
    /// <summary>
    /// Converts MCP response to C# type
    /// </summary>
    /// <typeparam name="T">The target type to convert to</typeparam>
    /// <param name="response">The MCP response to convert</param>
    /// <returns>The converted value of type T, or null if conversion fails</returns>
    T? ConvertFromMcp<T>(McpResponse response);
    
    /// <summary>
    /// Registers a custom type converter
    /// </summary>
    /// <typeparam name="T">The type to register a converter for</typeparam>
    /// <param name="converter">The converter implementation</param>
    void RegisterConverter<T>(ITypeConverter<T> converter);
}
