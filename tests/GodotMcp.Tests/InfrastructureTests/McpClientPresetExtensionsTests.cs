using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for preset extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientPresetExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task PresetApplyAsync_ReturnsPresetResult()
    {
        _client.InvokeToolAsync("preset.apply", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("1", true, new { success = true, message = "Applied", appliedToPath = "./Player" }));

        var result = await _client.PresetApplyAsync(new PresetApplyRequest("res://scenes/main.tscn", "./Player", "player_default"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("./Player", result.AppliedToPath);
    }
}
