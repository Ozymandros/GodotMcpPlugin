namespace GodotMcp.Core.Models;

/// <summary>
/// Physics command request for listing bodies (server: <c>projectPath</c> only).
/// </summary>
public sealed record PhysicsListBodiesRequest(string ProjectPath);

/// <summary>
/// Physics command request for listing shapes (tool surface not present on all server versions).
/// </summary>
public sealed record PhysicsListShapesRequest(McpProjectFile Scene);

/// <summary>
/// Physics command request for creating a shape.
/// </summary>
public sealed record PhysicsCreateShapeRequest(
    McpProjectFile Scene,
    string BodyPath,
    string ShapeName,
    string ShapeType,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Physics command request for updating a shape.
/// </summary>
public sealed record PhysicsUpdateShapeRequest(
    McpProjectFile Scene,
    string ShapePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Physics command request for validating physics setup (server: <c>projectPath</c> only).
/// </summary>
public sealed record PhysicsValidateRequest(string ProjectPath);

/// <summary>
/// Physics command request for creating a body.
/// </summary>
public sealed record PhysicsCreateBodyRequest(
    McpProjectFile Scene,
    string ParentNodePath,
    string BodyType,
    string NodeName,
    bool AddCollisionShape = true);

/// <summary>
/// Physics command request for updating a body.
/// </summary>
public sealed record PhysicsUpdateBodyRequest(
    McpProjectFile Scene,
    string NodePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Represents a physics validation result.
/// </summary>
public sealed record PhysicsValidationResult(
    bool Success,
    string? Message = null);

/// <summary>
/// Physics command request for setting collision layers and mask on a body.
/// </summary>
public sealed record PhysicsSetLayersRequest(
    McpProjectFile Scene,
    string BodyPath,
    int CollisionLayer,
    int CollisionMask);

/// <summary>
/// Physics command request for running physics checks in a scene.
/// </summary>
public sealed record PhysicsRunChecksRequest(McpProjectFile Scene, string? BodyPath = null);

/// <summary>
/// Represents a physics layers update operation result.
/// </summary>
public sealed record PhysicsLayerResult(
    bool Success,
    string? Message = null,
    int? CollisionLayer = null,
    int? CollisionMask = null);

/// <summary>
/// Represents the result of running physics checks.
/// </summary>
public sealed record PhysicsCheckResult(
    bool Success,
    string? Message = null,
    IReadOnlyList<string>? Issues = null);

/// <summary>
/// Physics command request for removing a shape.
/// </summary>
public sealed record PhysicsRemoveShapeRequest(
    McpProjectFile Scene,
    string ShapePath);

/// <summary>
/// Physics command request for adding a collision polygon.
/// </summary>
public sealed record PhysicsAddCollisionPolygonRequest(
    McpProjectFile Scene,
    string BodyPath,
    string PolygonName,
    IReadOnlyList<Vector2> Points);

/// <summary>
/// Physics command request for updating a collision polygon.
/// </summary>
public sealed record PhysicsUpdateCollisionPolygonRequest(
    McpProjectFile Scene,
    string PolygonPath,
    IReadOnlyList<Vector2>? Points = null,
    IReadOnlyDictionary<string, object?>? Properties = null);

/// <summary>
/// Physics command request for removing a collision polygon.
/// </summary>
public sealed record PhysicsRemoveCollisionPolygonRequest(
    McpProjectFile Scene,
    string PolygonPath);

/// <summary>
/// Physics command request for assigning a shape resource.
/// </summary>
public sealed record PhysicsAssignShapeResourceRequest(
    McpProjectFile Scene,
    string ShapePath,
    string ResourcePath);

/// <summary>
/// Physics command request for setting shape flags.
/// </summary>
public sealed record PhysicsSetShapeFlagsRequest(
    McpProjectFile Scene,
    string ShapePath,
    bool? Disabled = null,
    bool? Trigger = null);

/// <summary>
/// Physics command request for Area2D/Area3D monitoring flags (GodotMCP.Server v1.11.0+).
/// </summary>
public sealed record PhysicsAreaSetMonitoringRequest(
    McpProjectFile Scene,
    string AreaNodePath,
    bool Monitoring,
    bool Monitorable = true,
    string? RootType = null);

/// <summary>
/// Physics command request for Area2D/Area3D priority (GodotMCP.Server v1.11.0+).
/// </summary>
public sealed record PhysicsAreaSetPriorityRequest(
    McpProjectFile Scene,
    string AreaNodePath,
    double Priority,
    string? RootType = null);

/// <summary>
/// Physics command request for Area2D/Area3D space override and optional gravity/damping (GodotMCP.Server v1.11.0+).
/// </summary>
public sealed record PhysicsAreaSetSpaceOverrideRequest(
    McpProjectFile Scene,
    string AreaNodePath,
    string SpaceOverrideMode,
    double? Gravity = null,
    double? GravityPointUnitDistance = null,
    double? LinearDamp = null,
    double? AngularDamp = null,
    string? RootType = null);

/// <summary>
/// Physics command request for Area2D/Area3D collision layer and mask (GodotMCP.Server v1.11.0+).
/// </summary>
public sealed record PhysicsAreaSetCollisionFiltersRequest(
    McpProjectFile Scene,
    string AreaNodePath,
    int CollisionLayer,
    int CollisionMask,
    string? RootType = null);
