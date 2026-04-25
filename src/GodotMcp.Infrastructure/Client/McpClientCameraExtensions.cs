using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Camera wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientCameraExtensions
{
    /// <summary>
    /// Lists camera nodes under a project root path.
    /// </summary>
    public static async Task<IReadOnlyList<CameraInfo>> CameraListAsync(
        this IMcpClient client,
        CameraListRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return await client.SendAsync<IReadOnlyList<CameraInfo>>(
            "camera.list",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<CameraInfo>();
    }

    /// <summary>
    /// Creates a camera node in a scene.
    /// </summary>
    public static Task<CameraInfo?> CameraCreateAsync(
        this IMcpClient client,
        CameraCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["nodePath"] = request.NodePath;
        d["cameraType"] = request.CameraType;
        d["preset"] = request.Preset;
        return client.SendAsync<CameraInfo>("camera.create", d, cancellationToken);
    }

    /// <summary>
    /// Updates camera properties in a scene.
    /// </summary>
    public static Task<CameraInfo?> CameraUpdateAsync(
        this IMcpClient client,
        CameraUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["nodePath"] = request.NodePath;
        d["properties"] = request.Properties;
        return client.SendAsync<CameraInfo>("camera.update", d, cancellationToken);
    }

    /// <summary>
    /// Validates camera configuration under a project root path.
    /// </summary>
    public static async Task<IReadOnlyList<CameraValidationIssue>> CameraValidateAsync(
        this IMcpClient client,
        CameraValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return await client.SendAsync<IReadOnlyList<CameraValidationIssue>>(
            "camera.validate",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<CameraValidationIssue>();
    }
}
