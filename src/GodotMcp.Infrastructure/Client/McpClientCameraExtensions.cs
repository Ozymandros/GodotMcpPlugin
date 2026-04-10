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
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The camera list request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of camera descriptors. Returns an empty list when payload is empty.</returns>
    public static async Task<IReadOnlyList<CameraInfo>> CameraListAsync(
        this IMcpClient client,
        CameraListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<CameraInfo>>(
            "camera.list",
            new Dictionary<string, object?>
            {
                ["projectRootPath"] = request.ProjectRootPath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<CameraInfo>();
    }

    /// <summary>
    /// Creates a camera node in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The camera create request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created camera descriptor, or <c>null</c> when no payload is returned.</returns>
    public static Task<CameraInfo?> CameraCreateAsync(
        this IMcpClient client,
        CameraCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<CameraInfo>(
            "camera.create",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath,
                ["cameraType"] = request.CameraType,
                ["preset"] = request.Preset
            },
            cancellationToken);
    }

    /// <summary>
    /// Updates camera properties in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The camera update request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated camera descriptor, or <c>null</c> when no payload is returned.</returns>
    public static Task<CameraInfo?> CameraUpdateAsync(
        this IMcpClient client,
        CameraUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<CameraInfo>(
            "camera.update",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Validates camera configuration under a project root path.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The camera validate request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of camera validation issues. Returns an empty list when payload is empty.</returns>
    public static async Task<IReadOnlyList<CameraValidationIssue>> CameraValidateAsync(
        this IMcpClient client,
        CameraValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<CameraValidationIssue>>(
            "camera.validate",
            new Dictionary<string, object?>
            {
                ["projectRootPath"] = request.ProjectRootPath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<CameraValidationIssue>();
    }
}
