using GodotMcp.Core.Models;
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
        mcp.InvokeToolAsync("query_system_documentation", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "doc-1",
                true,
                new QuerySystemDocumentationMcpResponse { Success = true, RepositoryRoot = @"C:\McpRepo" }));

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
        Assert.NotNull(kernel.Plugins["docs"]);

        var sceneFunc = kernel.Plugins["scene"]["list_nodes"];
        await kernel.InvokeAsync(sceneFunc, new KernelArguments
        {
            ["projectPath"] = Root,
            ["fileName"] = "scenes/main.tscn"
        });

        await mcp.Received(1).InvokeToolAsync(
            "scene.list_nodes",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/main.tscn")),
            Arg.Any<CancellationToken>());

        var docsFunc = kernel.Plugins["docs"]["query_system_documentation"];
        await kernel.InvokeAsync(docsFunc, new KernelArguments
        {
            ["projectPath"] = Root,
            ["query"] = "",
            ["repositoryRoot"] = null,
            ["source"] = "both"
        });

        await mcp.Received(1).InvokeToolAsync(
            "query_system_documentation",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["query"], "") &&
                d["repositoryRoot"] == null &&
                Equals(d["source"], "both")),
            Arg.Any<CancellationToken>());
    }
}
