using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed preset wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientPresetExtensions
{
    /// <summary>
    /// Applies a preset to a target node.
    /// </summary>
    public static Task<PresetResult?> PresetApplyAsync(
        this IMcpClient client,
        PresetApplyRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<PresetResult>(
            "preset.apply",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath,
                ["presetName"] = request.PresetName
            },
            cancellationToken);
    }
}
