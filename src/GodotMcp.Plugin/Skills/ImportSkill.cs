using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Import MCP commands.
/// </summary>
public sealed class ImportSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Generates a Godot .import file for an asset.
    /// </summary>
    [KernelFunction("generate_import_file")]
    [Description("Generates a Godot .import file for a source asset.")]
    public Task<ImportOperationResult?> GenerateImportFileAsync(
        [Description("Source asset path.")] string assetPath,
        [Description("Godot importer identifier.")] string importer,
        [Description("Godot resource type token.")] string type,
        [Description("Optional importer parameters.")] IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) =>
        _mcp.GenerateImportFileAsync(new GenerateImportFileRequest(assetPath, importer, type, parameters), cancellationToken);

    /// <summary>
    /// Creates texture import/resource metadata.
    /// </summary>
    [KernelFunction("create_texture")]
    [Description("Creates texture import/resource metadata.")]
    public Task<ResourceInfo?> CreateTextureAsync(
        [Description("Texture source path.")] string texturePath,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateTextureAsync(new CreateTextureRequest(texturePath), cancellationToken);

    /// <summary>
    /// Creates audio import/resource metadata.
    /// </summary>
    [KernelFunction("create_audio")]
    [Description("Creates audio import/resource metadata.")]
    public Task<ResourceInfo?> CreateAudioAsync(
        [Description("Audio source path.")] string audioPath,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateAudioAsync(new CreateAudioRequest(audioPath), cancellationToken);

    /// <summary>
    /// Reimports an asset.
    /// </summary>
    [KernelFunction("reimport_asset")]
    [Description("Reimports an existing asset.")]
    public Task<ImportOperationResult?> ReimportAssetAsync(
        [Description("Asset path to reimport.")] string assetPath,
        CancellationToken cancellationToken = default) =>
        _mcp.ReimportAssetAsync(new ReimportAssetRequest(assetPath), cancellationToken);
}
