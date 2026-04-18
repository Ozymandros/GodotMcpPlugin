using GodotMcp.Infrastructure.Client;
using GodotMcp.Plugin.Extensions;
using Microsoft.SemanticKernel;

namespace GodotMcp.Tests.PluginTests;

/// <summary>
/// Tests for typed skill module registration and invocation paths.
/// </summary>
public class KernelSkillRegistrationTests
{
    [Fact]
    public async Task AddGodotMcpSkills_RegistersAllSkillPlugins()
    {
        var mcp = Substitute.For<IMcpClient>();
        mcp.InvokeToolAsync("scene.list_nodes", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("1", true, Array.Empty<object>()));

        var kernel = Kernel.CreateBuilder().Build();
        kernel.AddGodotMcpSkills(mcp);

        Assert.NotNull(kernel.Plugins["scene"]);
        Assert.NotNull(kernel.Plugins["project"]);
        Assert.NotNull(kernel.Plugins["resource"]);
        Assert.NotNull(kernel.Plugins["script"]);
        Assert.NotNull(kernel.Plugins["import"]);
        Assert.NotNull(kernel.Plugins["camera"]);
        Assert.NotNull(kernel.Plugins["ui"]);
        Assert.NotNull(kernel.Plugins["light"]);
        Assert.NotNull(kernel.Plugins["physics"]);
        Assert.NotNull(kernel.Plugins["nav"]);
        Assert.NotNull(kernel.Plugins["lint"]);
        Assert.NotNull(kernel.Plugins["preset"]);

        var sceneFunc = kernel.Plugins["scene"]["list_nodes"];
        await kernel.InvokeAsync(sceneFunc, new KernelArguments
        {
            ["projectPath"] = "res://",
            ["fileName"] = "scenes/main.tscn"
        });

        await mcp.Received(1).InvokeToolAsync(
            "scene.list_nodes",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://") &&
                Equals(d["fileName"], "scenes/main.tscn")),
            Arg.Any<CancellationToken>());
    }
}
