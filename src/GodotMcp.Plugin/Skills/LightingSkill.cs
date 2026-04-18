using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Lighting MCP commands.
/// </summary>
public sealed class LightingSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists lights across scenes under a project root.
    /// </summary>
    [KernelFunction("list")]
    [Description("Lists lights across scenes under a project root.")]
    public Task<IReadOnlyList<LightInfo>> ListAsync(
        [Description("Project root path to scan (res:// or absolute path under the project).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.LightListAsync(new LightListRequest(projectPath), cancellationToken);

    /// <summary>
    /// Creates a light in a scene.
    /// </summary>
    [KernelFunction("create")]
    [Description("Creates a light in a scene.")]
    public Task<LightInfo?> CreateAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Parent node path.")] string parentNodePath,
        [Description("Light node name.")] string nodeName,
        [Description("Light type.")] string lightType,
        [Description("Optional preset: sun, fill, spot.")] string? preset = null,
        CancellationToken cancellationToken = default) =>
        _mcp.LightCreateAsync(
            new LightCreateRequest(new McpProjectFile(projectPath, fileName), parentNodePath, nodeName, lightType, preset),
            cancellationToken);

    /// <summary>
    /// Updates a light in a scene.
    /// </summary>
    [KernelFunction("update")]
    [Description("Updates a light in a scene.")]
    public Task<LightInfo?> UpdateAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Light node path.")] string nodePath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.LightUpdateAsync(
            new LightUpdateRequest(new McpProjectFile(projectPath, fileName), nodePath, properties),
            cancellationToken);

    /// <summary>
    /// Validates lighting under a project root.
    /// </summary>
    [KernelFunction("validate")]
    [Description("Validates lighting under a project root.")]
    public Task<LightValidationResult?> ValidateAsync(
        [Description("Project root path to validate (res:// or absolute path under the project).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.LightValidateAsync(new LightValidateRequest(projectPath), cancellationToken);

    /// <summary>
    /// Tunes an existing light (server: light.update).
    /// </summary>
    [KernelFunction("tune")]
    [Description("Tunes an existing light in a scene.")]
    public Task<LightInfo?> TuneAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Light node path.")] string nodePath,
        [Description("Properties to tune.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.LightTuneAsync(
            new LightTuneRequest(new McpProjectFile(projectPath, fileName), nodePath, properties),
            cancellationToken);
}
