using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Import MCP commands.
/// </summary>
public sealed class ImportSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Generates a Godot .import file for a source asset.
    /// </summary>
    [KernelFunction("generate_import_file")]
    [Description("Generates a Godot .import file for a source asset.")]
    public Task<ImportOperationResult?> GenerateImportFileAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Asset file path relative to project root.")] string fileName,
        [Description("Godot importer identifier.")] string importer,
        [Description("Godot resource type token.")] string type,
        [Description("Optional importer parameters.")] IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) =>
        _mcp.GenerateImportFileAsync(
            new GenerateImportFileRequest(new McpProjectFile(projectPath, fileName), importer, type, parameters),
            cancellationToken);

    /// <summary>
    /// Creates texture import/resource metadata for the specified file.
    /// </summary>
    [KernelFunction("create_texture")]
    [Description("Creates texture import/resource metadata.")]
    public Task<ResourceInfo?> CreateTextureAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Texture file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateTextureAsync(new CreateTextureRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Creates audio import/resource metadata for the specified file.
    /// </summary>
    [KernelFunction("create_audio")]
    [Description("Creates audio import/resource metadata.")]
    public Task<ResourceInfo?> CreateAudioAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Audio file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateAudioAsync(new CreateAudioRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Reimports an existing asset and returns the import operation result.
    /// </summary>
    [KernelFunction("reimport_asset")]
    [Description("Reimports an existing asset.")]
    public Task<ImportOperationResult?> ReimportAssetAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Asset file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.ReimportAssetAsync(new ReimportAssetRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);
}
