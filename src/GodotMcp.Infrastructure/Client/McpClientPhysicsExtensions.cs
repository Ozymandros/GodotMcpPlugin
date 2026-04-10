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
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics list-bodies request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of physics bodies. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<PhysicsBodyInfo>> PhysicsListBodiesAsync(
        this IMcpClient client,
        PhysicsListBodiesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<PhysicsBodyInfo>>(
            "physics.list_bodies",
            BuildProjectRootPayload(request.ProjectRootPath),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<PhysicsBodyInfo>();
    }

    /// <summary>
    /// Creates a physics body.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics create-body request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created body information, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsBodyInfo?> PhysicsCreateBodyAsync(
        this IMcpClient client,
        PhysicsCreateBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsBodyInfo>(
            "physics.create_body",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["parentNodePath"] = request.ParentPath,
                ["bodyType"] = request.BodyType,
                ["nodeName"] = request.NodeName,
                ["addCollisionShape"] = request.AddCollisionShape
            },
            cancellationToken);
    }

    /// <summary>
    /// Updates a physics body.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics update-body request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated body information, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsBodyInfo?> PhysicsUpdateBodyAsync(
        this IMcpClient client,
        PhysicsUpdateBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsBodyInfo>(
            "physics.update_body",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.BodyPath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Lists physics shapes in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics list-shapes request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of physics shapes. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<PhysicsShapeInfo>> PhysicsListShapesAsync(
        this IMcpClient client,
        PhysicsListShapesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<PhysicsShapeInfo>>(
            "physics.list_shapes",
            new Dictionary<string, object?> { ["scenePath"] = request.ScenePath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<PhysicsShapeInfo>();
    }

    /// <summary>
    /// Creates a physics shape.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics create-shape request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created shape information, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsShapeInfo?> PhysicsCreateShapeAsync(
        this IMcpClient client,
        PhysicsCreateShapeRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsShapeInfo>(
            "physics.create_shape",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["bodyPath"] = request.BodyPath,
                ["shapeName"] = request.ShapeName,
                ["shapeType"] = request.ShapeType,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Updates a physics shape.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics update-shape request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated shape information, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsShapeInfo?> PhysicsUpdateShapeAsync(
        this IMcpClient client,
        PhysicsUpdateShapeRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsShapeInfo>(
            "physics.update_shape",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["shapePath"] = request.ShapePath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Validates physics setup under a project root path.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics validate request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The physics validation result, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsValidationResult?> PhysicsValidateAsync(
        this IMcpClient client,
        PhysicsValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsValidationResult>(
            "physics.validate",
            BuildProjectRootPayload(request.ProjectRootPath),
            cancellationToken);
    }

    private static Dictionary<string, object?> BuildProjectRootPayload(string projectRootPath)
    {
        return new Dictionary<string, object?>
        {
            ["projectRootPath"] = projectRootPath,
            ["scenePath"] = projectRootPath
        };
    }

    /// <summary>
    /// Sets collision layer and mask for a physics body.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics set-layers request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The layer update result, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsLayerResult?> PhysicsSetLayersAsync(
        this IMcpClient client,
        PhysicsSetLayersRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsLayerResult>(
            "physics.set_layers",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["bodyPath"] = request.BodyPath,
                ["collisionLayer"] = request.CollisionLayer,
                ["collisionMask"] = request.CollisionMask
            },
            cancellationToken);
    }

    /// <summary>
    /// Runs physics checks for a scene or a specific body.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The physics checks request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The checks result, or <c>null</c> when no payload is returned.</returns>
    public static Task<PhysicsCheckResult?> PhysicsRunChecksAsync(
        this IMcpClient client,
        PhysicsRunChecksRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PhysicsCheckResult>(
            "physics.run_checks",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["bodyPath"] = request.BodyPath
            },
            cancellationToken);
    }
}
