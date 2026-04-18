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
    [KernelFunction("list_regions")]
    [Description("Lists navigation regions in a scene.")]
    public Task<IReadOnlyList<NavigationRegionInfo>> ListRegionsAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationListRegionsAsync(
            new NavigationListRegionsRequest(new McpProjectFile(projectPath, fileName)),
            cancellationToken);

    /// <summary>
    /// Creates a navigation region in a scene.
    /// </summary>
    [KernelFunction("create_region")]
    [Description("Creates a navigation region in a scene.")]
    public Task<NavigationRegionInfo?> CreateRegionAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Parent node path.")] string parentPath,
        [Description("Region name.")] string regionName,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationCreateRegionAsync(
            new NavigationCreateRegionRequest(new McpProjectFile(projectPath, fileName), parentPath, regionName),
            cancellationToken);

    /// <summary>
    /// Validates navigation setup in a scene.
    /// </summary>
    [KernelFunction("validate")]
    [Description("Validates navigation setup in a scene.")]
    public Task<NavigationResult?> ValidateAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationValidateAsync(
            new NavigationValidateRequest(new McpProjectFile(projectPath, fileName)),
            cancellationToken);

    /// <summary>
    /// Bakes navigation in a scene.
    /// </summary>
    [KernelFunction("bake")]
    [Description("Bakes navigation in a scene.")]
    public Task<NavigationResult?> BakeAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.NavigationBakeAsync(
            new NavigationBakeRequest(new McpProjectFile(projectPath, fileName)),
            cancellationToken);
}
