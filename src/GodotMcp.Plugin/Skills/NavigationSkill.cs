using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Navigation MCP commands.
/// </summary>
public sealed class NavigationSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists navigation regions in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of navigation regions.</returns>
    [KernelFunction("list_regions")]
    [Description("Lists navigation regions in a scene.")]
    public Task<IReadOnlyList<NavigationRegionInfo>> ListRegionsAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationListRegionsAsync(new NavigationListRegionsRequest(scenePath), cancellationToken);

    /// <summary>
    /// Creates a navigation region in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="parentPath">Parent node path.</param>
    /// <param name="regionName">Region name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created region, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create_region")]
    [Description("Creates a navigation region in a scene.")]
    public Task<NavigationRegionInfo?> CreateRegionAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Parent node path.")] string parentPath,
        [Description("Region name.")] string regionName,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationCreateRegionAsync(new NavigationCreateRegionRequest(scenePath, parentPath, regionName), cancellationToken);

    /// <summary>
    /// Validates navigation setup in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The navigation validation result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("validate")]
    [Description("Validates navigation setup in a scene.")]
    public Task<NavigationResult?> ValidateAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationValidateAsync(new NavigationValidateRequest(scenePath), cancellationToken);

    /// <summary>
    /// Bakes navigation in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The navigation bake result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("bake")]
    [Description("Bakes navigation in a scene.")]
    public Task<NavigationResult?> BakeAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationBakeAsync(new NavigationBakeRequest(scenePath), cancellationToken);
}
