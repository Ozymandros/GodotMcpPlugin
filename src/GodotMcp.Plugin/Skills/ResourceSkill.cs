using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Resource MCP commands.
/// </summary>
public sealed class ResourceSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists resources, optionally filtering by directory or type.
    /// </summary>
    [KernelFunction("list")]
    [Description("Lists resources.")]
    public Task<IReadOnlyList<ResourceInfo>> ListAsync(
        [Description("Optional resource directory filter.")] string? directory = null,
        [Description("Optional resource type filter.")] string? resourceType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceListAsync(new ResourceListRequest(directory, resourceType), cancellationToken);

    /// <summary>
    /// Reads the specified resource file and returns its data.
    /// </summary>
    [KernelFunction("read")]
    [Description("Reads a resource.")]
    public Task<ResourceData?> ReadAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Resource file path relative to project root (e.g. materials/mat.tres).")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceReadAsync(new ResourceReadRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Updates properties of the specified resource.
    /// </summary>
    [KernelFunction("update")]
    [Description("Updates a resource.")]
    public Task<ResourceData?> UpdateAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Resource file path relative to project root.")] string fileName,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceUpdateAsync(new ResourceUpdateRequest(new McpProjectFile(projectPath, fileName), properties), cancellationToken);

    /// <summary>
    /// Creates a new resource with the provided type and properties.
    /// </summary>
    [KernelFunction("create")]
    [Description("Creates a resource.")]
    public Task<ResourceInfo?> CreateAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Resource file path relative to project root.")] string fileName,
        [Description("Resource type.")] string resourceType,
        [Description("Initial properties.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceCreateAsync(
            new ResourceCreateRequest(new McpProjectFile(projectPath, fileName), resourceType, properties),
            cancellationToken);

    /// <summary>
    /// Assigns a texture to a resource property.
    /// </summary>
    [KernelFunction("assign_texture")]
    [Description("Assigns a texture to a resource property.")]
    public Task<ProjectOperationResult?> AssignTextureAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Resource file path relative to project root.")] string fileName,
        [Description("Property path on the resource (e.g. 'albedo_texture').")] string propertyPath,
        [Description("Path to the texture resource.")] string texturePath,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceAssignTextureAsync(
            new ResourceAssignTextureRequest(new McpProjectFile(projectPath, fileName), propertyPath, texturePath),
            cancellationToken);
}
