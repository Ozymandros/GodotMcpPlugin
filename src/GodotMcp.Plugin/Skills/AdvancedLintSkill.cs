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
    [KernelFunction("scene_advanced")]
    [Description("Runs advanced lint checks for a scene.")]
    public Task<LintResult?> SceneAdvancedAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.LintSceneAdvancedAsync(
            new LintSceneAdvancedRequest(new McpProjectFile(projectPath, fileName)),
            cancellationToken);

    /// <summary>
    /// Runs advanced lint checks against a project.
    /// </summary>
    [KernelFunction("project_advanced")]
    [Description("Runs advanced lint checks for a project.")]
    public Task<LintResult?> ProjectAdvancedAsync(
        [Description("Project path (res:// or absolute path under the project).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.LintProjectAdvancedAsync(new LintProjectAdvancedRequest(projectPath), cancellationToken);

    /// <summary>
    /// Runs lint checks against a project using the server-default lint contract.
    /// </summary>
    [KernelFunction("project")]
    [Description("Runs lint checks for a project.")]
    public Task<LintResult?> ProjectAsync(
        [Description("Project path (res:// or absolute path under the project).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.LintProjectAsync(new LintProjectRequest(projectPath), cancellationToken);
}
