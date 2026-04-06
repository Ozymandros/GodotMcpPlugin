using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Lighting MCP commands.
/// </summary>
public sealed class LightingSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists lights in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of lights.</returns>
    [KernelFunction("list")]
    [Description("Lists lights in a scene.")]
    public Task<IReadOnlyList<LightInfo>> ListAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.LightListAsync(new LightListRequest(scenePath), cancellationToken);

    /// <summary>
    /// Creates a light in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="parentPath">Parent node path.</param>
    /// <param name="lightName">Light name.</param>
    /// <param name="lightType">Light type.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created light, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create")]
    [Description("Creates a light in a scene.")]
    public Task<LightInfo?> CreateAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Parent node path.")] string parentPath,
        [Description("Light name.")] string lightName,
        [Description("Light type.")] string lightType,
        CancellationToken cancellationToken = default) =>
        _mcp.LightCreateAsync(new LightCreateRequest(scenePath, parentPath, lightName, lightType), cancellationToken);

    /// <summary>
    /// Updates a light in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="lightPath">Light node path.</param>
    /// <param name="properties">Properties to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated light, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("update")]
    [Description("Updates a light in a scene.")]
    public Task<LightInfo?> UpdateAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Light node path.")] string lightPath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.LightUpdateAsync(new LightUpdateRequest(scenePath, lightPath, properties), cancellationToken);

    /// <summary>
    /// Validates lighting setup in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The lighting validation result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("validate")]
    [Description("Validates lighting setup in a scene.")]
    public Task<LightValidationResult?> ValidateAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.LightValidateAsync(new LightValidateRequest(scenePath), cancellationToken);

    /// <summary>
    /// Tunes an existing light in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="lightPath">Light node path.</param>
    /// <param name="properties">Properties to tune.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The tuned light, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("tune")]
    [Description("Tunes an existing light in a scene.")]
    public Task<LightInfo?> TuneAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Light node path.")] string lightPath,
        [Description("Properties to tune.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.LightTuneAsync(new LightTuneRequest(scenePath, lightPath, properties), cancellationToken);
}
