using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed advanced lint wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientAdvancedLintExtensions
{
    /// <summary>
    /// Runs advanced linting for a scene.
    /// </summary>
    public static Task<LintResult?> LintSceneAdvancedAsync(
        this IMcpClient client,
        LintSceneAdvancedRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<LintResult>(
            "lint.scene_advanced",
            new Dictionary<string, object?> { ["scenePath"] = request.ScenePath },
            cancellationToken);
    }

    /// <summary>
    /// Runs advanced linting for a project.
    /// </summary>
    public static Task<LintResult?> LintProjectAdvancedAsync(
        this IMcpClient client,
        LintProjectAdvancedRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<LintResult>(
            "lint.project_advanced",
            new Dictionary<string, object?> { ["projectPath"] = request.ProjectPath },
            cancellationToken);
    }

    /// <summary>
    /// Runs lint checks against a project.
    /// </summary>
    public static async Task<LintResult?> LintProjectAsync(
        this IMcpClient client,
        LintProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?> { ["projectPath"] = request.ProjectPath };

        try
        {
            return await client.SendAsync<LintResult>("lint_project", payload, cancellationToken).ConfigureAwait(false);
        }
        catch (McpServerException ex) when (IsToolNotFound(ex))
        {
            return await client.SendAsync<LintResult>("lint.project_advanced", payload, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsToolNotFound(McpServerException ex)
        => ex.ErrorCode == -32601
           || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
           || ex.Message.Contains("unknown tool", StringComparison.OrdinalIgnoreCase);
}
