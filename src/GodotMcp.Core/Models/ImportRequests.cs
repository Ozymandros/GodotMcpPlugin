namespace GodotMcp.Core.Models;

/// <summary>
/// Import command request for generating a .import file.
/// </summary>
public sealed record GenerateImportFileRequest(
    McpProjectFile Asset,
    string Importer,
    string Type,
    IReadOnlyDictionary<string, object?>? Parameters = null);

/// <summary>
/// Import command request for creating a texture resource/import setup.
/// </summary>
public sealed record CreateTextureRequest(McpProjectFile Texture);

/// <summary>
/// Import command request for creating an audio resource/import setup.
/// </summary>
public sealed record CreateAudioRequest(McpProjectFile Audio);

/// <summary>
/// Import command request for reimporting an asset.
/// </summary>
public sealed record ReimportAssetRequest(McpProjectFile Asset);

/// <summary>
/// Represents an import operation result.
/// </summary>
public sealed record ImportOperationResult(
    bool Success,
    string? Message = null,
    string? AssetPath = null,
    string? ImportPath = null);
