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
