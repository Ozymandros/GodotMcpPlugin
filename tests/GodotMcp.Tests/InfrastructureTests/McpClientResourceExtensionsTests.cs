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
        var matTres = Combine("materials", "mat.tres");
        _client
            .InvokeToolAsync("resource.list", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new[] { new { path = matTres, type = "StandardMaterial3D", name = "mat", exists = true } }));

        var materialsDir = Combine("materials");
        var result = await _client.ResourceListAsync(new ResourceListRequest(materialsDir, "StandardMaterial3D"));

        Assert.Single(result);
        Assert.Equal(matTres, result[0].Path);
        Assert.Equal("StandardMaterial3D", result[0].Type);

        await _client.Received(1).InvokeToolAsync(
            "resource.list",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["directory"], materialsDir) &&
                Equals(d["resourceType"], "StandardMaterial3D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceReadAsync_MapsPayloadAndReturnsTypedResourceData()
    {
        var matTres = Combine("materials", "mat.tres");
        _client
            .InvokeToolAsync("resource.read", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new
                {
                    path = matTres,
                    type = "StandardMaterial3D",
                    properties = new Dictionary<string, object?>
                    {
                        ["albedoColor"] = "#ffffff",
                        ["metallic"] = 0.25
                    }
                }));

        var result = await _client.ResourceReadAsync(
            new ResourceReadRequest(new McpProjectFile(Root, "materials/mat.tres")));

        Assert.NotNull(result);
        Assert.Equal(matTres, result!.Path);
        Assert.Equal("StandardMaterial3D", result.Type);
        Assert.True(result.Properties.ContainsKey("albedoColor"));

        await _client.Received(1).InvokeToolAsync(
            "resource.read",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "materials/mat.tres")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceUpdateAsync_MapsPayloadAndReturnsTypedResourceData()
    {
        var matTres = Combine("materials", "mat.tres");
        var properties = new Dictionary<string, object?> { ["metallic"] = 0.8 };

        _client
            .InvokeToolAsync("resource.update_properties", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-3",
                true,
                new
                {
                    path = matTres,
                    type = "StandardMaterial3D",
                    properties = new Dictionary<string, object?> { ["metallic"] = 0.8 }
                }));

        var result = await _client.ResourceUpdateAsync(
            new ResourceUpdateRequest(new McpProjectFile(Root, "materials/mat.tres"), properties));

        Assert.NotNull(result);
        Assert.Equal(matTres, result!.Path);

        await _client.Received(1).InvokeToolAsync(
            "resource.update_properties",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "materials/mat.tres") &&
                ((Dictionary<string, string>)d["properties"]!).ContainsKey("metallic")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceCreateAsync_MapsPayloadAndReturnsTypedResourceInfo()
    {
        var properties = new Dictionary<string, object?> { ["size"] = 512 };

        _client
            .InvokeToolAsync("create_resource", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-4",
                true,
                new
                {
                    path = Combine("textures", "new_texture.tres"),
                    type = "ImageTexture",
                    name = "new_texture",
                    exists = true
                }));

        var result = await _client.ResourceCreateAsync(
            new ResourceCreateRequest(new McpProjectFile(Root, "textures/new_texture.tres"), "ImageTexture", properties));

        Assert.NotNull(result);
        Assert.Equal("new_texture", result!.Name);

        await _client.Received(1).InvokeToolAsync(
            "create_resource",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "textures/new_texture.tres") &&
                Equals(d["type"], "ImageTexture") &&
                Equals(((Dictionary<string, string>)d["properties"]!)["size"], "512")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceUpdateAsync_FallsBackToLegacyCommand_WhenPrimaryIsMissing()
    {
        var matTres = Combine("materials", "mat.tres");
        var properties = new Dictionary<string, object?> { ["metallic"] = 0.8 };

        _client
            .InvokeToolAsync("resource.update_properties", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns<Task<McpResponse>>(_ => throw new McpServerException("Tool not found", -32601));

        _client
            .InvokeToolAsync("resource.update", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-5",
                true,
                new
                {
                    path = matTres,
                    type = "StandardMaterial3D",
                    properties = new Dictionary<string, object?> { ["metallic"] = 0.8 }
                }));

        var result = await _client.ResourceUpdateAsync(
            new ResourceUpdateRequest(new McpProjectFile(Root, "materials/mat.tres"), properties));

        Assert.NotNull(result);
        Assert.Equal(matTres, result!.Path);

        await _client.Received(1).InvokeToolAsync("resource.update", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResourceCreateAsync_FallsBackToLegacyCommand_WhenPrimaryIsMissing()
    {
        var properties = new Dictionary<string, object?> { ["size"] = 512 };

        _client
            .InvokeToolAsync("create_resource", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns<Task<McpResponse>>(_ => throw new McpServerException("Unknown tool", -32601));

        _client
            .InvokeToolAsync("resource.create", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-6",
                true,
                new
                {
                    path = Combine("textures", "new_texture.tres"),
                    type = "ImageTexture",
                    name = "new_texture",
                    exists = true
                }));

        var result = await _client.ResourceCreateAsync(
            new ResourceCreateRequest(new McpProjectFile(Root, "textures/new_texture.tres"), "ImageTexture", properties));

        Assert.NotNull(result);
        Assert.Equal("new_texture", result!.Name);

        await _client.Received(1).InvokeToolAsync("resource.create", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
    }
}
