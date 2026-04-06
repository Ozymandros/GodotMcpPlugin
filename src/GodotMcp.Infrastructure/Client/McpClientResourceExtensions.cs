using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Resource wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientResourceExtensions
{
    /// <summary>
    /// Lists resources using optional filters.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The resource list request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of resources. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<ResourceInfo>> ResourceListAsync(
        this IMcpClient client,
        ResourceListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<ResourceInfo>>(
            "resource.list",
            new Dictionary<string, object?>
            {
                ["directory"] = request.Directory,
                ["resourceType"] = request.ResourceType
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<ResourceInfo>();
    }

    /// <summary>
    /// Reads a resource payload.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The resource read request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The resource payload, or <c>null</c> when no payload is returned.</returns>
    public static Task<ResourceData?> ResourceReadAsync(
        this IMcpClient client,
        ResourceReadRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ResourceData>(
            "resource.read",
            new Dictionary<string, object?>
            {
                ["resourcePath"] = request.ResourcePath
            },
            cancellationToken);
    }

    /// <summary>
    /// Updates a resource payload.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The resource update request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated resource payload, or <c>null</c> when no payload is returned.</returns>
    public static Task<ResourceData?> ResourceUpdateAsync(
        this IMcpClient client,
        ResourceUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ResourceData>(
            "resource.update",
            new Dictionary<string, object?>
            {
                ["resourcePath"] = request.ResourcePath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Creates a resource payload.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The resource create request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Information for the created resource, or <c>null</c> when no payload is returned.</returns>
    public static Task<ResourceInfo?> ResourceCreateAsync(
        this IMcpClient client,
        ResourceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ResourceInfo>(
            "resource.create",
            new Dictionary<string, object?>
            {
                ["resourcePath"] = request.ResourcePath,
                ["resourceType"] = request.ResourceType,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }
}
