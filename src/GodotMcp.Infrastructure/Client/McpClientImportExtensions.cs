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
        var d = McpProjectFilePayload.ToDictionary(request.Asset);
        d["importer"] = request.Importer;
        d["type"] = request.Type;
        d["parameters"] = request.Parameters;
        return client.SendAsync<ImportOperationResult>("generate_import_file", d, cancellationToken);
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
            McpProjectFilePayload.ToDictionary(request.Texture),
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
            McpProjectFilePayload.ToDictionary(request.Audio),
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
            McpProjectFilePayload.ToDictionary(request.Asset),
            cancellationToken);
    }
}
