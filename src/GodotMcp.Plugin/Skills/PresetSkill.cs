using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing preset MCP commands.
/// </summary>
public sealed class PresetSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Applies a node preset.
    /// </summary>
    [KernelFunction("apply")]
    [Description("Applies a node preset.")]
    public Task<PresetResult?> ApplyAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Target node path.")] string nodePath,
        [Description("Preset name.")] string presetName,
        CancellationToken cancellationToken = default) =>
        _mcp.PresetApplyAsync(
            new PresetApplyRequest(new McpProjectFile(projectPath, fileName), nodePath, presetName),
            cancellationToken);
}
