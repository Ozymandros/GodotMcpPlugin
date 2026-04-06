using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Camera MCP commands.
/// </summary>
public sealed class CameraSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists camera nodes under a project root path.
    /// </summary>
    /// <param name="projectRootPath">Project root path to scan.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of camera descriptors.</returns>
    [KernelFunction("list")]
    [Description("Lists camera nodes under a project root path.")]
    public Task<IReadOnlyList<CameraInfo>> ListAsync(
        [Description("Project root path to scan.")] string projectRootPath,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraListAsync(new CameraListRequest(projectRootPath), cancellationToken);

    /// <summary>
    /// Creates a camera node in a scene.
    /// </summary>
    /// <param name="scenePath">Scene path where camera is created.</param>
    /// <param name="nodePath">Node path for the new camera.</param>
    /// <param name="cameraType">Camera type token.</param>
    /// <param name="preset">Optional camera preset.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created camera descriptor, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create")]
    [Description("Creates a camera node in a scene.")]
    public Task<CameraInfo?> CreateAsync(
        [Description("Scene path where camera is created.")] string scenePath,
        [Description("Node path for the new camera.")] string nodePath,
        [Description("Camera type token (2d/3d or camera2d/camera3d).")]
        string cameraType,
        [Description("Optional camera preset.")] string? preset = null,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraCreateAsync(new CameraCreateRequest(scenePath, nodePath, cameraType, preset), cancellationToken);

    /// <summary>
    /// Updates camera properties in a scene.
    /// </summary>
    /// <param name="scenePath">Scene path containing the camera.</param>
    /// <param name="nodePath">Camera node path to update.</param>
    /// <param name="properties">Properties to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated camera descriptor, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("update")]
    [Description("Updates camera properties in a scene.")]
    public Task<CameraInfo?> UpdateAsync(
        [Description("Scene path containing the camera.")] string scenePath,
        [Description("Camera node path to update.")] string nodePath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraUpdateAsync(new CameraUpdateRequest(scenePath, nodePath, properties), cancellationToken);

    /// <summary>
    /// Validates camera configuration under a project root path.
    /// </summary>
    /// <param name="projectRootPath">Project root path to validate.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of camera validation issues.</returns>
    [KernelFunction("validate")]
    [Description("Validates camera configuration under a project root path.")]
    public Task<IReadOnlyList<CameraValidationIssue>> ValidateAsync(
        [Description("Project root path to validate.")] string projectRootPath,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraValidateAsync(new CameraValidateRequest(projectRootPath), cancellationToken);
}
