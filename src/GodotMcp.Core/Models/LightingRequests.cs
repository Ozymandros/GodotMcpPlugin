namespace GodotMcp.Core.Models;

/// <summary>
/// Lighting command request for listing lights.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record LightListRequest(string ScenePath);

/// <summary>
/// Lighting command request for creating a light.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ParentPath">The parent node path for the new light.</param>
/// <param name="LightName">The light node name.</param>
/// <param name="LightType">The Godot light type.</param>
public sealed record LightCreateRequest(
    string ScenePath,
    string ParentPath,
    string LightName,
    string LightType);

/// <summary>
/// Lighting command request for updating a light.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="LightPath">The light node path.</param>
/// <param name="Properties">Properties to update on the light node.</param>
public sealed record LightUpdateRequest(
    string ScenePath,
    string LightPath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Lighting command request for validating light setup.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record LightValidateRequest(string ScenePath);

/// <summary>
/// Represents a lighting validation result.
/// </summary>
/// <param name="Success">Whether validation succeeded.</param>
/// <param name="Message">Optional validation message.</param>
public sealed record LightValidationResult(
    bool Success,
    string? Message = null);

/// <summary>
/// Lighting command request for tuning an existing light.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="LightPath">The light node path.</param>
/// <param name="Properties">Light properties to tune.</param>
public sealed record LightTuneRequest(
    string ScenePath,
    string LightPath,
    IReadOnlyDictionary<string, object?> Properties);
