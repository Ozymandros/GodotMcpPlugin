using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Physics MCP commands.
/// </summary>
public sealed class PhysicsSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists physics bodies under a project root path.
    /// </summary>
    /// <param name="projectRootPath">Project root path to scan.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of physics bodies.</returns>
    [KernelFunction("list_bodies")]
    [Description("Lists physics bodies in a scene.")]
    public Task<IReadOnlyList<PhysicsBodyInfo>> ListBodiesAsync(
        [Description("Project root path to scan.")] string projectRootPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsListBodiesAsync(new PhysicsListBodiesRequest(projectRootPath), cancellationToken);

    /// <summary>
    /// Creates a physics body in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="parentPath">Parent node path.</param>
    /// <param name="bodyType">Body type.</param>
    /// <param name="nodeName">Body node name.</param>
    /// <param name="addCollisionShape">Whether to auto-add a collision shape child.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created body, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create_body")]
    [Description("Creates a physics body in a scene.")]
    public Task<PhysicsBodyInfo?> CreateBodyAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Parent node path.")] string parentPath,
        [Description("Body type.")] string bodyType,
        [Description("Body node name.")] string nodeName,
        [Description("Whether to auto-add collision shape.")] bool addCollisionShape = true,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsCreateBodyAsync(
            new PhysicsCreateBodyRequest(scenePath, parentPath, bodyType, nodeName, addCollisionShape),
            cancellationToken);

    /// <summary>
    /// Updates a physics body.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="bodyPath">Body node path.</param>
    /// <param name="properties">Body properties to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated body, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("update_body")]
    [Description("Updates a physics body.")]
    public Task<PhysicsBodyInfo?> UpdateBodyAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Body node path.")] string bodyPath,
        [Description("Body properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsUpdateBodyAsync(new PhysicsUpdateBodyRequest(scenePath, bodyPath, properties), cancellationToken);

    /// <summary>
    /// Lists physics shapes in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of physics shapes.</returns>
    [KernelFunction("list_shapes")]
    [Description("Lists physics shapes in a scene.")]
    public Task<IReadOnlyList<PhysicsShapeInfo>> ListShapesAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsListShapesAsync(new PhysicsListShapesRequest(scenePath), cancellationToken);

    /// <summary>
    /// Creates a physics shape.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="bodyPath">Body node path.</param>
    /// <param name="shapeName">Shape name.</param>
    /// <param name="shapeType">Shape type.</param>
    /// <param name="properties">Initial shape properties.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created shape, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create_shape")]
    [Description("Creates a physics shape.")]
    public Task<PhysicsShapeInfo?> CreateShapeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Body node path.")] string bodyPath,
        [Description("Shape name.")] string shapeName,
        [Description("Shape type.")] string shapeType,
        [Description("Initial shape properties.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsCreateShapeAsync(new PhysicsCreateShapeRequest(scenePath, bodyPath, shapeName, shapeType, properties), cancellationToken);

    /// <summary>
    /// Updates a physics shape.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="shapePath">Shape node path.</param>
    /// <param name="properties">Shape properties to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated shape, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("update_shape")]
    [Description("Updates a physics shape.")]
    public Task<PhysicsShapeInfo?> UpdateShapeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Shape node path.")] string shapePath,
        [Description("Shape properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsUpdateShapeAsync(new PhysicsUpdateShapeRequest(scenePath, shapePath, properties), cancellationToken);

    /// <summary>
    /// Validates physics setup under a project root path.
    /// </summary>
    /// <param name="projectRootPath">Project root path to validate.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The physics validation result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("validate")]
    [Description("Validates physics setup in a scene.")]
    public Task<PhysicsValidationResult?> ValidateAsync(
        [Description("Project root path to validate.")] string projectRootPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsValidateAsync(new PhysicsValidateRequest(projectRootPath), cancellationToken);

    /// <summary>
    /// Sets collision layer and mask for a physics body.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="bodyPath">Body node path.</param>
    /// <param name="collisionLayer">Collision layer bitmask.</param>
    /// <param name="collisionMask">Collision mask bitmask.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The layer update result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("set_layers")]
    [Description("Sets collision layer and mask for a physics body.")]
    public Task<PhysicsLayerResult?> SetLayersAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Body node path.")] string bodyPath,
        [Description("Collision layer bitmask.")] int collisionLayer,
        [Description("Collision mask bitmask.")] int collisionMask,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsSetLayersAsync(new PhysicsSetLayersRequest(scenePath, bodyPath, collisionLayer, collisionMask), cancellationToken);

    /// <summary>
    /// Runs physics checks in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="bodyPath">Optional body node path filter.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The checks result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("run_checks")]
    [Description("Runs physics checks in a scene.")]
    public Task<PhysicsCheckResult?> RunChecksAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Optional body node path filter.")] string? bodyPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsRunChecksAsync(new PhysicsRunChecksRequest(scenePath, bodyPath), cancellationToken);
}
