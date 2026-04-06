namespace GodotMcp.Core.Models;

/// <summary>
/// Import command request for generating a .import file.
/// </summary>
/// <param name="AssetPath">Source asset path.</param>
/// <param name="Importer">Godot importer identifier.</param>
/// <param name="Type">Godot resource type token.</param>
/// <param name="Parameters">Optional importer parameters.</param>
public sealed record GenerateImportFileRequest(
    string AssetPath,
    string Importer,
    string Type,
    IReadOnlyDictionary<string, object?>? Parameters = null);

/// <summary>
/// Import command request for creating a texture resource/import setup.
/// </summary>
/// <param name="TexturePath">Texture source path.</param>
public sealed record CreateTextureRequest(string TexturePath);

/// <summary>
/// Import command request for creating an audio resource/import setup.
/// </summary>
/// <param name="AudioPath">Audio source path.</param>
public sealed record CreateAudioRequest(string AudioPath);

/// <summary>
/// Import command request for reimporting an asset.
/// </summary>
/// <param name="AssetPath">Asset path to reimport.</param>
public sealed record ReimportAssetRequest(string AssetPath);

/// <summary>
/// Represents an import operation result.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional operation message.</param>
/// <param name="AssetPath">Optional asset path associated with the result.</param>
/// <param name="ImportPath">Optional generated import file path.</param>
public sealed record ImportOperationResult(
    bool Success,
    string? Message = null,
    string? AssetPath = null,
    string? ImportPath = null);
