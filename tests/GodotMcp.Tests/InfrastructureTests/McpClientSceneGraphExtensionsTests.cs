using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Scene Graph extension wrappers and typed send behavior.
/// </summary>
public class McpClientSceneGraphExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task SendAsync_WithNullResult_ReturnsDefault()
    {
        _client
            .InvokeToolAsync("scene.remove_node", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-1", true, null));

        var result = await _client.SendAsync<SceneCommandResult>(
            "scene.remove_node",
            new Dictionary<string, object?>());

        Assert.Null(result);
    }

    [Fact]
    public async Task SceneRenameNodeAsync_MapsPayloadAndReturnsTypedNode()
    {
        _client
            .InvokeToolAsync("scene.rename_node", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new { name = "CameraPivot", path = "./CameraPivot", type = "Node3D", parentPath = ".", isInternal = false }));

        var result = await _client.SceneRenameNodeAsync(
            new SceneRenameNodeRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Camera", "CameraPivot"));

        Assert.NotNull(result);
        Assert.Equal("CameraPivot", result!.Name);

        await _client.Received(1).InvokeToolAsync(
            "scene.rename_node",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/main.tscn") &&
                Equals(d["nodePath"], "./Camera") &&
                Equals(d["newName"], "CameraPivot")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SceneSetNodePropertiesAsync_MapsPayloadAndReturnsTypedList()
    {
        var properties = new[]
        {
            new NodePropertyInfo("fov", "float", System.Text.Json.JsonSerializer.SerializeToElement(75f), false)
        };

        _client
            .InvokeToolAsync("scene.set_node_properties", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-3",
                true,
                new[] { new { name = "fov", type = "float", value = 75, readOnly = false } }));

        var result = await _client.SceneSetNodePropertiesAsync(
            new SceneSetNodePropertiesRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Camera", properties));

        Assert.Single(result);
        Assert.Equal("fov", result[0].Name);

        await _client.Received(1).InvokeToolAsync(
            "scene.set_node_properties",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/main.tscn") &&
                Equals(d["nodePath"], "./Camera") &&
                ReferenceEquals(d["properties"], properties)),
            Arg.Any<CancellationToken>());
    }
}
