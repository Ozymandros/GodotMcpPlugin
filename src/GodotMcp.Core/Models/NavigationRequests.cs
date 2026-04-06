namespace GodotMcp.Core.Models;

/// <summary>
/// Navigation command request for listing regions.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record NavigationListRegionsRequest(string ScenePath);

/// <summary>
/// Navigation command request for creating a region.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ParentPath">The parent node path for the region.</param>
/// <param name="RegionName">Navigation region name.</param>
public sealed record NavigationCreateRegionRequest(
    string ScenePath,
    string ParentPath,
    string RegionName);

/// <summary>
/// Navigation command request for validating navigation setup.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record NavigationValidateRequest(string ScenePath);

/// <summary>
/// Navigation command request for baking navigation data.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record NavigationBakeRequest(string ScenePath);

/// <summary>
/// Represents a navigation validation or bake result.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional result message.</param>
public sealed record NavigationResult(
    bool Success,
    string? Message = null);
