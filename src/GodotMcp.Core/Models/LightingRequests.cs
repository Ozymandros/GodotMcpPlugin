namespace GodotMcp.Core.Models;

/// <summary>
/// Lighting command request for listing lights (server: <c>projectPath</c> only).
/// </summary>
public sealed record LightListRequest(string ProjectPath);

/// <summary>
/// Lighting command request for creating a light.
/// </summary>
public sealed record LightCreateRequest(
    McpProjectFile Scene,
    string ParentNodePath,
    string NodeName,
    string LightType,
    string? Preset = null);

/// <summary>
/// Lighting command request for updating a light.
/// </summary>
public sealed record LightUpdateRequest(
    McpProjectFile Scene,
    string NodePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Lighting command request for validating lighting (server: <c>projectPath</c> only).
/// </summary>
public sealed record LightValidateRequest(string ProjectPath);

/// <summary>
/// Represents a lighting validation result.
/// </summary>
public sealed record LightValidationResult(
    bool Success,
    string? Message = null);

/// <summary>
/// Lighting command request for tuning an existing light (maps to server <c>light.update</c>).
/// </summary>
public sealed record LightTuneRequest(
    McpProjectFile Scene,
    string NodePath,
    IReadOnlyDictionary<string, object?> Properties);
