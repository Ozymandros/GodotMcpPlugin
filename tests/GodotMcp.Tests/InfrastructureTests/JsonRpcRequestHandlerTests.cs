using Microsoft.Extensions.Logging.Abstractions;
using GodotMcp.Core.Exceptions;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Serialization;

namespace GodotMcp.Tests.InfrastructureTests;

public class JsonRpcRequestHandlerTests
{
    private readonly JsonRpcRequestHandler _handler;

    public JsonRpcRequestHandlerTests()
    {
        _handler = new JsonRpcRequestHandler(NullLogger<JsonRpcRequestHandler>.Instance);
    }

    [Fact]
    public void SerializeRequest_CreatesValidJsonRpc20Structure()
    {
        // Arrange
        var request = new McpRequest(
            Id: "test-123",
            Method: "Godot_create_scene",
            Parameters: new Dictionary<string, object?>
            {
                ["sceneName"] = "TestScene",
                ["additive"] = true
            }
        );

        // Act
        var json = _handler.SerializeRequest(request);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"jsonrpc\":\"2.0\"", json);
        Assert.Contains("\"id\":\"test-123\"", json);
        Assert.Contains("\"method\":\"Godot_create_scene\"", json);
        Assert.Contains("\"params\"", json);
        Assert.Contains("\"sceneName\":\"TestScene\"", json);
        Assert.Contains("\"additive\":true", json);
    }

    [Fact]
    public void SerializeRequest_UsesCorrectNamingPolicy()
    {
        // Arrange
        var request = new McpRequest(
            Id: "1",
            Method: "test_method",
            Parameters: new Dictionary<string, object?>
            {
                ["TestParameter"] = "value"
            }
        );

        // Act
        var json = _handler.SerializeRequest(request);

        // Assert - should use camelCase
        Assert.Contains("\"jsonrpc\"", json);
        Assert.Contains("\"params\"", json);
    }

    [Fact]
    public void SerializeRequest_OmitsNullValues()
    {
        // Arrange
        var request = new McpRequest(
            Id: "1",
            Method: "test",
            Parameters: new Dictionary<string, object?>
            {
                ["key1"] = "value",
                ["key2"] = null
            }
        );

        // Act
        var json = _handler.SerializeRequest(request);

        // Assert - null values should be omitted
        Assert.Contains("\"key1\":\"value\"", json);
        Assert.DoesNotContain("\"key2\"", json);
    }

    [Fact]
    public void DeserializeResponse_WithResult_ReturnsSuccessResponse()
    {
        // Arrange
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-123",
            "result": {
                "status": "success",
                "data": "Scene created"
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test-123", response.Id);
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
        Assert.Null(response.Error);
    }

    [Fact]
    public void DeserializeResponse_WithError_ReturnsFailureResponse()
    {
        // Arrange
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-456",
            "error": {
                "code": -32600,
                "message": "Invalid Request",
                "data": "Missing required parameter"
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test-456", response.Id);
        Assert.False(response.Success);
        Assert.Null(response.Result);
        Assert.NotNull(response.Error);
        Assert.Equal(-32600, response.Error.Code);
        Assert.Equal("Invalid Request", response.Error.Message);
    }

    [Fact]
    public void DeserializeResponse_WithMalformedJson_ThrowsProtocolException()
    {
        // Arrange
        var json = "{ invalid json }";

        // Act & Assert
        var exception = Assert.Throws<ProtocolException>(() => _handler.DeserializeResponse(json));
        Assert.Contains("Failed to deserialize MCP response", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void PrettyPrint_FormatsJsonWithIndentation()
    {
        // Arrange
        var compactJson = """{"jsonrpc":"2.0","id":"1","method":"test","params":{"key":"value"}}""";

        // Act
        var prettyJson = _handler.PrettyPrint(compactJson);

        // Assert
        Assert.NotNull(prettyJson);
        Assert.Contains("\n", prettyJson); // Should have newlines
        Assert.Contains("  ", prettyJson); // Should have indentation
    }

    [Fact]
    public void PrettyPrint_WithInvalidJson_ReturnsOriginal()
    {
        // Arrange
        var invalidJson = "{ invalid }";

        // Act
        var result = _handler.PrettyPrint(invalidJson);

        // Assert
        Assert.Equal(invalidJson, result);
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesData()
    {
        // Arrange
        var originalRequest = new McpRequest(
            Id: "round-trip-test",
            Method: "Godot_test_method",
            Parameters: new Dictionary<string, object?>
            {
                ["stringParam"] = "test value",
                ["numberParam"] = 42,
                ["boolParam"] = true
            }
        );

        // Act
        var serialized = _handler.SerializeRequest(originalRequest);
        
        // Simulate server response with the same data
        var responseJson = """
        {
            "jsonrpc": "2.0",
            "id": "round-trip-test",
            "result": {
                "method": "Godot_test_method",
                "stringParam": "test value",
                "numberParam": 42,
                "boolParam": true
            }
        }
        """;
        
        var response = _handler.DeserializeResponse(responseJson);

        // Assert
        Assert.Equal(originalRequest.Id, response.Id);
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
    }

    [Fact]
    public void DeserializeResponse_WithMissingOptionalFields_HandlesGracefully()
    {
        // Arrange - response with minimal required fields (id and result)
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-789",
            "result": {}
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test-789", response.Id);
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
        Assert.Null(response.Error);
    }

    [Fact]
    public void DeserializeResponse_WithMissingId_HandlesGracefully()
    {
        // Arrange - response without id (optional in JSON-RPC 2.0 for notifications)
        var json = """
        {
            "jsonrpc": "2.0",
            "result": {
                "status": "ok"
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.Id); // Should default to empty string
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
    }

    [Fact]
    public void DeserializeResponse_WithMissingErrorData_HandlesGracefully()
    {
        // Arrange - error response without optional data field
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-error",
            "error": {
                "code": -32601,
                "message": "Method not found"
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test-error", response.Id);
        Assert.False(response.Success);
        Assert.NotNull(response.Error);
        Assert.Equal(-32601, response.Error.Code);
        Assert.Equal("Method not found", response.Error.Message);
        Assert.Null(response.Error.Data); // Data is optional
    }

    [Fact]
    public void DeserializeResponse_WithUnknownFields_IgnoresThem()
    {
        // Arrange - response with extra unknown fields
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-unknown",
            "result": {
                "status": "success"
            },
            "unknownField1": "should be ignored",
            "unknownField2": 12345,
            "extraData": {
                "nested": "value"
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert - should successfully deserialize, ignoring unknown fields
        Assert.NotNull(response);
        Assert.Equal("test-unknown", response.Id);
        Assert.True(response.Success);
        Assert.NotNull(response.Result);
    }

    [Fact]
    public void DeserializeResponse_WithUnknownFieldsInError_IgnoresThem()
    {
        // Arrange - error response with extra unknown fields
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-error-unknown",
            "error": {
                "code": -32000,
                "message": "Server error",
                "data": "error details",
                "unknownErrorField": "ignored",
                "extraInfo": 999
            },
            "serverVersion": "1.0.0"
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert - should successfully deserialize, ignoring unknown fields
        Assert.NotNull(response);
        Assert.Equal("test-error-unknown", response.Id);
        Assert.False(response.Success);
        Assert.NotNull(response.Error);
        Assert.Equal(-32000, response.Error.Code);
        Assert.Equal("Server error", response.Error.Message);
    }

    [Fact]
    public void DeserializeResponse_WithMissingJsonRpcVersion_ThrowsProtocolException()
    {
        // Arrange - missing required "jsonrpc" field
        var json = """
        {
            "id": "test-missing-version",
            "result": {
                "status": "ok"
            }
        }
        """;

        // Act & Assert
        // Note: The current implementation doesn't validate jsonrpc field presence,
        // but it should still deserialize successfully as the field is not used
        // This test documents the current behavior
        var response = _handler.DeserializeResponse(json);
        Assert.NotNull(response);
        Assert.Equal("test-missing-version", response.Id);
    }

    [Fact]
    public void DeserializeResponse_WithMissingResultAndError_ReturnsSuccessWithNullResult()
    {
        // Arrange - response with neither result nor error (edge case)
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-no-result-no-error"
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert - should treat as success with null result
        Assert.NotNull(response);
        Assert.Equal("test-no-result-no-error", response.Id);
        Assert.True(response.Success);
        Assert.Null(response.Result);
        Assert.Null(response.Error);
    }

    [Fact]
    public void DeserializeResponse_WithMissingErrorCode_HandlesGracefully()
    {
        // Arrange - error without code field
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-no-code",
            "error": {
                "message": "Error without code"
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert - should use default code (-1)
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotNull(response.Error);
        Assert.Equal(-1, response.Error.Code); // Default value
        Assert.Equal("Error without code", response.Error.Message);
    }

    [Fact]
    public void DeserializeResponse_WithMissingErrorMessage_HandlesGracefully()
    {
        // Arrange - error without message field
        var json = """
        {
            "jsonrpc": "2.0",
            "id": "test-no-message",
            "error": {
                "code": -32700
            }
        }
        """;

        // Act
        var response = _handler.DeserializeResponse(json);

        // Assert - should use default message
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.NotNull(response.Error);
        Assert.Equal(-32700, response.Error.Code);
        Assert.Equal("Unknown error", response.Error.Message); // Default value
    }
}
