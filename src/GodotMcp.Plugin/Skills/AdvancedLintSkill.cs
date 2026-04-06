using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing advanced lint MCP commands.
/// </summary>
public sealed class AdvancedLintSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Runs advanced lint checks against a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The lint result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("scene_advanced")]
    [Description("Runs advanced lint checks for a scene.")]
    public Task<LintResult?> SceneAdvancedAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.LintSceneAdvancedAsync(new LintSceneAdvancedRequest(scenePath), cancellationToken);

    /// <summary>
    /// Runs advanced lint checks against a project.
    /// </summary>
    /// <param name="projectPath">Project path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The lint result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("project_advanced")]
    [Description("Runs advanced lint checks for a project.")]
    public Task<LintResult?> ProjectAdvancedAsync(
        [Description("Project path.")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.LintProjectAdvancedAsync(new LintProjectAdvancedRequest(projectPath), cancellationToken);
}
