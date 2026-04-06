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
}
