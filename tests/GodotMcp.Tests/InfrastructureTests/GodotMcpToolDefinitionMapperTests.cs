using System.Reflection;
using System.Text.Json;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

public class GodotMcpToolDefinitionMapperTests
{
    [Fact]
    public void ParseInputSchema_WithNullableTypeArray_MapsToPrimaryType()
    {
        var schemaJson =
            """
            {
              "properties": {
                "fov": {
                  "type": ["number", "null"],
                  "description": "Camera field of view",
                  "default": 75.0
                }
              },
              "required": ["fov"]
            }
            """;

        var parameters = InvokeParseInputSchema(schemaJson);

        Assert.Single(parameters);
        Assert.True(parameters.ContainsKey("fov"));

        var definition = parameters["fov"];
        Assert.Equal("number", definition.Type);
        Assert.True(definition.Required);
        Assert.Equal("Camera field of view", definition.Description);
        Assert.Equal(75d, Convert.ToDouble(definition.DefaultValue));
    }

    [Fact]
    public void ParseInputSchema_WithOneOfSchema_MapsToFirstConcreteType()
    {
        var schemaJson =
            """
            {
              "properties": {
                "near": {
                  "oneOf": [
                    { "type": "null" },
                    { "type": "number" }
                  ]
                }
              }
            }
            """;

        var parameters = InvokeParseInputSchema(schemaJson);

        Assert.Single(parameters);
        Assert.Equal("number", parameters["near"].Type);
    }

    [Fact]
    public void ParseInputSchema_WithObjectDefault_ParsesStructuredDefaultValue()
    {
        var schemaJson =
            """
            {
              "properties": {
                "cameraSettings": {
                  "type": "object",
                  "default": {
                    "fov": 70,
                    "projection": "perspective",
                    "flags": [1, 2]
                  }
                }
              }
            }
            """;

        var parameters = InvokeParseInputSchema(schemaJson);

        var defaultValue = Assert.IsType<Dictionary<string, object?>>(parameters["cameraSettings"].DefaultValue);
        Assert.Equal(70, Convert.ToInt32(defaultValue["fov"]));
        Assert.Equal("perspective", defaultValue["projection"]);

        var flags = Assert.IsType<List<object?>>(defaultValue["flags"]);
        Assert.Equal(2, flags.Count);
        Assert.Equal(1, Convert.ToInt32(flags[0]));
        Assert.Equal(2, Convert.ToInt32(flags[1]));
    }

    private static IReadOnlyDictionary<string, McpParameterDefinition> InvokeParseInputSchema(string schemaJson)
    {
        using var doc = JsonDocument.Parse(schemaJson);

        var method = typeof(GodotMcpToolDefinitionMapper).GetMethod(
            "ParseInputSchema",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method!.Invoke(null, [doc.RootElement]);
        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, McpParameterDefinition>>(result);
    }
}
