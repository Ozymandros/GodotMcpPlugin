using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using GodotMcp.Infrastructure.Serialization;
using FsCheck;
using FsCheck.Xunit;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Property-based tests for MCP message round-trip consistency
/// **Validates: Requirements 1.9, 19.8**
/// </summary>
public class McpMessageRoundTripPropertyTests
{
    private readonly JsonRpcRequestHandler _handler;

    public McpMessageRoundTripPropertyTests()
    {
        _handler = new JsonRpcRequestHandler(NullLogger<JsonRpcRequestHandler>.Instance);
    }

    /// <summary>
    /// Property 1: Round-trip consistency for MCP messages
    /// For all valid MCP requests, serializing then deserializing should produce equivalent structured data
    /// </summary>
    [Property(MaxTest = 100)]
    public bool McpRequest_RoundTrip_PreservesStructure(NonEmptyString id, NonEmptyString method)
    {
        // Arrange
        var request = new McpRequest(
            Id: id.Get,
            Method: method.Get,
            Parameters: new Dictionary<string, object?> { ["test"] = "value" }
        );

        // Act
        var serialized = _handler.SerializeRequest(request);
        
        // Parse the serialized JSON to extract the structure
        using var document = JsonDocument.Parse(serialized);
        var root = document.RootElement;
        
        // Verify JSON-RPC structure
        var hasJsonRpc = root.TryGetProperty("jsonrpc", out var jsonRpcElement) 
            && jsonRpcElement.GetString() == "2.0";
        var hasId = root.TryGetProperty("id", out var idElement) 
            && idElement.GetString() == request.Id;
        var hasMethod = root.TryGetProperty("method", out var methodElement) 
            && methodElement.GetString() == request.Method;
        var hasParams = root.TryGetProperty("params", out var paramsElement);
        
        // Verify parameters are preserved (excluding null values which are omitted)
        var parametersPreserved = true;
        if (hasParams && paramsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var (key, value) in request.Parameters)
            {
                if (value != null)
                {
                    parametersPreserved = parametersPreserved && paramsElement.TryGetProperty(key, out _);
                }
            }
        }
        
        return hasJsonRpc && hasId && hasMethod && hasParams && parametersPreserved;
    }

    /// <summary>
    /// Property 2: Round-trip consistency for MCP responses with results
    /// For all valid MCP responses with results, parsing then serializing should preserve data
    /// </summary>
    [Property(MaxTest = 100)]
    public bool McpResponse_WithResult_RoundTrip_PreservesData(NonEmptyString id, int resultValue)
    {
        // Arrange - Create a response JSON
        var responseJson = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = id.Get,
            result = resultValue
        });

        // Act
        var response = _handler.DeserializeResponse(responseJson);
        
        // Re-serialize the response
        var reSerialized = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = response.Id,
            result = response.Result
        });
        
        var reDeserialized = _handler.DeserializeResponse(reSerialized);

        // Assert
        return response.Id == reDeserialized.Id 
                && response.Success == reDeserialized.Success
                && response.Error == reDeserialized.Error;
    }

    /// <summary>
    /// Property 3: Round-trip consistency for MCP responses with errors
    /// For all valid MCP error responses, parsing then serializing should preserve error data
    /// </summary>
    [Property(MaxTest = 100)]
    public bool McpResponse_WithError_RoundTrip_PreservesError(NonEmptyString id, int errorCode, NonEmptyString errorMessage)
    {
        // Arrange - Create an error response JSON
        var responseJson = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = id.Get,
            error = new
            {
                code = errorCode,
                message = errorMessage.Get
            }
        });

        // Act
        var response = _handler.DeserializeResponse(responseJson);
        
        // Re-serialize the response
        var reSerialized = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = response.Id,
            error = response.Error != null ? new
            {
                code = response.Error.Code,
                message = response.Error.Message,
                data = response.Error.Data
            } : null
        });
        
        var reDeserialized = _handler.DeserializeResponse(reSerialized);

        // Assert
        return response.Id == reDeserialized.Id 
                && response.Success == reDeserialized.Success
                && response.Error?.Code == reDeserialized.Error?.Code
                && response.Error?.Message == reDeserialized.Error?.Message;
    }

    /// <summary>
    /// Property 4: Round-trip with various parameter types
    /// Tests that different parameter types (primitives, objects, arrays, nulls) survive round-trip
    /// </summary>
    [Property(MaxTest = 100)]
    public bool McpRequest_WithVariousParameterTypes_RoundTrip_PreservesTypes(
        NonEmptyString id,
        NonEmptyString method,
        string stringParam,
        int intParam,
        bool boolParam,
        NormalFloat doubleParam)
    {
        // Arrange
        var request = new McpRequest(
            Id: id.Get,
            Method: method.Get,
            Parameters: new Dictionary<string, object?>
            {
                ["stringParam"] = stringParam,
                ["intParam"] = intParam,
                ["boolParam"] = boolParam,
                ["doubleParam"] = doubleParam.Get,
                ["nullParam"] = null,
                ["arrayParam"] = new[] { 1, 2, 3 },
                ["objectParam"] = new Dictionary<string, object?> { ["nested"] = "value", ["count"] = 42 }
            }
        );

        // Act
        var serialized = _handler.SerializeRequest(request);
        using var document = JsonDocument.Parse(serialized);
        var root = document.RootElement;
        
        // Assert - Verify all non-null parameters are present with correct types
        var paramsElement = root.GetProperty("params");
        
        var stringPreserved = paramsElement.TryGetProperty("stringParam", out var strEl) 
            && (strEl.ValueKind == JsonValueKind.String || strEl.ValueKind == JsonValueKind.Null);
        var intPreserved = paramsElement.TryGetProperty("intParam", out var intEl) 
            && intEl.ValueKind == JsonValueKind.Number;
        var boolPreserved = paramsElement.TryGetProperty("boolParam", out var boolEl) 
            && (boolEl.ValueKind == JsonValueKind.True || boolEl.ValueKind == JsonValueKind.False);
        var doublePreserved = paramsElement.TryGetProperty("doubleParam", out var dblEl) 
            && dblEl.ValueKind == JsonValueKind.Number;
        var nullOmitted = !paramsElement.TryGetProperty("nullParam", out _);
        var arrayPreserved = paramsElement.TryGetProperty("arrayParam", out var arrEl) 
            && arrEl.ValueKind == JsonValueKind.Array;
        var objectPreserved = paramsElement.TryGetProperty("objectParam", out var objEl) 
            && objEl.ValueKind == JsonValueKind.Object;

        return stringPreserved && intPreserved && boolPreserved && doublePreserved 
                && nullOmitted && arrayPreserved && objectPreserved;
    }
}
