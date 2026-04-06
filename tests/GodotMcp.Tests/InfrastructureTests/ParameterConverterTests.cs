using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using GodotMcp.Core.Exceptions;
using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Conversion;

namespace GodotMcp.Tests.InfrastructureTests;

public class ParameterConverterTests
{
    private readonly ParameterConverter _converter;
    private readonly JsonSerializerOptions _testJsonOptions;

    public ParameterConverterTests()
    {
        _converter = new ParameterConverter(NullLogger<ParameterConverter>.Instance);
        GodotTypeConverter.RegisterDefaults(_converter);

        // Create test-specific JSON options without source generator for test types
        _testJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region ConvertToMcp - Primitive Types Tests

    [Fact]
    public void ConvertToMcp_WithIntParameter_ReturnsIntValue()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["count"] = 42
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result["count"]);
    }

    [Fact]
    public void ConvertToMcp_WithFloatParameter_ReturnsFloatValue()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["speed"] = 3.14f
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3.14f, result["speed"]);
    }

    [Fact]
    public void ConvertToMcp_WithStringParameter_ReturnsStringValue()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "TestScene"
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestScene", result["name"]);
    }

    [Fact]
    public void ConvertToMcp_WithBoolParameter_ReturnsBoolValue()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["enabled"] = true
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(true, result["enabled"]);
    }

    [Fact]
    public void ConvertToMcp_WithMultiplePrimitives_ReturnsAllValues()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["count"] = 10,
            ["speed"] = 2.5f,
            ["name"] = "Test",
            ["enabled"] = false
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(10, result["count"]);
        Assert.Equal(2.5f, result["speed"]);
        Assert.Equal("Test", result["name"]);
        Assert.Equal(false, result["enabled"]);
    }

    #endregion

    #region ConvertToMcp - Godot Types Tests

    [Fact]
    public void ConvertToMcp_WithVector2_SerializesToJsonElement()
    {
        // Arrange
        var vector = new Vector2(1.5f, 2.5f);
        var parameters = new Dictionary<string, object?>
        {
            ["position"] = vector
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("position"));
        var jsonElement = Assert.IsType<JsonElement>(result["position"]);
        Assert.Equal(JsonValueKind.Object, jsonElement.ValueKind);
        Assert.Equal(1.5f, jsonElement.GetProperty("x").GetSingle());
        Assert.Equal(2.5f, jsonElement.GetProperty("y").GetSingle());
    }

    [Fact]
    public void ConvertToMcp_WithVector3_SerializesToJsonElement()
    {
        // Arrange
        var vector = new Vector3(1.0f, 2.0f, 3.0f);
        var parameters = new Dictionary<string, object?>
        {
            ["position"] = vector
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("position"));
        var jsonElement = Assert.IsType<JsonElement>(result["position"]);
        Assert.Equal(JsonValueKind.Object, jsonElement.ValueKind);
        Assert.Equal(1.0f, jsonElement.GetProperty("x").GetSingle());
        Assert.Equal(2.0f, jsonElement.GetProperty("y").GetSingle());
        Assert.Equal(3.0f, jsonElement.GetProperty("z").GetSingle());
    }

    [Fact]
    public void ConvertToMcp_WithColor_SerializesToJsonElement()
    {
        // Arrange
        var color = new Color(0.5f, 0.75f, 1.0f, 0.8f);
        var parameters = new Dictionary<string, object?>
        {
            ["color"] = color
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("color"));
        var jsonElement = Assert.IsType<JsonElement>(result["color"]);
        Assert.Equal(JsonValueKind.Object, jsonElement.ValueKind);
        Assert.Equal(0.5f, jsonElement.GetProperty("r").GetSingle());
        Assert.Equal(0.75f, jsonElement.GetProperty("g").GetSingle());
        Assert.Equal(1.0f, jsonElement.GetProperty("b").GetSingle());
        Assert.Equal(0.8f, jsonElement.GetProperty("a").GetSingle());
    }

    [Fact]
    public void ConvertToMcp_WithNodePath_UsesStringRepresentation()
    {
        var parameters = new Dictionary<string, object?>
        {
            ["nodePath"] = new NodePath("Level/Player")
        };

        var result = _converter.ConvertToMcp(parameters, CreateToolDefinition());

        Assert.Equal("Level/Player", result["nodePath"]);
    }

    [Fact]
    public void ConvertToMcp_WithResourceReference_UsesPathObject()
    {
        var parameters = new Dictionary<string, object?>
        {
            ["scene"] = new ResourceReference("res://Scenes/Main.tscn")
        };

        var result = _converter.ConvertToMcp(parameters, CreateToolDefinition());
        var value = Assert.IsType<Dictionary<string, object?>>(result["scene"]);
        Assert.Equal("res://Scenes/Main.tscn", value["path"]);
    }

    [Fact]
    public void ConvertToMcp_WithCallableReference_UsesMethodObject()
    {
        var parameters = new Dictionary<string, object?>
        {
            ["callable"] = new CallableReference("OnReady", "PlayerNode")
        };

        var result = _converter.ConvertToMcp(parameters, CreateToolDefinition());
        var value = Assert.IsType<Dictionary<string, object?>>(result["callable"]);
        Assert.Equal("OnReady", value["method"]);
        Assert.Equal("PlayerNode", value["object"]);
    }

    #endregion

    #region ConvertToMcp - Complex Objects Tests

    [Fact]
    public void ConvertToMcp_WithGameObjectDefinition_SerializesToJsonElement()
    {
        // Arrange
        var gameObject = new GameObjectDefinition(
            Name: "TestObject",
            Position: new Vector3(1.0f, 2.0f, 3.0f),
            Rotation: new Vector3(0.0f, 90.0f, 0.0f),
            Scale: new Vector3(1.0f, 1.0f, 1.0f),
            Components: new List<ComponentDefinition>
            {
                new ComponentDefinition("MeshRenderer", new Dictionary<string, object?> { ["enabled"] = true })
            }
        );
        var parameters = new Dictionary<string, object?>
        {
            ["data"] = gameObject
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("data"));
        var jsonElement = Assert.IsType<JsonElement>(result["data"]);
        Assert.Equal(JsonValueKind.Object, jsonElement.ValueKind);
        Assert.Equal("TestObject", jsonElement.GetProperty("name").GetString());
    }

    #endregion

    #region ConvertToMcp - Collections and Arrays Tests

    [Fact]
    public void ConvertToMcp_WithIntArray_ReturnsListOfInts()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["numbers"] = new[] { 1, 2, 3, 4, 5 }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("numbers"));
        var list = Assert.IsType<List<object?>>(result["numbers"]);
        Assert.Equal(5, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(5, list[4]);
    }

    [Fact]
    public void ConvertToMcp_WithStringList_ReturnsListOfStrings()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["tags"] = new List<string> { "tag1", "tag2", "tag3" }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("tags"));
        var list = Assert.IsType<List<object?>>(result["tags"]);
        Assert.Equal(3, list.Count);
        Assert.Equal("tag1", list[0]);
        Assert.Equal("tag3", list[2]);
    }

    [Fact]
    public void ConvertToMcp_WithComplexObjectArray_SerializesEachElement()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["objects"] = new[]
            {
                new ComponentDefinition("Renderer", new Dictionary<string, object?> { ["enabled"] = true }),
                new ComponentDefinition("Collider", new Dictionary<string, object?> { ["isTrigger"] = false })
            }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("objects"));
        var list = Assert.IsType<List<object?>>(result["objects"]);
        Assert.Equal(2, list.Count);

        var firstElement = Assert.IsType<JsonElement>(list[0]);
        Assert.Equal("Renderer", firstElement.GetProperty("type").GetString());
    }

    [Fact]
    public void ConvertToMcp_WithMixedTypeCollection_HandlesEachTypeCorrectly()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["mixed"] = new object[] { 42, "text", 3.14, true }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("mixed"));
        var list = Assert.IsType<List<object?>>(result["mixed"]);
        Assert.Equal(4, list.Count);
        Assert.Equal(42, list[0]);
        Assert.Equal("text", list[1]);
        Assert.Equal(3.14, list[2]);
        Assert.Equal(true, list[3]);
    }

    [Fact]
    public void ConvertToMcp_WithEmptyArray_ReturnsEmptyList()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["empty"] = Array.Empty<int>()
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("empty"));
        var list = Assert.IsType<List<object?>>(result["empty"]);
        Assert.Empty(list);
    }

    #endregion

    #region ConvertToMcp - Null Values Tests

    [Fact]
    public void ConvertToMcp_WithNullValue_ReturnsNull()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["nullParam"] = null
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("nullParam"));
        Assert.Null(result["nullParam"]);
    }

    [Fact]
    public void ConvertToMcp_WithMultipleNullValues_ReturnsAllNulls()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["null1"] = null,
            ["null2"] = null,
            ["value"] = 42
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Null(result["null1"]);
        Assert.Null(result["null2"]);
        Assert.Equal(42, result["value"]);
    }

    [Fact]
    public void ConvertToMcp_WithCollectionContainingNulls_PreservesNulls()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["items"] = new object?[] { 1, null, "text", null, 5 }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("items"));
        var list = Assert.IsType<List<object?>>(result["items"]);
        Assert.Equal(5, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Null(list[1]);
        Assert.Equal("text", list[2]);
        Assert.Null(list[3]);
        Assert.Equal(5, list[4]);
    }

    #endregion

    #region ConvertFromMcp Tests

    [Fact]
    public void ConvertFromMcp_WithIntResult_ReturnsInt()
    {
        // Arrange
        var response = new McpResponse(
            Id: "test-1",
            Success: true,
            Result: 42
        );

        // Act
        var result = _converter.ConvertFromMcp<int>(response);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertFromMcp_WithStringResult_ReturnsString()
    {
        // Arrange
        var response = new McpResponse(
            Id: "test-2",
            Success: true,
            Result: "TestResult"
        );

        // Act
        var result = _converter.ConvertFromMcp<string>(response);

        // Assert
        Assert.Equal("TestResult", result);
    }

    [Fact]
    public void ConvertFromMcp_WithJsonElement_DeserializesCorrectly()
    {
        // Arrange
        var vector = new Vector3(1.0f, 2.0f, 3.0f);
        var jsonElement = JsonSerializer.SerializeToElement(vector, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var response = new McpResponse(
            Id: "test-3",
            Success: true,
            Result: jsonElement
        );

        // Act
        var result = _converter.ConvertFromMcp<Vector3>(response);

        // Assert
        Assert.Equal(1.0f, result.X);
        Assert.Equal(2.0f, result.Y);
        Assert.Equal(3.0f, result.Z);
    }

    [Fact]
    public void ConvertFromMcp_WithNullResult_ReturnsDefault()
    {
        // Arrange
        var response = new McpResponse(
            Id: "test-4",
            Success: true,
            Result: null
        );

        // Act
        var result = _converter.ConvertFromMcp<int>(response);

        // Assert
        Assert.Equal(0, result); // Default for int
    }

    [Fact]
    public void ConvertFromMcp_WithNullableType_ReturnsNull()
    {
        // Arrange
        var response = new McpResponse(
            Id: "test-5",
            Success: true,
            Result: null
        );

        // Act
        var result = _converter.ConvertFromMcp<int?>(response);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConvertFromMcp_WithComplexObject_DeserializesCorrectly()
    {
        // Arrange
        var gameObject = new GameObjectDefinition(
            Name: "Complex",
            Position: new Vector3(1.0f, 2.0f, 3.0f),
            Components: new List<ComponentDefinition>
            {
                new ComponentDefinition("Renderer", new Dictionary<string, object?> { ["enabled"] = true })
            }
        );
        var jsonElement = JsonSerializer.SerializeToElement(gameObject, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var response = new McpResponse(
            Id: "test-6",
            Success: true,
            Result: jsonElement
        );

        // Act
        var result = _converter.ConvertFromMcp<GameObjectDefinition>(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Complex", result.Name);
        Assert.NotNull(result.Position);
        Assert.Equal(1.0f, result.Position.Value.X);
        Assert.NotNull(result.Components);
        Assert.Single(result.Components);
    }

    [Fact]
    public void ConvertFromMcp_WithInvalidType_ThrowsTypeConversionException()
    {
        // Arrange
        var response = new McpResponse(
            Id: "test-7",
            Success: true,
            Result: "not a number"
        );

        // Act & Assert
        var exception = Assert.Throws<TypeConversionException>(() =>
            _converter.ConvertFromMcp<int>(response));

        Assert.Contains("Failed to convert MCP response to type", exception.Message);
        Assert.NotNull(exception.SourceType);
        Assert.NotNull(exception.TargetType);
    }

    #endregion

    #region Custom Converter Tests

    [Fact]
    public void RegisterConverter_WithCustomConverter_StoresConverter()
    {
        // Arrange
        var customConverter = new TestTypeConverter();

        // Act
        _converter.RegisterConverter(customConverter);

        // Assert - verify by using the converter
        var parameters = new Dictionary<string, object?>
        {
            ["custom"] = new TestType { Value = "Original" }
        };
        var toolDefinition = CreateToolDefinition();
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        Assert.NotNull(result);
        Assert.Equal("Converted: Original", result["custom"]);
    }

    [Fact]
    public void ConvertToMcp_WithRegisteredConverter_UsesCustomConverter()
    {
        // Arrange
        var customConverter = new TestTypeConverter();
        _converter.RegisterConverter(customConverter);

        var parameters = new Dictionary<string, object?>
        {
            ["custom"] = new TestType { Value = "TestValue" }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Converted: TestValue", result["custom"]);
    }

    [Fact]
    public void ConvertFromMcp_WithRegisteredConverter_UsesCustomConverter()
    {
        // Arrange
        var customConverter = new TestTypeConverter();
        _converter.RegisterConverter(customConverter);

        var response = new McpResponse(
            Id: "test-8",
            Success: true,
            Result: "Converted: FromMcp"
        );

        // Act
        var result = _converter.ConvertFromMcp<TestType>(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FromMcp", result.Value);
    }

    [Fact]
    public void RegisterConverter_MultipleConverters_AllWorkCorrectly()
    {
        // Arrange
        var converter1 = new TestTypeConverter();
        var converter2 = new AnotherTestTypeConverter();

        _converter.RegisterConverter(converter1);
        _converter.RegisterConverter(converter2);

        var parameters = new Dictionary<string, object?>
        {
            ["type1"] = new TestType { Value = "Value1" },
            ["type2"] = new AnotherTestType { Data = 42 }
        };
        var toolDefinition = CreateToolDefinition();

        // Act
        var result = _converter.ConvertToMcp(parameters, toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Converted: Value1", result["type1"]);
        Assert.Equal("Data: 42", result["type2"]);
    }

    #endregion

    #region Helper Methods and Test Classes

    private static McpToolDefinition CreateToolDefinition()
    {
        return new McpToolDefinition(
            Name: "test_tool",
            Description: "Test tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
    }

    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class ComplexTestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public NestedObject? Nested { get; set; }
    }

    private class NestedObject
    {
        public string Property { get; set; } = string.Empty;
    }

    private class TestType
    {
        public string Value { get; set; } = string.Empty;
    }

    private class AnotherTestType
    {
        public int Data { get; set; }
    }

    private class TestTypeConverter : ITypeConverter<TestType>
    {
        public object? ToMcp(TestType value)
        {
            return $"Converted: {value.Value}";
        }

        public TestType? FromMcp(object? mcpValue)
        {
            if (mcpValue is string str && str.StartsWith("Converted: "))
            {
                return new TestType { Value = str.Substring("Converted: ".Length) };
            }
            return null;
        }
    }

    private class AnotherTestTypeConverter : ITypeConverter<AnotherTestType>
    {
        public object? ToMcp(AnotherTestType value)
        {
            return $"Data: {value.Data}";
        }

        public AnotherTestType? FromMcp(object? mcpValue)
        {
            if (mcpValue is string str && str.StartsWith("Data: "))
            {
                var data = int.Parse(str.Substring("Data: ".Length));
                return new AnotherTestType { Data = data };
            }
            return null;
        }
    }

    #endregion
}
