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

    [KernelFunction("list")]
    [Description("Lists resources.")]
    public Task<IReadOnlyList<ResourceInfo>> ListAsync(
        [Description("Optional resource directory filter.")] string? directory = null,
        [Description("Optional resource type filter.")] string? resourceType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceListAsync(new ResourceListRequest(directory, resourceType), cancellationToken);

    [KernelFunction("read")]
    [Description("Reads a resource.")]
    public Task<ResourceData?> ReadAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Resource file path relative to project root (e.g. materials/mat.tres).")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceReadAsync(new ResourceReadRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    [KernelFunction("update")]
    [Description("Updates a resource.")]
    public Task<ResourceData?> UpdateAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Resource file path relative to project root.")] string fileName,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceUpdateAsync(new ResourceUpdateRequest(new McpProjectFile(projectPath, fileName), properties), cancellationToken);

    [KernelFunction("create")]
    [Description("Creates a resource.")]
    public Task<ResourceInfo?> CreateAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Resource file path relative to project root.")] string fileName,
        [Description("Resource type.")] string resourceType,
        [Description("Initial properties.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceCreateAsync(
            new ResourceCreateRequest(new McpProjectFile(projectPath, fileName), resourceType, properties),
            cancellationToken);
}
