using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Physics wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientPhysicsExtensions
{
    /// <summary>
    /// Lists physics bodies under a project root path.
    /// </summary>
    public static async Task<IReadOnlyList<PhysicsBodyInfo>> PhysicsListBodiesAsync(
        this IMcpClient client,
        PhysicsListBodiesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<PhysicsBodyInfo>>(
            "physics.list_bodies",
            new Dictionary<string, object?> { ["projectPath"] = request.ProjectPath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<PhysicsBodyInfo>();
    }

    /// <summary>
    /// Creates a physics body.
    /// </summary>
    public static Task<PhysicsBodyInfo?> PhysicsCreateBodyAsync(
        this IMcpClient client,
        PhysicsCreateBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["parentNodePath"] = request.ParentNodePath;
        d["bodyType"] = request.BodyType;
        d["nodeName"] = request.NodeName;
        d["addCollisionShape"] = request.AddCollisionShape;
        return client.SendAsync<PhysicsBodyInfo>("physics.create_body", d, cancellationToken);
    }

    /// <summary>
    /// Updates a physics body.
    /// </summary>
    public static Task<PhysicsBodyInfo?> PhysicsUpdateBodyAsync(
        this IMcpClient client,
        PhysicsUpdateBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["nodePath"] = request.NodePath;
        d["properties"] = request.Properties;
        return client.SendAsync<PhysicsBodyInfo>("physics.update_body", d, cancellationToken);
    }

    /// <summary>
    /// Lists physics shapes in a scene.
    /// </summary>
    public static async Task<IReadOnlyList<PhysicsShapeInfo>> PhysicsListShapesAsync(
        this IMcpClient client,
        PhysicsListShapesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<PhysicsShapeInfo>>(
            "physics.list_shapes",
            McpProjectFilePayload.ToDictionary(request.Scene),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<PhysicsShapeInfo>();
    }

    /// <summary>
    /// Creates a physics shape.
    /// </summary>
    public static Task<PhysicsShapeInfo?> PhysicsCreateShapeAsync(
        this IMcpClient client,
        PhysicsCreateShapeRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["bodyPath"] = request.BodyPath;
        d["shapeName"] = request.ShapeName;
        d["shapeType"] = request.ShapeType;
        d["properties"] = request.Properties;
        return client.SendAsync<PhysicsShapeInfo>("physics.create_shape", d, cancellationToken);
    }

    /// <summary>
    /// Updates a physics shape.
    /// </summary>
    public static Task<PhysicsShapeInfo?> PhysicsUpdateShapeAsync(
        this IMcpClient client,
        PhysicsUpdateShapeRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["shapePath"] = request.ShapePath;
        d["properties"] = request.Properties;
        return client.SendAsync<PhysicsShapeInfo>("physics.update_shape", d, cancellationToken);
    }

    /// <summary>
    /// Validates physics setup under a project root path.
    /// </summary>
    public static Task<PhysicsValidationResult?> PhysicsValidateAsync(
        this IMcpClient client,
        PhysicsValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsValidationResult>(
            "physics.validate",
            new Dictionary<string, object?> { ["projectPath"] = request.ProjectPath },
            cancellationToken);
    }

    /// <summary>
    /// Sets collision layer and mask for a physics body.
    /// </summary>
    public static Task<PhysicsLayerResult?> PhysicsSetLayersAsync(
        this IMcpClient client,
        PhysicsSetLayersRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["bodyPath"] = request.BodyPath;
        d["collisionLayer"] = request.CollisionLayer;
        d["collisionMask"] = request.CollisionMask;
        return client.SendAsync<PhysicsLayerResult>("physics.set_layers", d, cancellationToken);
    }

    /// <summary>
    /// Runs physics checks for a scene or a specific body.
    /// </summary>
    public static Task<PhysicsCheckResult?> PhysicsRunChecksAsync(
        this IMcpClient client,
        PhysicsRunChecksRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["bodyPath"] = request.BodyPath;
        return client.SendAsync<PhysicsCheckResult>("physics.run_checks", d, cancellationToken);
    }
}
