namespace GodotMcp.Core.Models;

/// <summary>
/// Camera command request for listing cameras (server: <c>projectPath</c> only).
/// </summary>
public sealed record CameraListRequest(string ProjectPath);

/// <summary>
/// Camera command request for creating a camera node.
/// </summary>
public sealed record CameraCreateRequest(
    McpProjectFile Scene,
    string NodePath,
    string CameraType,
    string? Preset = null);

/// <summary>
/// Camera command request for updating camera properties.
/// </summary>
public sealed record CameraUpdateRequest(
    McpProjectFile Scene,
    string NodePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Camera command request for validating camera configuration (server: <c>projectPath</c> only).
/// </summary>
public sealed record CameraValidateRequest(string ProjectPath);

/// <summary>
/// Represents a camera validation issue.
/// </summary>
public sealed record CameraValidationIssue(
    string Path,
    string Severity,
    string Message,
    string? SuggestedFix = null,
    string? Rule = null,
    string? ScenePath = null,
    string? NodePath = null);
