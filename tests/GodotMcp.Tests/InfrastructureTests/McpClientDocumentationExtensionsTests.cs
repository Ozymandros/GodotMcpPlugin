using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

public class McpClientDocumentationExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task QuerySystemDocumentationAsync_InvokesToolWithExpectedParameters()
    {
        _client
            .InvokeToolAsync("query_system_documentation", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "1",
                true,
                new QuerySystemDocumentationMcpResponse
                {
                    Success = true,
                    RepositoryRoot = @"C:\repo",
                    Manifest = default,
                    Markdown = default
                }));

        var req = new QuerySystemDocumentationRequest(Root, "plugin", source: DocumentationMcpValidation.SourceManifest);
        var result = await _client.QuerySystemDocumentationAsync(req);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(@"C:\repo", result.Data!.RepositoryRoot);

        await _client.Received(1).InvokeToolAsync(
            "query_system_documentation",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["query"], "plugin") &&
                d["repositoryRoot"] == null &&
                Equals(d["source"], DocumentationMcpValidation.SourceManifest)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryGodotEngineDocumentationAsync_UsesMaxResultsAndVersion()
    {
        _client
            .InvokeToolAsync("query_godot_engine_documentation", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "2",
                true,
                new QueryGodotEngineDocumentationMcpResponse
                {
                    Success = true,
                    Query = "Area2D",
                    Version = "stable",
                    TotalCount = 0,
                    Hits = []
                }));

        var req = new QueryGodotEngineDocumentationRequest(Root, "Area2D", "stable", 40);
        var result = await _client.QueryGodotEngineDocumentationAsync(req);

        Assert.True(result.Success);

        await _client.Received(1).InvokeToolAsync(
            "query_godot_engine_documentation",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["query"], "Area2D") &&
                Equals(d["version"], "stable") &&
                Equals(d["max_results"], 40)),
            Arg.Any<CancellationToken>());
    }
}
