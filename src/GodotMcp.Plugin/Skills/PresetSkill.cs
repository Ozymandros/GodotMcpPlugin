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
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="nodePath">Target node path.</param>
    /// <param name="presetName">Preset name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The preset result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("apply")]
    [Description("Applies a node preset.")]
    public Task<PresetResult?> ApplyAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Target node path.")] string nodePath,
        [Description("Preset name.")] string presetName,
        CancellationToken cancellationToken = default) =>
        _mcp.PresetApplyAsync(new PresetApplyRequest(scenePath, nodePath, presetName), cancellationToken);
}
