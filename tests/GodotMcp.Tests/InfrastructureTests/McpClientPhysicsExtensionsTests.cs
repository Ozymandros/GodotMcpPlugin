using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Physics extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientPhysicsExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task PhysicsListBodiesAsync_MapsPayloadAndReturnsTypedList()
    {
        _client.InvokeToolAsync("physics.list_bodies", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("1", true, new[] { new { name = "PlayerBody", path = "./Player", bodyType = "CharacterBody3D", enabled = true } }));

        var result = await _client.PhysicsListBodiesAsync(new PhysicsListBodiesRequest("res://"));

        Assert.Single(result);
        Assert.Equal("CharacterBody3D", result[0].BodyType);

        await _client.Received(1).InvokeToolAsync(
            "physics.list_bodies",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsCreateBodyAsync_MapsPayloadAndReturnsTypedBody()
    {
        _client.InvokeToolAsync("physics.create_body", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("body-1", true, new { name = "Player", path = "./Player", bodyType = "CharacterBody3D", enabled = true }));

        var result = await _client.PhysicsCreateBodyAsync(
            new PhysicsCreateBodyRequest(new McpProjectFile("res://", "scenes/main.tscn"), ".", "CharacterBody3D", "Player", true));

        Assert.NotNull(result);
        Assert.Equal("Player", result!.Name);
    }

    [Fact]
    public async Task PhysicsUpdateBodyAsync_MapsPayloadAndReturnsTypedBody()
    {
        _client.InvokeToolAsync("physics.update_body", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("body-2", true, new { name = "Player", path = "./Player", bodyType = "CharacterBody3D", enabled = true }));

        var result = await _client.PhysicsUpdateBodyAsync(
            new PhysicsUpdateBodyRequest(
                new McpProjectFile("res://", "scenes/main.tscn"),
                "./Player",
                new Dictionary<string, object?> { ["collision_layer"] = 2 }));

        Assert.NotNull(result);
        Assert.Equal("CharacterBody3D", result!.BodyType);
    }

    [Fact]
    public async Task PhysicsValidateAsync_ReturnsValidationResult()
    {
        _client.InvokeToolAsync("physics.validate", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("2", true, new { success = true, message = "Physics valid" }));

        var result = await _client.PhysicsValidateAsync(new PhysicsValidateRequest("res://"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Physics valid", result.Message);

        await _client.Received(1).InvokeToolAsync(
            "physics.validate",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsSetLayersAsync_MapsPayloadAndReturnsLayerResult()
    {
        _client.InvokeToolAsync("physics.set_layers", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("3", true, new { success = true, message = "Layers updated", collisionLayer = 2, collisionMask = 5 }));

        var result = await _client.PhysicsSetLayersAsync(
            new PhysicsSetLayersRequest(new McpProjectFile("res://", "scenes/main.tscn"), "./Player", 2, 5));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal(2, result.CollisionLayer);
        Assert.Equal(5, result.CollisionMask);
    }

    [Fact]
    public async Task PhysicsRunChecksAsync_ReturnsCheckResult()
    {
        _client.InvokeToolAsync("physics.run_checks", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("4", true, new { success = true, message = "No issues", issues = new string[0] }));

        var result = await _client.PhysicsRunChecksAsync(
            new PhysicsRunChecksRequest(new McpProjectFile("res://", "scenes/main.tscn")));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Issues);
        Assert.Empty(result.Issues);
    }
}
