namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Handles serialization and deserialization of MCP messages
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// Serializes an MCP request
    /// </summary>
    /// <param name="request">The MCP request to serialize</param>
    /// <returns>The serialized JSON string</returns>
    string SerializeRequest(McpRequest request);

    /// <summary>
    /// Deserializes an MCP response
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized MCP response</returns>
    McpResponse DeserializeResponse(string json);

    /// <summary>
    /// Pretty prints an MCP message for debugging
    /// </summary>
    /// <param name="json">The JSON string to format</param>
    /// <returns>A formatted JSON string with indentation</returns>
    string PrettyPrint(string json);
}
