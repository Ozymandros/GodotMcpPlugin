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
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return await client.SendAsync<IReadOnlyList<PhysicsBodyInfo>>(
            "physics.list_bodies",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
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
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return client.SendAsync<PhysicsValidationResult>(
            "physics.validate",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
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

    /// <summary>
    /// Removes a physics shape.
    /// </summary>
    public static Task<ProjectOperationResult?> PhysicsRemoveShapeAsync(
        this IMcpClient client,
        PhysicsRemoveShapeRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["shapePath"] = request.ShapePath;
        return client.SendAsync<ProjectOperationResult>("physics.remove_shape", d, cancellationToken);
    }

    /// <summary>
    /// Adds a collision polygon to a physics body.
    /// </summary>
    public static Task<ProjectOperationResult?> PhysicsAddCollisionPolygonAsync(
        this IMcpClient client,
        PhysicsAddCollisionPolygonRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["bodyPath"] = request.BodyPath;
        d["polygonName"] = request.PolygonName;
        d["points"] = request.Points.Select(p => new { x = p.X, y = p.Y }).ToArray();
        return client.SendAsync<ProjectOperationResult>("physics.add_collision_polygon", d, cancellationToken);
    }

    /// <summary>
    /// Updates a collision polygon.
    /// </summary>
    public static Task<ProjectOperationResult?> PhysicsUpdateCollisionPolygonAsync(
        this IMcpClient client,
        PhysicsUpdateCollisionPolygonRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["polygonPath"] = request.PolygonPath;
        if (request.Points != null)
        {
            d["points"] = request.Points.Select(p => new { x = p.X, y = p.Y }).ToArray();
        }
        if (request.Properties != null)
        {
            d["properties"] = request.Properties;
        }
        return client.SendAsync<ProjectOperationResult>("physics.update_collision_polygon", d, cancellationToken);
    }

    /// <summary>
    /// Removes a collision polygon.
    /// </summary>
    public static Task<ProjectOperationResult?> PhysicsRemoveCollisionPolygonAsync(
        this IMcpClient client,
        PhysicsRemoveCollisionPolygonRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["polygonPath"] = request.PolygonPath;
        return client.SendAsync<ProjectOperationResult>("physics.remove_collision_polygon", d, cancellationToken);
    }

    /// <summary>
    /// Assigns a shape resource to a physics shape.
    /// </summary>
    public static Task<ProjectOperationResult?> PhysicsAssignShapeResourceAsync(
        this IMcpClient client,
        PhysicsAssignShapeResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["shapePath"] = request.ShapePath;
        d["resourcePath"] = request.ResourcePath;
        return client.SendAsync<ProjectOperationResult>("physics.assign_shape_resource", d, cancellationToken);
    }

    /// <summary>
    /// Sets shape flags on a physics shape.
    /// </summary>
    public static Task<ProjectOperationResult?> PhysicsSetShapeFlagsAsync(
        this IMcpClient client,
        PhysicsSetShapeFlagsRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["shapePath"] = request.ShapePath;
        if (request.Disabled.HasValue)
        {
            d["disabled"] = request.Disabled.Value;
        }
        if (request.Trigger.HasValue)
        {
            d["trigger"] = request.Trigger.Value;
        }
        return client.SendAsync<ProjectOperationResult>("physics.set_shape_flags", d, cancellationToken);
    }
}
