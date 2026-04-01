using System.Text.Json;
using System.Text.Json.Serialization;
using GodotMcp.Core.Utilities;

namespace GodotMcp.Infrastructure.Serialization;

/// <summary>
/// Handles JSON-RPC 2.0 serialization/deserialization for MCP using source generators
/// </summary>
public sealed partial class JsonRpcRequestHandler : IRequestHandler
{
    private readonly JsonSerializerOptions _options;
    private readonly ILogger<JsonRpcRequestHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcRequestHandler"/> class
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic output</param>
    public JsonRpcRequestHandler(ILogger<JsonRpcRequestHandler> logger)
    {
        _logger = logger;
        
        // Configure options with source generator context
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            TypeInfoResolver = McpJsonSerializerContext.Default
        };
    }

    /// <summary>
    /// Serializes an MCP request to JSON-RPC 2.0 format
    /// </summary>
    public string SerializeRequest(McpRequest request)
    {
        try
        {
            LogSerializingRequest(request.Id, request.Method);

            // Create JSON-RPC 2.0 structure manually, filtering out null values
            var jsonRpc = new Dictionary<string, object?>
            {
                ["jsonrpc"] = "2.0",
                ["id"] = request.Id,
                ["method"] = request.Method,
                ["params"] = request.Parameters.Where(kvp => kvp.Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            var json = JsonSerializer.Serialize(jsonRpc, _options);
            
            LogRequestSerialized(request.Id, json.Length);
            
            return json;
        }
        catch (Exception ex)
        {
            LogSerializationFailed(request.Id, ex);
            throw new ProtocolException("Failed to serialize MCP request", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON-RPC 2.0 response to MCP response format
    /// </summary>
    public McpResponse DeserializeResponse(string json)
    {
        try
        {
            LogDeserializingResponse(json.Length);

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Extract id
            var id = root.TryGetProperty("id", out var idElement)
                ? idElement.GetString() ?? string.Empty
                : string.Empty;

            // Check for error
            if (root.TryGetProperty("error", out var errorElement))
            {
                var code = errorElement.TryGetProperty("code", out var codeElement)
                    ? codeElement.GetInt32()
                    : -1;

                var message = errorElement.TryGetProperty("message", out var messageElement)
                    ? messageElement.GetString() ?? "Unknown error"
                    : "Unknown error";

                var data = errorElement.TryGetProperty("data", out var dataElement)
                    ? JsonSerializer.Deserialize<object>(dataElement.GetRawText(), _options)
                    : null;

                var error = new McpError(code, message, data);
                
                LogResponseDeserialized(id, false);
                
                return new McpResponse(id, false, null, error);
            }

            // Extract result
            var result = root.TryGetProperty("result", out var resultElement)
                ? JsonSerializer.Deserialize<object>(resultElement.GetRawText(), _options)
                : null;

            LogResponseDeserialized(id, true);

            return new McpResponse(id, true, result);
        }
        catch (Exception ex)
        {
            // Don't log the full JSON as it may contain sensitive data
            LogDeserializationFailed(json.Length, ex);
            throw new ProtocolException("Failed to deserialize MCP response", ex, json);
        }
    }

    /// <summary>
    /// Pretty prints a JSON message for debugging
    /// </summary>
    public string PrettyPrint(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            // Use a separate options instance without source generator for pretty printing
            var prettyOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(document.RootElement, prettyOptions);
        }
        catch
        {
            // Return original if parsing fails
            return json;
        }
    }

    // LoggerMessage source generator methods for structured logging
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Serializing MCP request. ID: {RequestId}, Method: {Method}")]
    partial void LogSerializingRequest(string requestId, string method);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Debug,
        Message = "Request serialized successfully. ID: {RequestId}, Size: {Size} bytes")]
    partial void LogRequestSerialized(string requestId, int size);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Failed to serialize request. ID: {RequestId}")]
    partial void LogSerializationFailed(string requestId, Exception ex);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Debug,
        Message = "Deserializing MCP response. Size: {Size} bytes")]
    partial void LogDeserializingResponse(int size);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Debug,
        Message = "Response deserialized successfully. ID: {ResponseId}, Success: {Success}")]
    partial void LogResponseDeserialized(string responseId, bool success);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Error,
        Message = "Failed to deserialize response. Size: {Size} bytes")]
    partial void LogDeserializationFailed(int size, Exception ex);
}
