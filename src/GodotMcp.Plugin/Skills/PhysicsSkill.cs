using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Physics MCP commands.
/// </summary>
public sealed class PhysicsSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists physics bodies under the specified project root path.
    /// </summary>
    [KernelFunction("list_bodies")]
    [Description("Lists physics bodies under a project root path.")]
    public Task<IReadOnlyList<PhysicsBodyInfo>> ListBodiesAsync(
        [Description("Absolute filesystem path to the Godot project root to scan (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsListBodiesAsync(new PhysicsListBodiesRequest(projectPath), cancellationToken);

    /// <summary>
    /// Creates a physics body in the specified scene.
    /// </summary>
    [KernelFunction("create_body")]
    [Description("Creates a physics body in a scene.")]
    public Task<PhysicsBodyInfo?> CreateBodyAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Parent node path.")] string parentNodePath,
        [Description("Body type.")] string bodyType,
        [Description("Body node name.")] string nodeName,
        [Description("Whether to auto-add collision shape.")] bool addCollisionShape = true,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsCreateBodyAsync(
            new PhysicsCreateBodyRequest(new McpProjectFile(projectPath, fileName), parentNodePath, bodyType, nodeName, addCollisionShape),
            cancellationToken);

    /// <summary>
    /// Updates properties of a physics body in a scene.
    /// </summary>
    [KernelFunction("update_body")]
    [Description("Updates a physics body.")]
    public Task<PhysicsBodyInfo?> UpdateBodyAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Body node path.")] string nodePath,
        [Description("Body properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsUpdateBodyAsync(
            new PhysicsUpdateBodyRequest(new McpProjectFile(projectPath, fileName), nodePath, properties),
            cancellationToken);

    /// <summary>
    /// Lists physics shapes present in the specified scene.
    /// </summary>
    [KernelFunction("list_shapes")]
    [Description("Lists physics shapes in a scene.")]
    public Task<IReadOnlyList<PhysicsShapeInfo>> ListShapesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsListShapesAsync(new PhysicsListShapesRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Creates a physics shape under the given body node in a scene.
    /// </summary>
    [KernelFunction("create_shape")]
    [Description("Creates a physics shape.")]
    public Task<PhysicsShapeInfo?> CreateShapeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Body node path.")] string bodyPath,
        [Description("Shape name.")] string shapeName,
        [Description("Shape type.")] string shapeType,
        [Description("Initial shape properties.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsCreateShapeAsync(
            new PhysicsCreateShapeRequest(new McpProjectFile(projectPath, fileName), bodyPath, shapeName, shapeType, properties),
            cancellationToken);

    /// <summary>
    /// Updates properties of an existing physics shape in a scene.
    /// </summary>
    [KernelFunction("update_shape")]
    [Description("Updates a physics shape.")]
    public Task<PhysicsShapeInfo?> UpdateShapeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Shape node path.")] string shapePath,
        [Description("Shape properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsUpdateShapeAsync(
            new PhysicsUpdateShapeRequest(new McpProjectFile(projectPath, fileName), shapePath, properties),
            cancellationToken);

    /// <summary>
    /// Validates the physics setup under the specified project root path.
    /// </summary>
    [KernelFunction("validate")]
    [Description("Validates physics setup under a project root path.")]
    public Task<PhysicsValidationResult?> ValidateAsync(
        [Description("Absolute filesystem path to the Godot project root to validate (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsValidateAsync(new PhysicsValidateRequest(projectPath), cancellationToken);

    /// <summary>
    /// Sets collision layer and mask values for a physics body.
    /// </summary>
    [KernelFunction("set_layers")]
    [Description("Sets collision layer and mask for a physics body.")]
    public Task<PhysicsLayerResult?> SetLayersAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Body node path.")] string bodyPath,
        [Description("Collision layer bitmask.")] int collisionLayer,
        [Description("Collision mask bitmask.")] int collisionMask,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsSetLayersAsync(
            new PhysicsSetLayersRequest(new McpProjectFile(projectPath, fileName), bodyPath, collisionLayer, collisionMask),
            cancellationToken);

    /// <summary>
    /// Runs physics checks in the specified scene and returns results.
    /// </summary>
    [KernelFunction("run_checks")]
    [Description("Runs physics checks in a scene.")]
    public Task<PhysicsCheckResult?> RunChecksAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Optional body node path filter.")] string? bodyPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsRunChecksAsync(new PhysicsRunChecksRequest(new McpProjectFile(projectPath, fileName), bodyPath), cancellationToken);

    /// <summary>
    /// Removes a physics shape from a scene.
    /// </summary>
    [KernelFunction("remove_shape")]
    [Description("Removes a physics shape.")]
    public Task<ProjectOperationResult?> RemoveShapeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Shape node path.")] string shapePath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsRemoveShapeAsync(new PhysicsRemoveShapeRequest(new McpProjectFile(projectPath, fileName), shapePath), cancellationToken);

    /// <summary>
    /// Adds a collision polygon to a physics body.
    /// </summary>
    [KernelFunction("add_collision_polygon")]
    [Description("Adds a collision polygon to a physics body.")]
    public Task<ProjectOperationResult?> AddCollisionPolygonAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Body node path.")] string bodyPath,
        [Description("Polygon node name.")] string polygonName,
        [Description("Polygon points as flat array [x1,y1,x2,y2,...] or array of Vector2.")] IReadOnlyList<Vector2> points,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsAddCollisionPolygonAsync(
            new PhysicsAddCollisionPolygonRequest(new McpProjectFile(projectPath, fileName), bodyPath, polygonName, points),
            cancellationToken);

    /// <summary>
    /// Updates a collision polygon.
    /// </summary>
    [KernelFunction("update_collision_polygon")]
    [Description("Updates a collision polygon.")]
    public Task<ProjectOperationResult?> UpdateCollisionPolygonAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Polygon node path.")] string polygonPath,
        [Description("Optional new points.")] IReadOnlyList<Vector2>? points = null,
        [Description("Optional polygon properties.")] IReadOnlyDictionary<string, object?>? properties = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsUpdateCollisionPolygonAsync(
            new PhysicsUpdateCollisionPolygonRequest(new McpProjectFile(projectPath, fileName), polygonPath, points, properties),
            cancellationToken);

    /// <summary>
    /// Removes a collision polygon.
    /// </summary>
    [KernelFunction("remove_collision_polygon")]
    [Description("Removes a collision polygon.")]
    public Task<ProjectOperationResult?> RemoveCollisionPolygonAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Polygon node path.")] string polygonPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsRemoveCollisionPolygonAsync(
            new PhysicsRemoveCollisionPolygonRequest(new McpProjectFile(projectPath, fileName), polygonPath),
            cancellationToken);

    /// <summary>
    /// Assigns a shape resource to a physics shape.
    /// </summary>
    [KernelFunction("assign_shape_resource")]
    [Description("Assigns a shape resource to a physics shape.")]
    public Task<ProjectOperationResult?> AssignShapeResourceAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Shape node path.")] string shapePath,
        [Description("Path to the shape resource (.tres).")] string resourcePath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsAssignShapeResourceAsync(
            new PhysicsAssignShapeResourceRequest(new McpProjectFile(projectPath, fileName), shapePath, resourcePath),
            cancellationToken);

    /// <summary>
    /// Sets shape flags on a physics shape.
    /// </summary>
    [KernelFunction("set_shape_flags")]
    [Description("Sets shape flags on a physics shape.")]
    public Task<ProjectOperationResult?> SetShapeFlagsAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Shape node path.")] string shapePath,
        [Description("Whether the shape is disabled.")] bool? disabled = null,
        [Description("Whether the shape is a trigger.")] bool? trigger = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsSetShapeFlagsAsync(
            new PhysicsSetShapeFlagsRequest(new McpProjectFile(projectPath, fileName), shapePath, disabled, trigger),
            cancellationToken);

    /// <summary>
    /// Sets monitoring and monitorable on an Area2D/Area3D node.
    /// </summary>
    [KernelFunction("area_set_monitoring")]
    [Description("Sets monitoring and monitorable on an Area2D/Area3D node.")]
    public Task<ProjectOperationResult?> AreaSetMonitoringAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Area2D/Area3D node path.")] string areaNodePath,
        [Description("Whether the area detects overlaps/signals.")] bool monitoring,
        [Description("Whether this area can be detected by other areas/bodies.")] bool monitorable = true,
        [Description("Optional root node type when the scene is bootstrapped (server default Node3D).")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsAreaSetMonitoringAsync(
            new PhysicsAreaSetMonitoringRequest(new McpProjectFile(projectPath, fileName), areaNodePath, monitoring, monitorable, rootType),
            cancellationToken);

    /// <summary>
    /// Sets priority on an Area2D/Area3D node.
    /// </summary>
    [KernelFunction("area_set_priority")]
    [Description("Sets priority on an Area2D/Area3D node.")]
    public Task<ProjectOperationResult?> AreaSetPriorityAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Area2D/Area3D node path.")] string areaNodePath,
        [Description("Priority value.")] double priority,
        [Description("Optional root node type when the scene is bootstrapped (server default Node3D).")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsAreaSetPriorityAsync(
            new PhysicsAreaSetPriorityRequest(new McpProjectFile(projectPath, fileName), areaNodePath, priority, rootType),
            cancellationToken);

    /// <summary>
    /// Sets space override mode and optional gravity/damping on an Area2D/Area3D node.
    /// </summary>
    [KernelFunction("area_set_space_override")]
    [Description("Sets space override mode (disabled, combine, combine_replace, replace, replace_combine or 0-4) and optional gravity/damping.")]
    public Task<ProjectOperationResult?> AreaSetSpaceOverrideAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Area2D/Area3D node path.")] string areaNodePath,
        [Description("Override mode: disabled, combine, combine_replace, replace, replace_combine (or 0-4).")] string spaceOverrideMode,
        [Description("Optional gravity override.")] double? gravity = null,
        [Description("Optional gravity point unit distance.")] double? gravityPointUnitDistance = null,
        [Description("Optional linear damp override.")] double? linearDamp = null,
        [Description("Optional angular damp override.")] double? angularDamp = null,
        [Description("Optional root node type when the scene is bootstrapped (server default Node3D).")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsAreaSetSpaceOverrideAsync(
            new PhysicsAreaSetSpaceOverrideRequest(
                new McpProjectFile(projectPath, fileName),
                areaNodePath,
                spaceOverrideMode,
                gravity,
                gravityPointUnitDistance,
                linearDamp,
                angularDamp,
                rootType),
            cancellationToken);

    /// <summary>
    /// Sets collision layer and mask on an Area2D/Area3D node.
    /// </summary>
    [KernelFunction("area_set_collision_filters")]
    [Description("Sets collision layer and mask on an Area2D/Area3D node (both must be greater than zero on the server).")]
    public Task<ProjectOperationResult?> AreaSetCollisionFiltersAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Area2D/Area3D node path.")] string areaNodePath,
        [Description("Collision layer bitmask (must be greater than zero).")] int collisionLayer,
        [Description("Collision mask bitmask (must be greater than zero).")] int collisionMask,
        [Description("Optional root node type when the scene is bootstrapped (server default Node3D).")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsAreaSetCollisionFiltersAsync(
            new PhysicsAreaSetCollisionFiltersRequest(new McpProjectFile(projectPath, fileName), areaNodePath, collisionLayer, collisionMask, rootType),
            cancellationToken);
}
