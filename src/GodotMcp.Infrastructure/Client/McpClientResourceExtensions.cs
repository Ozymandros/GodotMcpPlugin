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
        // Extract rawContent if present in properties and forward it as top-level field
        var (rawContent, remaining) = ExtractRawContent(request.Properties);
        if (rawContent is not null)
        {
            d["rawContent"] = rawContent;
        }
        d["properties"] = ToStringPropertyMap(remaining);
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
        // Extract rawContent if present in properties and forward it as top-level field
        var (rawContentCreate, remainingCreate) = ExtractRawContent(request.Properties);
        if (rawContentCreate is not null)
        {
            d["rawContent"] = rawContentCreate;
        }
        d["properties"] = ToStringPropertyMap(remainingCreate);

        // Use only 'create_resource' as the server does not support 'resource.create' anymore
        return client.SendAsync<ResourceInfo>(
            "create_resource",
            d,
            cancellationToken);
    }

    private static (string? rawContent, IReadOnlyDictionary<string, object?> remaining) ExtractRawContent(IReadOnlyDictionary<string, object?>? properties)
    {
        if (properties == null || properties.Count == 0)
        {
            return (null, new Dictionary<string, object?>(StringComparer.Ordinal));
        }

        string? raw = null;
        var remaining = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (k, v) in properties)
        {
            if (string.Equals(k, "rawContent", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(k, "content", StringComparison.OrdinalIgnoreCase))
            {
                if (v is string s)
                {
                    raw = s;
                }
                else if (v is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    raw = je.GetString();
                }
                else if (v != null)
                {
                    raw = v.ToString();
                }
            }
            else
            {
                remaining[k] = v;
            }
        }

        return (raw, remaining);
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
