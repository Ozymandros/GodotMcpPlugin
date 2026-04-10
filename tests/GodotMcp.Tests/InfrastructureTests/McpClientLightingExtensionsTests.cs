using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Lighting extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientLightingExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task LightListAsync_MapsPayloadAndReturnsTypedList()
    {
        _client
            .InvokeToolAsync("light.list", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new[] { new { name = "Sun", path = "./Sun", lightType = "DirectionalLight3D", enabled = true } }));

        var result = await _client.LightListAsync(new LightListRequest("res://scenes/main.tscn"));

        Assert.Single(result);
        Assert.Equal("DirectionalLight3D", result[0].LightType);

        await _client.Received(1).InvokeToolAsync(
            "light.list",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => Equals(d["scenePath"], "res://scenes/main.tscn")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LightCreateAsync_MapsPayloadAndReturnsTypedLight()
    {
        _client
            .InvokeToolAsync("light.create", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new { name = "Fill", path = "./Fill", lightType = "OmniLight3D", enabled = true }));

        var result = await _client.LightCreateAsync(
            new LightCreateRequest("res://scenes/main.tscn", ".", "Fill", "OmniLight3D"));

        Assert.NotNull(result);
        Assert.Equal("Fill", result!.Name);

        await _client.Received(1).InvokeToolAsync(
            "light.create",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["scenePath"], "res://scenes/main.tscn") &&
                Equals(d["parentPath"], ".") &&
                Equals(d["lightName"], "Fill") &&
                Equals(d["lightType"], "OmniLight3D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LightValidateAsync_MapsPayloadAndReturnsValidationResult()
    {
        _client
            .InvokeToolAsync("light.validate", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-3", true, new { success = true, message = "Lighting valid" }));

        var result = await _client.LightValidateAsync(new LightValidateRequest("res://scenes/main.tscn"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Lighting valid", result.Message);
    }

    [Fact]
    public async Task LightTuneAsync_MapsPayloadAndReturnsTunedLight()
    {
        _client
            .InvokeToolAsync("light.tune", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-4",
                true,
                new { name = "Sun", path = "./Sun", lightType = "DirectionalLight3D", enabled = true }));

        var result = await _client.LightTuneAsync(
            new LightTuneRequest(
                "res://scenes/main.tscn",
                "./Sun",
                new Dictionary<string, object?> { ["energy"] = 3.2 }));

        Assert.NotNull(result);
        Assert.Equal("Sun", result!.Name);
    }
}
