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

        var result = await _client.PhysicsListBodiesAsync(new PhysicsListBodiesRequest(Root));

        Assert.Single(result);
        Assert.Equal("CharacterBody3D", result[0].BodyType);

        await _client.Received(1).InvokeToolAsync(
            "physics.list_bodies",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsCreateBodyAsync_MapsPayloadAndReturnsTypedBody()
    {
        _client.InvokeToolAsync("physics.create_body", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("body-1", true, new { name = "Player", path = "./Player", bodyType = "CharacterBody3D", enabled = true }));

        var result = await _client.PhysicsCreateBodyAsync(
            new PhysicsCreateBodyRequest(new McpProjectFile(Root, "scenes/main.tscn"), ".", "CharacterBody3D", "Player", true));

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
                new McpProjectFile(Root, "scenes/main.tscn"),
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

        var result = await _client.PhysicsValidateAsync(new PhysicsValidateRequest(Root));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Physics valid", result.Message);

        await _client.Received(1).InvokeToolAsync(
            "physics.validate",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsSetLayersAsync_MapsPayloadAndReturnsLayerResult()
    {
        _client.InvokeToolAsync("physics.set_layers", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("3", true, new { success = true, message = "Layers updated", collisionLayer = 2, collisionMask = 5 }));

        var result = await _client.PhysicsSetLayersAsync(
            new PhysicsSetLayersRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Player", 2, 5));

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
            new PhysicsRunChecksRequest(new McpProjectFile(Root, "scenes/main.tscn")));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Issues);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public async Task PhysicsAreaSetMonitoringAsync_MapsPayload_OmitsRootTypeWhenNull()
    {
        _client.InvokeToolAsync("physics.area_set_monitoring", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("area-1", true, new { success = true, message = "ok" }));

        var result = await _client.PhysicsAreaSetMonitoringAsync(
            new PhysicsAreaSetMonitoringRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Area3D", true, false));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "physics.area_set_monitoring",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/main.tscn") &&
                Equals(d["areaNodePath"], "./Area3D") &&
                Equals(d["monitoring"], true) &&
                Equals(d["monitorable"], false) &&
                !d.ContainsKey("root_type")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsAreaSetMonitoringAsync_IncludesRootTypeWhenProvided()
    {
        _client.InvokeToolAsync("physics.area_set_monitoring", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("area-2", true, new { success = true, message = "ok" }));

        _ = await _client.PhysicsAreaSetMonitoringAsync(
            new PhysicsAreaSetMonitoringRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Area2D", false, true, "Node2D"));

        await _client.Received(1).InvokeToolAsync(
            "physics.area_set_monitoring",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["root_type"], "Node2D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsAreaSetPriorityAsync_MapsPayload()
    {
        _client.InvokeToolAsync("physics.area_set_priority", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("area-3", true, new { success = true, message = "ok" }));

        _ = await _client.PhysicsAreaSetPriorityAsync(
            new PhysicsAreaSetPriorityRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Area3D", 10.5));

        await _client.Received(1).InvokeToolAsync(
            "physics.area_set_priority",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["priority"], 10.5) &&
                Equals(d["areaNodePath"], "./Area3D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsAreaSetSpaceOverrideAsync_MapsPayload_OmitsOptionalDoublesWhenNull()
    {
        _client.InvokeToolAsync("physics.area_set_space_override", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("area-4", true, new { success = true, message = "ok" }));

        _ = await _client.PhysicsAreaSetSpaceOverrideAsync(
            new PhysicsAreaSetSpaceOverrideRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Area3D", "combine"));

        await _client.Received(1).InvokeToolAsync(
            "physics.area_set_space_override",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["space_override_mode"], "combine") &&
                !d.ContainsKey("gravity") &&
                !d.ContainsKey("gravity_point_unit_distance") &&
                !d.ContainsKey("linear_damp") &&
                !d.ContainsKey("angular_damp")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsAreaSetSpaceOverrideAsync_IncludesOptionalDoublesWhenSet()
    {
        _client.InvokeToolAsync("physics.area_set_space_override", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("area-5", true, new { success = true, message = "ok" }));

        _ = await _client.PhysicsAreaSetSpaceOverrideAsync(
            new PhysicsAreaSetSpaceOverrideRequest(
                new McpProjectFile(Root, "scenes/main.tscn"),
                "./Area3D",
                "replace",
                Gravity: 1.25,
                GravityPointUnitDistance: 2.0,
                LinearDamp: 0.1,
                AngularDamp: 0.2));

        await _client.Received(1).InvokeToolAsync(
            "physics.area_set_space_override",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["gravity"], 1.25) &&
                Equals(d["gravity_point_unit_distance"], 2.0) &&
                Equals(d["linear_damp"], 0.1) &&
                Equals(d["angular_damp"], 0.2)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PhysicsAreaSetCollisionFiltersAsync_MapsPayload()
    {
        _client.InvokeToolAsync("physics.area_set_collision_filters", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("area-6", true, new { success = true, message = "ok" }));

        _ = await _client.PhysicsAreaSetCollisionFiltersAsync(
            new PhysicsAreaSetCollisionFiltersRequest(new McpProjectFile(Root, "scenes/main.tscn"), "./Area3D", 4, 8));

        await _client.Received(1).InvokeToolAsync(
            "physics.area_set_collision_filters",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["collision_layer"], 4) &&
                Equals(d["collision_mask"], 8)),
            Arg.Any<CancellationToken>());
    }
}
