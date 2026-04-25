using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Conversion;
using GodotMcp.Plugin;

namespace GodotMcp.Tests.PluginTests;

public class GodotPluginGeneratedContentTests
{
    [Fact]
    public async Task InvokeToolAsync_ForwardsGeneratedTextParameter()
    {
        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var parameterConverter = new ParameterConverter(NullLogger<ParameterConverter>.Instance);

        var plugin = new GodotPlugin(mockClient, mockMapper, parameterConverter, NullLogger<GodotPlugin>.Instance);

        var toolName = "Godot_create_script";
        var paramName = "rawContent";
        var largeText = string.Concat(Enumerable.Repeat("// generated code\n", 50));

        var toolDefinition = new McpToolDefinition(
            toolName,
            "Creates a script",
            new Dictionary<string, McpParameterDefinition>
            {
                [paramName] = new McpParameterDefinition(paramName, "string", "Generated script content", true)
            });

        mockMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        mockMapper.GetToolDefinition(toolName).Returns(toolDefinition);

        mockClient.InvokeToolAsync(toolName, Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("1", true, new { path = "scripts/new.gd" }));

        var parameters = new Dictionary<string, object?> { [paramName] = largeText };

        var result = await plugin.InvokeToolAsync(toolName, parameters);

        await mockClient.Received(1).InvokeToolAsync(
            toolName,
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => d.ContainsKey(paramName) && d[paramName]!.Equals(largeText)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeToolAsync_ForwardsComplexParameter_AsJsonElement()
    {
        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var parameterConverter = new ParameterConverter(NullLogger<ParameterConverter>.Instance);

        var plugin = new GodotPlugin(mockClient, mockMapper, parameterConverter, NullLogger<GodotPlugin>.Instance);

        var toolName = "Godot_create_resource";
        var paramName = "properties";
        var propertiesValue = new { content = "some generated data", version = 1 };

        var toolDefinition = new McpToolDefinition(
            toolName,
            "Creates a resource",
            new Dictionary<string, McpParameterDefinition>
            {
                [paramName] = new McpParameterDefinition(paramName, "object", "Properties", true)
            });

        mockMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        mockMapper.GetToolDefinition(toolName).Returns(toolDefinition);

        mockClient.InvokeToolAsync(toolName, Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("2", true, true));

        var parameters = new Dictionary<string, object?> { [paramName] = propertiesValue };

        var result = await plugin.InvokeToolAsync(toolName, parameters);

        await mockClient.Received(1).InvokeToolAsync(
            toolName,
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => d.ContainsKey(paramName) && d[paramName] is System.Text.Json.JsonElement),
            Arg.Any<CancellationToken>());
    }
}
