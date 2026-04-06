using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using GodotMcp.Core.Models;
using GodotMcp.Plugin.Mapping;

namespace GodotMcp.Tests.PluginTests;

public class FunctionMapperTests
{
    private readonly FunctionMapper _mapper;

    public FunctionMapperTests()
    {
        _mapper = new FunctionMapper(NullLogger<FunctionMapper>.Instance);
    }

    #region MapToKernelFunction Tests

    [Fact]
    public void MapToKernelFunction_WithBasicTool_CreatesKernelFunctionMetadata()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test_function",
            Description: "A test function",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test_function", result.Name);
        Assert.Equal("A test function", result.Description);
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public void MapToKernelFunction_WithRequiredParameter_SetsIsRequiredTrue()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "create_object",
            Description: "Creates an object",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["name"] = new McpParameterDefinition(
                    Name: "name",
                    Type: "string",
                    Description: "Object name",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        var param = result.Parameters[0];
        Assert.Equal("name", param.Name);
        Assert.Equal("Object name", param.Description);
        Assert.True(param.IsRequired);
        Assert.Equal(typeof(string), param.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithOptionalParameter_SetsIsRequiredFalse()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "create_object",
            Description: "Creates an object",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["tag"] = new McpParameterDefinition(
                    Name: "tag",
                    Type: "string",
                    Description: "Optional tag",
                    Required: false
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        var param = result.Parameters[0];
        Assert.Equal("tag", param.Name);
        Assert.False(param.IsRequired);
    }

    [Fact]
    public void MapToKernelFunction_WithDefaultValue_SetsDefaultValue()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "set_speed",
            Description: "Sets speed",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["speed"] = new McpParameterDefinition(
                    Name: "speed",
                    Type: "number",
                    Description: "Speed value",
                    Required: false,
                    DefaultValue: 1.0
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        var param = result.Parameters[0];
        Assert.Equal("speed", param.Name);
        Assert.Equal(1.0, param.DefaultValue);
    }

    [Fact]
    public void MapToKernelFunction_WithMultipleParameters_MapsAllParameters()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "complex_function",
            Description: "A complex function",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["name"] = new McpParameterDefinition(
                    Name: "name",
                    Type: "string",
                    Description: "Name parameter",
                    Required: true
                ),
                ["count"] = new McpParameterDefinition(
                    Name: "count",
                    Type: "integer",
                    Description: "Count parameter",
                    Required: false,
                    DefaultValue: 1
                ),
                ["enabled"] = new McpParameterDefinition(
                    Name: "enabled",
                    Type: "boolean",
                    Description: "Enabled flag",
                    Required: false,
                    DefaultValue: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Parameters.Count);

        var nameParam = result.Parameters.First(p => p.Name == "name");
        Assert.True(nameParam.IsRequired);
        Assert.Equal(typeof(string), nameParam.ParameterType);

        var countParam = result.Parameters.First(p => p.Name == "count");
        Assert.False(countParam.IsRequired);
        Assert.Equal(typeof(int), countParam.ParameterType);
        Assert.Equal(1, countParam.DefaultValue);

        var enabledParam = result.Parameters.First(p => p.Name == "enabled");
        Assert.False(enabledParam.IsRequired);
        Assert.Equal(typeof(bool), enabledParam.ParameterType);
        Assert.Equal(true, enabledParam.DefaultValue);
    }

    [Fact]
    public void MapToKernelFunction_WithReturnType_CreatesReturnParameterMetadata()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "get_value",
            Description: "Gets a value",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "string",
                Description: "The returned value"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal("The returned value", result.ReturnParameter.Description);
        Assert.Equal(typeof(string), result.ReturnParameter.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithoutReturnType_HasDefaultReturnParameter()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "void_function",
            Description: "A void function",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: null
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter); // Should have a default return parameter, not null
    }

    [Fact]
    public void MapToKernelFunction_WithNumberReturnType_MapsToDouble()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "calculate",
            Description: "Calculates a value",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "number",
                Description: "Calculated result"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal(typeof(double), result.ReturnParameter.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithIntegerReturnType_MapsToInt()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "count_items",
            Description: "Counts items",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "integer",
                Description: "Item count"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal(typeof(int), result.ReturnParameter.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithBooleanReturnType_MapsToBool()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "is_valid",
            Description: "Checks validity",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "boolean",
                Description: "Validity status"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal(typeof(bool), result.ReturnParameter.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithObjectReturnType_MapsToObject()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "get_data",
            Description: "Gets data",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "object",
                Description: "Data object"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal(typeof(object), result.ReturnParameter.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithArrayReturnType_MapsToObjectArray()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "get_items",
            Description: "Gets items",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "array",
                Description: "Item array"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal(typeof(object[]), result.ReturnParameter.ParameterType);
    }

    #endregion

    #region MapMcpTypeToClrType Tests

    [Fact]
    public void MapToKernelFunction_WithStringType_MapsToStringType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "string",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(string), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithNumberType_MapsToDoubleType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "number",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(double), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithIntegerType_MapsToIntType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "integer",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(int), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithBooleanType_MapsToBoolType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "boolean",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(bool), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithObjectType_MapsToObjectType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "object",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(object), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithArrayType_MapsToObjectArrayType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "array",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(object[]), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithUnknownType_MapsToObjectType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param"] = new McpParameterDefinition(
                    Name: "param",
                    Type: "unknown_type",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(object), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithUnionParameterType_MapsToFirstKnownType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "set_camera",
            Description: "Sets camera property",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["fov"] = new McpParameterDefinition(
                    Name: "fov",
                    Type: "number|null",
                    Required: true
                )
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(double), result.Parameters[0].ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithUnionReturnType_MapsToFirstKnownType()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "get_camera_mode",
            Description: "Gets camera mode",
            Parameters: new Dictionary<string, McpParameterDefinition>(),
            ReturnType: new McpReturnType(
                Type: "integer|string",
                Description: "Camera mode"
            )
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.NotNull(result.ReturnParameter);
        Assert.Equal(typeof(int), result.ReturnParameter.ParameterType);
    }

    [Fact]
    public void MapToKernelFunction_WithMixedCaseType_HandlesCorrectly()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test",
            Description: "Test",
            Parameters: new Dictionary<string, McpParameterDefinition>
            {
                ["param1"] = new McpParameterDefinition(Name: "param1", Type: "STRING", Required: true),
                ["param2"] = new McpParameterDefinition(Name: "param2", Type: "Number", Required: true),
                ["param3"] = new McpParameterDefinition(Name: "param3", Type: "BOOLEAN", Required: true)
            }
        );

        // Act
        var result = _mapper.MapToKernelFunction(toolDefinition);

        // Assert
        Assert.Equal(typeof(string), result.Parameters.First(p => p.Name == "param1").ParameterType);
        Assert.Equal(typeof(double), result.Parameters.First(p => p.Name == "param2").ParameterType);
        Assert.Equal(typeof(bool), result.Parameters.First(p => p.Name == "param3").ParameterType);
    }

    #endregion

    #region GetToolDefinition Tests

    [Fact]
    public async Task GetToolDefinition_WithRegisteredTool_ReturnsToolDefinition()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "test_tool",
            Description: "Test tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tools = new List<McpToolDefinition> { toolDefinition };
        await _mapper.RegisterToolsAsync(tools);

        // Act
        var result = _mapper.GetToolDefinition("test_tool");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test_tool", result.Name);
        Assert.Equal("Test tool", result.Description);
    }

    [Fact]
    public async Task GetToolDefinition_WithUnknownTool_ReturnsNull()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "known_tool",
            Description: "Known tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tools = new List<McpToolDefinition> { toolDefinition };
        await _mapper.RegisterToolsAsync(tools);

        // Act
        var result = _mapper.GetToolDefinition("unknown_tool");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetToolDefinition_WithEmptyRegistry_ReturnsNull()
    {
        // Arrange - no tools registered

        // Act
        var result = _mapper.GetToolDefinition("any_tool");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetToolDefinition_WithMultipleTools_ReturnsCorrectTool()
    {
        // Arrange
        var tool1 = new McpToolDefinition(
            Name: "tool1",
            Description: "First tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tool2 = new McpToolDefinition(
            Name: "tool2",
            Description: "Second tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tool3 = new McpToolDefinition(
            Name: "tool3",
            Description: "Third tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tools = new List<McpToolDefinition> { tool1, tool2, tool3 };
        await _mapper.RegisterToolsAsync(tools);

        // Act
        var result = _mapper.GetToolDefinition("tool2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("tool2", result.Name);
        Assert.Equal("Second tool", result.Description);
    }

    [Fact]
    public async Task GetToolDefinition_IsCaseSensitive()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "TestTool",
            Description: "Test tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tools = new List<McpToolDefinition> { toolDefinition };
        await _mapper.RegisterToolsAsync(tools);

        // Act
        var result1 = _mapper.GetToolDefinition("TestTool");
        var result2 = _mapper.GetToolDefinition("testtool");
        var result3 = _mapper.GetToolDefinition("TESTTOOL");

        // Assert
        Assert.NotNull(result1);
        Assert.Null(result2);
        Assert.Null(result3);
    }

    #endregion

    #region RegisterToolsAsync Tests

    [Fact]
    public async Task RegisterToolsAsync_WithSingleTool_StoresToolCorrectly()
    {
        // Arrange
        var toolDefinition = new McpToolDefinition(
            Name: "single_tool",
            Description: "Single tool",
            Parameters: new Dictionary<string, McpParameterDefinition>()
        );
        var tools = new List<McpToolDefinition> { toolDefinition };

        // Act
        await _mapper.RegisterToolsAsync(tools);

        // Assert
        var result = _mapper.GetToolDefinition("single_tool");
        Assert.NotNull(result);
        Assert.Equal("single_tool", result.Name);
    }

    [Fact]
    public async Task RegisterToolsAsync_WithMultipleTools_StoresAllTools()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool1", "First", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool2", "Second", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool3", "Third", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool4", "Fourth", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool5", "Fifth", new Dictionary<string, McpParameterDefinition>())
        };

        // Act
        await _mapper.RegisterToolsAsync(tools);

        // Assert
        Assert.NotNull(_mapper.GetToolDefinition("tool1"));
        Assert.NotNull(_mapper.GetToolDefinition("tool2"));
        Assert.NotNull(_mapper.GetToolDefinition("tool3"));
        Assert.NotNull(_mapper.GetToolDefinition("tool4"));
        Assert.NotNull(_mapper.GetToolDefinition("tool5"));
    }

    [Fact]
    public async Task RegisterToolsAsync_WithEmptyList_CompletesSuccessfully()
    {
        // Arrange
        var tools = new List<McpToolDefinition>();

        // Act
        await _mapper.RegisterToolsAsync(tools);

        // Assert
        var result = _mapper.GetToolDefinition("any_tool");
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterToolsAsync_CalledTwice_ReplacesOldTools()
    {
        // Arrange
        var firstTools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool1", "First", new Dictionary<string, McpParameterDefinition>())
        };
        var secondTools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool2", "Second", new Dictionary<string, McpParameterDefinition>())
        };

        // Act
        await _mapper.RegisterToolsAsync(firstTools);
        await _mapper.RegisterToolsAsync(secondTools);

        // Assert
        Assert.Null(_mapper.GetToolDefinition("tool1")); // Old tool should be gone
        Assert.NotNull(_mapper.GetToolDefinition("tool2")); // New tool should exist
    }

    [Fact]
    public async Task RegisterToolsAsync_WithComplexTools_PreservesAllMetadata()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition(
                Name: "complex_tool",
                Description: "A complex tool with many parameters",
                Parameters: new Dictionary<string, McpParameterDefinition>
                {
                    ["name"] = new McpParameterDefinition("name", "string", "Name param", true),
                    ["count"] = new McpParameterDefinition("count", "integer", "Count param", false, 10),
                    ["enabled"] = new McpParameterDefinition("enabled", "boolean", "Enabled flag", false, true)
                },
                ReturnType: new McpReturnType("object", "Complex result")
            )
        };

        // Act
        await _mapper.RegisterToolsAsync(tools);

        // Assert
        var result = _mapper.GetToolDefinition("complex_tool");
        Assert.NotNull(result);
        Assert.Equal("complex_tool", result.Name);
        Assert.Equal("A complex tool with many parameters", result.Description);
        Assert.Equal(3, result.Parameters.Count);
        Assert.NotNull(result.ReturnType);
        Assert.Equal("object", result.ReturnType.Type);
    }

    [Fact]
    public async Task RegisterToolsAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool1", "First", new Dictionary<string, McpParameterDefinition>())
        };
        using var cts = new CancellationTokenSource();

        // Act
        await _mapper.RegisterToolsAsync(tools, cts.Token);

        // Assert
        Assert.NotNull(_mapper.GetToolDefinition("tool1"));
    }

    #endregion

    #region GetRegisteredTools Tests

    [Fact]
    public void GetRegisteredTools_WithEmptyRegistry_ReturnsEmptyList()
    {
        var result = _mapper.GetRegisteredTools();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRegisteredTools_ReturnsAllRegisteredTools()
    {
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool_b", "B tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool_a", "A tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool_c", "C tool", new Dictionary<string, McpParameterDefinition>())
        };
        await _mapper.RegisterToolsAsync(tools);

        var result = _mapper.GetRegisteredTools();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetRegisteredTools_ReturnsDeterministicOrder()
    {
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("zebra_tool", "Z", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("alpha_tool", "A", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("mid_tool", "M", new Dictionary<string, McpParameterDefinition>())
        };
        await _mapper.RegisterToolsAsync(tools);

        var result = _mapper.GetRegisteredTools();

        Assert.Equal("alpha_tool", result[0].Name);
        Assert.Equal("mid_tool", result[1].Name);
        Assert.Equal("zebra_tool", result[2].Name);
    }

    [Fact]
    public async Task GetRegisteredTools_PreservesToolMetadata()
    {
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition(
                "complex_tool",
                "A complex tool",
                new Dictionary<string, McpParameterDefinition>
                {
                    ["name"] = new McpParameterDefinition("name", "string", "Name param", true)
                },
                ReturnType: new McpReturnType("object", "Result"))
        };
        await _mapper.RegisterToolsAsync(tools);

        var result = _mapper.GetRegisteredTools();

        Assert.Single(result);
        Assert.Equal("complex_tool", result[0].Name);
        Assert.Equal("A complex tool", result[0].Description);
        Assert.Single(result[0].Parameters);
        Assert.NotNull(result[0].ReturnType);
    }

    #endregion
}
