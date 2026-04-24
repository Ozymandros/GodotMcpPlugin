using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Lighting wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientLightingExtensions
{
    /// <summary>
    /// Lists lights under a project root path.
    /// </summary>
    public static async Task<IReadOnlyList<LightInfo>> LightListAsync(
        this IMcpClient client,
        LightListRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return await client.SendAsync<IReadOnlyList<LightInfo>>(
            "light.list",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<LightInfo>();
    }

    /// <summary>
    /// Creates a light in a scene.
    /// </summary>
    public static Task<LightInfo?> LightCreateAsync(
        this IMcpClient client,
        LightCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["parentNodePath"] = request.ParentNodePath;
        d["nodeName"] = request.NodeName;
        d["lightType"] = request.LightType;
        d["preset"] = request.Preset;
        return client.SendAsync<LightInfo>("light.create", d, cancellationToken);
    }

    /// <summary>
    /// Updates a light in a scene.
    /// </summary>
    public static Task<LightInfo?> LightUpdateAsync(
        this IMcpClient client,
        LightUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["nodePath"] = request.NodePath;
        d["properties"] = request.Properties;
        return client.SendAsync<LightInfo>("light.update", d, cancellationToken);
    }

    /// <summary>
    /// Validates lighting under a project root path.
    /// </summary>
    public static Task<LightValidationResult?> LightValidateAsync(
        this IMcpClient client,
        LightValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return client.SendAsync<LightValidationResult>(
            "light.validate",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
            cancellationToken);
    }

    /// <summary>
    /// Tunes an existing light (server: <c>light.update</c>).
    /// </summary>
    public static Task<LightInfo?> LightTuneAsync(
        this IMcpClient client,
        LightTuneRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["nodePath"] = request.NodePath;
        d["properties"] = request.Properties;
        return client.SendAsync<LightInfo>("light.update", d, cancellationToken);
    }
}
