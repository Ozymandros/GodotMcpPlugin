using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Resource extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientResourceExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task ResourceListAsync_MapsPayloadAndReturnsTypedList()
    {
        _client
            .InvokeToolAsync("resource.list", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new[] { new { path = "res://materials/mat.tres", type = "StandardMaterial3D", name = "mat", exists = true } }));

        var result = await _client.ResourceListAsync(new ResourceListRequest("res://materials", "StandardMaterial3D"));

        Assert.Single(result);
        Assert.Equal("res://materials/mat.tres", result[0].Path);
        Assert.Equal("StandardMaterial3D", result[0].Type);

        await _client.Received(1).InvokeToolAsync(
            "resource.list",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["directory"], "res://materials") &&
                Equals(d["resourceType"], "StandardMaterial3D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceReadAsync_MapsPayloadAndReturnsTypedResourceData()
    {
        _client
            .InvokeToolAsync("resource.read", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new
                {
                    path = "res://materials/mat.tres",
                    type = "StandardMaterial3D",
                    properties = new Dictionary<string, object?>
                    {
                        ["albedoColor"] = "#ffffff",
                        ["metallic"] = 0.25
                    }
                }));

        var result = await _client.ResourceReadAsync(new ResourceReadRequest("res://materials/mat.tres"));

        Assert.NotNull(result);
        Assert.Equal("res://materials/mat.tres", result!.Path);
        Assert.Equal("StandardMaterial3D", result.Type);
        Assert.True(result.Properties.ContainsKey("albedoColor"));

        await _client.Received(1).InvokeToolAsync(
            "resource.read",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => Equals(d["resourcePath"], "res://materials/mat.tres")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceUpdateAsync_MapsPayloadAndReturnsTypedResourceData()
    {
        var properties = new Dictionary<string, object?> { ["metallic"] = 0.8 };

        _client
            .InvokeToolAsync("resource.update", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-3",
                true,
                new
                {
                    path = "res://materials/mat.tres",
                    type = "StandardMaterial3D",
                    properties = new Dictionary<string, object?> { ["metallic"] = 0.8 }
                }));

        var result = await _client.ResourceUpdateAsync(new ResourceUpdateRequest("res://materials/mat.tres", properties));

        Assert.NotNull(result);
        Assert.Equal("res://materials/mat.tres", result!.Path);

        await _client.Received(1).InvokeToolAsync(
            "resource.update",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["resourcePath"], "res://materials/mat.tres") &&
                ReferenceEquals(d["properties"], properties)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceCreateAsync_MapsPayloadAndReturnsTypedResourceInfo()
    {
        var properties = new Dictionary<string, object?> { ["size"] = 512 };

        _client
            .InvokeToolAsync("resource.create", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-4",
                true,
                new
                {
                    path = "res://textures/new_texture.tres",
                    type = "ImageTexture",
                    name = "new_texture",
                    exists = true
                }));

        var result = await _client.ResourceCreateAsync(
            new ResourceCreateRequest("res://textures/new_texture.tres", "ImageTexture", properties));

        Assert.NotNull(result);
        Assert.Equal("new_texture", result!.Name);

        await _client.Received(1).InvokeToolAsync(
            "resource.create",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["resourcePath"], "res://textures/new_texture.tres") &&
                Equals(d["resourceType"], "ImageTexture") &&
                ReferenceEquals(d["properties"], properties)),
            Arg.Any<CancellationToken>());
    }
}
