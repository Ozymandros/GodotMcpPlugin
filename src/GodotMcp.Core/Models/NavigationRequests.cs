namespace GodotMcp.Core.Models;

/// <summary>
/// Navigation command request for listing regions.
/// </summary>
public sealed record NavigationListRegionsRequest(McpProjectFile Scene);

/// <summary>
/// Navigation command request for creating a region.
/// </summary>
public sealed record NavigationCreateRegionRequest(
    McpProjectFile Scene,
    string ParentPath,
    string RegionName);

/// <summary>
/// Navigation command request for validating navigation setup.
/// </summary>
public sealed record NavigationValidateRequest(McpProjectFile Scene);

/// <summary>
/// Navigation command request for baking navigation data.
/// </summary>
public sealed record NavigationBakeRequest(McpProjectFile Scene);

/// <summary>
/// Represents a navigation validation or bake result.
/// </summary>
public sealed record NavigationResult(
    bool Success,
    string? Message = null);
