using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Resource MCP commands.
/// </summary>
public sealed class ResourceSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists resources.
    /// </summary>
    /// <param name="directory">Optional resource directory filter.</param>
    /// <param name="resourceType">Optional resource type filter.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of resources.</returns>
    [KernelFunction("list")]
    [Description("Lists resources.")]
    public Task<IReadOnlyList<ResourceInfo>> ListAsync(
        [Description("Optional resource directory filter.")] string? directory = null,
        [Description("Optional resource type filter.")] string? resourceType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceListAsync(new ResourceListRequest(directory, resourceType), cancellationToken);

    /// <summary>
    /// Reads a resource.
    /// </summary>
    /// <param name="resourcePath">Resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The resource payload, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("read")]
    [Description("Reads a resource.")]
    public Task<ResourceData?> ReadAsync(
        [Description("Resource path.")] string resourcePath,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceReadAsync(new ResourceReadRequest(resourcePath), cancellationToken);

    /// <summary>
    /// Updates a resource.
    /// </summary>
    /// <param name="resourcePath">Resource path.</param>
    /// <param name="properties">Properties to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated resource payload, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("update")]
    [Description("Updates a resource.")]
    public Task<ResourceData?> UpdateAsync(
        [Description("Resource path.")] string resourcePath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceUpdateAsync(new ResourceUpdateRequest(resourcePath, properties), cancellationToken);

    /// <summary>
    /// Creates a resource.
    /// </summary>
    /// <param name="resourcePath">Resource path.</param>
    /// <param name="resourceType">Resource type.</param>
    /// <param name="properties">Initial properties.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created resource info, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create")]
    [Description("Creates a resource.")]
    public Task<ResourceInfo?> CreateAsync(
        [Description("Resource path.")] string resourcePath,
        [Description("Resource type.")] string resourceType,
        [Description("Initial properties.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.ResourceCreateAsync(new ResourceCreateRequest(resourcePath, resourceType, properties), cancellationToken);
}
