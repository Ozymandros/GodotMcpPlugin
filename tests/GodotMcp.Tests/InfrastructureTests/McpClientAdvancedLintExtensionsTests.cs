using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for advanced lint extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientAdvancedLintExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task LintSceneAdvancedAsync_ReturnsLintResult()
    {
        _client.InvokeToolAsync("lint.scene_advanced", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "1",
                true,
                new
                {
                    issues = new[] { new { code = "L001", severity = "warning", message = "Unused node", path = "./Temp" } },
                    success = true
                }));

        var result = await _client.LintSceneAdvancedAsync(new LintSceneAdvancedRequest("res://scenes/main.tscn"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Single(result.Issues);
        Assert.Equal("L001", result.Issues[0].Code);
    }

    [Fact]
    public async Task LintProjectAsync_UsesBasicProjectLintCommand()
    {
        _client.InvokeToolAsync("lint_project", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "2",
                true,
                new
                {
                    issues = new[] { new { code = "LP001", severity = "warning", message = "Project warning", path = "res://" } },
                    success = true
                }));

        var result = await _client.LintProjectAsync(new LintProjectRequest("res://"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Single(result.Issues);
        Assert.Equal("LP001", result.Issues[0].Code);
    }

    [Fact]
    public async Task LintProjectAsync_FallsBackToAdvancedCommand_WhenBasicMissing()
    {
        _client.InvokeToolAsync("lint_project", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns<Task<McpResponse>>(_ => throw new McpServerException("Tool not found", -32601));

        _client.InvokeToolAsync("lint.project_advanced", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "3",
                true,
                new
                {
                    issues = new[] { new { code = "LP002", severity = "info", message = "Advanced project lint", path = "res://" } },
                    success = true
                }));

        var result = await _client.LintProjectAsync(new LintProjectRequest("res://"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("LP002", result.Issues[0].Code);

        await _client.Received(1).InvokeToolAsync("lint.project_advanced", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
    }
}
