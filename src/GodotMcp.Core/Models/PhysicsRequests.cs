namespace GodotMcp.Core.Models;

/// <summary>
/// Physics command request for listing bodies.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record PhysicsListBodiesRequest(string ScenePath);

/// <summary>
/// Physics command request for listing shapes.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record PhysicsListShapesRequest(string ScenePath);

/// <summary>
/// Physics command request for creating a shape.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="BodyPath">Target body node path.</param>
/// <param name="ShapeName">Shape node name.</param>
/// <param name="ShapeType">Godot shape type name.</param>
/// <param name="Properties">Initial shape properties.</param>
public sealed record PhysicsCreateShapeRequest(
    string ScenePath,
    string BodyPath,
    string ShapeName,
    string ShapeType,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Physics command request for updating a shape.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ShapePath">Shape node path.</param>
/// <param name="Properties">Shape properties to update.</param>
public sealed record PhysicsUpdateShapeRequest(
    string ScenePath,
    string ShapePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Physics command request for validating physics setup.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record PhysicsValidateRequest(string ScenePath);

/// <summary>
/// Represents a physics validation result.
/// </summary>
/// <param name="Success">Whether validation succeeded.</param>
/// <param name="Message">Optional validation message.</param>
public sealed record PhysicsValidationResult(
    bool Success,
    string? Message = null);

/// <summary>
/// Physics command request for setting collision layers and mask on a body.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="BodyPath">Body node path.</param>
/// <param name="CollisionLayer">Collision layer bitmask.</param>
/// <param name="CollisionMask">Collision mask bitmask.</param>
public sealed record PhysicsSetLayersRequest(
    string ScenePath,
    string BodyPath,
    int CollisionLayer,
    int CollisionMask);

/// <summary>
/// Physics command request for running physics checks in a scene.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="BodyPath">Optional body node path filter.</param>
public sealed record PhysicsRunChecksRequest(
    string ScenePath,
    string? BodyPath = null);

/// <summary>
/// Represents the result of a physics layers update operation.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional result message.</param>
/// <param name="CollisionLayer">Applied collision layer bitmask.</param>
/// <param name="CollisionMask">Applied collision mask bitmask.</param>
public sealed record PhysicsLayerResult(
    bool Success,
    string? Message = null,
    int? CollisionLayer = null,
    int? CollisionMask = null);

/// <summary>
/// Represents the result of running physics checks.
/// </summary>
/// <param name="Success">Whether checks succeeded.</param>
/// <param name="Message">Optional result message.</param>
/// <param name="Issues">Detected issue messages.</param>
public sealed record PhysicsCheckResult(
    bool Success,
    string? Message = null,
    IReadOnlyList<string>? Issues = null);
