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
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["nodePath"] = request.NodePath;
        d["presetName"] = request.PresetName;
        return client.SendAsync<PresetResult>("preset.apply", d, cancellationToken);
    }
}
