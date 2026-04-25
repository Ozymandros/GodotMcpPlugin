using System.ComponentModel;
using GodotMcp.Core.Models;
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
    [KernelFunction("list")]
    [Description("Lists camera nodes under a project root path.")]
    public Task<IReadOnlyList<CameraInfo>> ListAsync(
        [Description("Absolute filesystem path to the Godot project root to scan (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraListAsync(new CameraListRequest(projectPath), cancellationToken);

    /// <summary>
    /// Creates a camera node in a scene.
    /// </summary>
    [KernelFunction("create")]
    [Description("Creates a camera node in a scene.")]
    public Task<CameraInfo?> CreateAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Node path for the new camera.")] string nodePath,
        [Description("Camera type token (2d/3d or camera2d/camera3d).")] string cameraType,
        [Description("Optional camera preset.")] string? preset = null,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraCreateAsync(
            new CameraCreateRequest(new McpProjectFile(projectPath, fileName), nodePath, cameraType, preset),
            cancellationToken);

    /// <summary>
    /// Updates camera properties in a scene.
    /// </summary>
    [KernelFunction("update")]
    [Description("Updates camera properties in a scene.")]
    public Task<CameraInfo?> UpdateAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Camera node path to update.")] string nodePath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraUpdateAsync(
            new CameraUpdateRequest(new McpProjectFile(projectPath, fileName), nodePath, properties),
            cancellationToken);

    /// <summary>
    /// Validates camera configuration under a project root path.
    /// </summary>
    [KernelFunction("validate")]
    [Description("Validates camera configuration under a project root path.")]
    public Task<IReadOnlyList<CameraValidationIssue>> ValidateAsync(
        [Description("Absolute filesystem path to the Godot project root to validate (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.CameraValidateAsync(new CameraValidateRequest(projectPath), cancellationToken);
}
