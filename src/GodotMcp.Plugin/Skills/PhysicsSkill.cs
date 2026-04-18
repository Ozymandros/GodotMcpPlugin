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

    [KernelFunction("list_bodies")]
    [Description("Lists physics bodies under a project root path.")]
    public Task<IReadOnlyList<PhysicsBodyInfo>> ListBodiesAsync(
        [Description("Absolute filesystem path to the Godot project root to scan (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsListBodiesAsync(new PhysicsListBodiesRequest(projectPath), cancellationToken);

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

    [KernelFunction("list_shapes")]
    [Description("Lists physics shapes in a scene.")]
    public Task<IReadOnlyList<PhysicsShapeInfo>> ListShapesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsListShapesAsync(new PhysicsListShapesRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

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

    [KernelFunction("validate")]
    [Description("Validates physics setup under a project root path.")]
    public Task<PhysicsValidationResult?> ValidateAsync(
        [Description("Absolute filesystem path to the Godot project root to validate (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsValidateAsync(new PhysicsValidateRequest(projectPath), cancellationToken);

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

    [KernelFunction("run_checks")]
    [Description("Runs physics checks in a scene.")]
    public Task<PhysicsCheckResult?> RunChecksAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Optional body node path filter.")] string? bodyPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.PhysicsRunChecksAsync(new PhysicsRunChecksRequest(new McpProjectFile(projectPath, fileName), bodyPath), cancellationToken);
}
