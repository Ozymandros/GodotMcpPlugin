using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Import wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientImportExtensions
{
    /// <summary>
    /// Generates a Godot .import file for a source asset.
    /// </summary>
    public static Task<ImportOperationResult?> GenerateImportFileAsync(
        this IMcpClient client,
        GenerateImportFileRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ImportOperationResult>(
            "generate_import_file",
            new Dictionary<string, object?>
            {
                ["assetPath"] = request.AssetPath,
                ["importer"] = request.Importer,
                ["type"] = request.Type,
                ["parameters"] = request.Parameters
            },
            cancellationToken);
    }

    /// <summary>
    /// Creates a texture import/resource.
    /// </summary>
    public static Task<ResourceInfo?> CreateTextureAsync(
        this IMcpClient client,
        CreateTextureRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ResourceInfo>(
            "create_texture",
            new Dictionary<string, object?>
            {
                ["texturePath"] = request.TexturePath
            },
            cancellationToken);
    }

    /// <summary>
    /// Creates an audio import/resource.
    /// </summary>
    public static Task<ResourceInfo?> CreateAudioAsync(
        this IMcpClient client,
        CreateAudioRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ResourceInfo>(
            "create_audio",
            new Dictionary<string, object?>
            {
                ["audioPath"] = request.AudioPath
            },
            cancellationToken);
    }

    /// <summary>
    /// Reimports an asset.
    /// </summary>
    public static Task<ImportOperationResult?> ReimportAssetAsync(
        this IMcpClient client,
        ReimportAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ImportOperationResult>(
            "reimport_asset",
            new Dictionary<string, object?>
            {
                ["assetPath"] = request.AssetPath
            },
            cancellationToken);
    }
}
