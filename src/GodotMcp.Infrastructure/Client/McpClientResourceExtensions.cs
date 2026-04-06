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
        var payload = new Dictionary<string, object?>
        {
            ["resourcePath"] = request.ResourcePath,
            ["path"] = request.ResourcePath,
            ["properties"] = request.Properties
        };

        return InvokeResourceWithFallbackAsync<ResourceData>(
            client,
            "resource.update_properties",
            "resource.update",
            payload,
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
        var payload = new Dictionary<string, object?>
        {
            ["resourcePath"] = request.ResourcePath,
            ["path"] = request.ResourcePath,
            ["resourceType"] = request.ResourceType,
            ["type"] = request.ResourceType,
            ["properties"] = request.Properties
        };

        return InvokeResourceWithFallbackAsync<ResourceInfo>(
            client,
            "create_resource",
            "resource.create",
            payload,
            cancellationToken);
    }

    private static async Task<T?> InvokeResourceWithFallbackAsync<T>(
        IMcpClient client,
        string primaryCommand,
        string fallbackCommand,
        IReadOnlyDictionary<string, object?> payload,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.SendAsync<T>(primaryCommand, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (McpServerException ex) when (IsToolNotFound(ex))
        {
            return await client.SendAsync<T>(fallbackCommand, payload, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsToolNotFound(McpServerException ex)
        => ex.ErrorCode == -32601
           || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
           || ex.Message.Contains("unknown tool", StringComparison.OrdinalIgnoreCase);
}
