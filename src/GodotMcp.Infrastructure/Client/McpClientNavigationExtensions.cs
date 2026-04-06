using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Navigation wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientNavigationExtensions
{
    /// <summary>
    /// Lists navigation regions in a scene.
    /// </summary>
    public static async Task<IReadOnlyList<NavigationRegionInfo>> NavigationListRegionsAsync(
        this IMcpClient client,
        NavigationListRegionsRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NavigationRegionInfo>>(
            "nav.list_regions",
            new Dictionary<string, object?> { ["scenePath"] = request.ScenePath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NavigationRegionInfo>();
    }

    /// <summary>
    /// Creates a navigation region in a scene.
    /// </summary>
    public static Task<NavigationRegionInfo?> NavigationCreateRegionAsync(
        this IMcpClient client,
        NavigationCreateRegionRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<NavigationRegionInfo>(
            "nav.create_region",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["parentPath"] = request.ParentPath,
                ["regionName"] = request.RegionName
            },
            cancellationToken);
    }

    /// <summary>
    /// Validates navigation setup in a scene.
    /// </summary>
    public static Task<NavigationResult?> NavigationValidateAsync(
        this IMcpClient client,
        NavigationValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<NavigationResult>(
            "nav.validate",
            new Dictionary<string, object?> { ["scenePath"] = request.ScenePath },
            cancellationToken);
    }

    /// <summary>
    /// Bakes navigation in a scene.
    /// </summary>
    public static Task<NavigationResult?> NavigationBakeAsync(
        this IMcpClient client,
        NavigationBakeRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<NavigationResult>(
            "nav.bake",
            new Dictionary<string, object?> { ["scenePath"] = request.ScenePath },
            cancellationToken);
    }
}
