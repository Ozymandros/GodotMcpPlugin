using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Navigation extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientNavigationExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task NavigationListRegionsAsync_MapsPayloadAndReturnsTypedList()
    {
        _client.InvokeToolAsync("nav.list_regions", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("1", true, new[] { new { name = "RegionA", path = "./RegionA", enabled = true } }));

        var result = await _client.NavigationListRegionsAsync(
            new NavigationListRegionsRequest(new McpProjectFile("res://", "scenes/main.tscn")));

        Assert.Single(result);
        Assert.Equal("RegionA", result[0].Name);
    }

    [Fact]
    public async Task NavigationBakeAsync_ReturnsResult()
    {
        _client.InvokeToolAsync("nav.bake", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("2", true, new { success = true, message = "Baked" }));

        var result = await _client.NavigationBakeAsync(
            new NavigationBakeRequest(new McpProjectFile("res://", "scenes/main.tscn")));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Baked", result.Message);
    }
}
