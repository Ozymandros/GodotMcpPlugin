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
    public static async Task<IReadOnlyList<ResourceInfo>> ResourceListAsync(
        this IMcpClient client,
        ResourceListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<ResourceInfo>>(
            "list_resources",
            new Dictionary<string, object?>
            {
                ["projectPath"] = request.Directory, // Directory is used as projectPath for server compatibility
                ["directory"] = request.Directory,
                ["resourceType"] = request.ResourceType
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<ResourceInfo>();
    }

    /// <summary>
    /// Reads a resource payload.
    /// </summary>
    public static Task<ResourceData?> ResourceReadAsync(
        this IMcpClient client,
        ResourceReadRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ResourceData>(
            "resource.read",
            McpProjectFilePayload.ToDictionary(request.Resource),
            cancellationToken);
    }

    /// <summary>
    /// Updates a resource payload.
    /// </summary>
    public static Task<ResourceData?> ResourceUpdateAsync(
        this IMcpClient client,
        ResourceUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Resource);
        d["properties"] = ToStringPropertyMap(request.Properties);
        return InvokeResourceWithFallbackAsync<ResourceData>(
            client,
            "resource.update_properties",
            "resource.update",
            d,
            cancellationToken);
    }

    /// <summary>
    /// Creates a resource payload.
    /// </summary>
    public static Task<ResourceInfo?> ResourceCreateAsync(
        this IMcpClient client,
        ResourceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Resource);
        d["type"] = request.ResourceType;
        d["properties"] = ToStringPropertyMap(request.Properties);

        // Use only 'create_resource' as the server does not support 'resource.create' anymore
        return client.SendAsync<ResourceInfo>(
            "create_resource",
            d,
            cancellationToken);
    }

    private static Dictionary<string, string> ToStringPropertyMap(IReadOnlyDictionary<string, object?> properties)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (k, v) in properties)
        {
            map[k] = v?.ToString() ?? string.Empty;
        }

        return map;
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
