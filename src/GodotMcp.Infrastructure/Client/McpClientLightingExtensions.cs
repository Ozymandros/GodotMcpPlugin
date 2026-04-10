using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Lighting wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientLightingExtensions
{
    /// <summary>
    /// Lists lights in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The light list request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of lights. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<LightInfo>> LightListAsync(
        this IMcpClient client,
        LightListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<LightInfo>>(
            "light.list",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<LightInfo>();
    }

    /// <summary>
    /// Creates a light in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The light create request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created light information, or <c>null</c> when no payload is returned.</returns>
    public static Task<LightInfo?> LightCreateAsync(
        this IMcpClient client,
        LightCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<LightInfo>(
            "light.create",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["parentPath"] = request.ParentPath,
                ["lightName"] = request.LightName,
                ["lightType"] = request.LightType
            },
            cancellationToken);
    }

    /// <summary>
    /// Updates a light in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The light update request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated light information, or <c>null</c> when no payload is returned.</returns>
    public static Task<LightInfo?> LightUpdateAsync(
        this IMcpClient client,
        LightUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<LightInfo>(
            "light.update",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["lightPath"] = request.LightPath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Validates lighting setup for a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The light validate request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The lighting validation result, or <c>null</c> when no payload is returned.</returns>
    public static Task<LightValidationResult?> LightValidateAsync(
        this IMcpClient client,
        LightValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<LightValidationResult>(
            "light.validate",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath
            },
            cancellationToken);
    }

    /// <summary>
    /// Tunes an existing light in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The light tune request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The tuned light information, or <c>null</c> when no payload is returned.</returns>
    public static Task<LightInfo?> LightTuneAsync(
        this IMcpClient client,
        LightTuneRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<LightInfo>(
            "light.tune",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["lightPath"] = request.LightPath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }
}
