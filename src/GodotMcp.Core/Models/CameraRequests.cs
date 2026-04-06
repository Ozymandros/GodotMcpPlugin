namespace GodotMcp.Core.Models;

/// <summary>
/// Camera command request for listing cameras.
/// </summary>
/// <param name="ProjectRootPath">Project root path to scan.</param>
public sealed record CameraListRequest(string ProjectRootPath);

/// <summary>
/// Camera command request for creating a camera node.
/// </summary>
/// <param name="ScenePath">Scene path where camera is created.</param>
/// <param name="NodePath">Node path for the new camera.</param>
/// <param name="CameraType">Camera type token (2d/3d or camera2d/camera3d).</param>
/// <param name="Preset">Optional preset name.</param>
public sealed record CameraCreateRequest(
    string ScenePath,
    string NodePath,
    string CameraType,
    string? Preset = null);

/// <summary>
/// Camera command request for updating camera properties.
/// </summary>
/// <param name="ScenePath">Scene path containing the camera.</param>
/// <param name="NodePath">Node path of the camera to update.</param>
/// <param name="Properties">Camera properties to update.</param>
public sealed record CameraUpdateRequest(
    string ScenePath,
    string NodePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// Camera command request for validating camera configuration.
/// </summary>
/// <param name="ProjectRootPath">Project root path to validate.</param>
public sealed record CameraValidateRequest(string ProjectRootPath);

/// <summary>
/// Represents a camera validation issue.
/// </summary>
/// <param name="Path">Primary path associated with the issue.</param>
/// <param name="Severity">Issue severity level.</param>
/// <param name="Message">Issue message.</param>
/// <param name="SuggestedFix">Optional suggested remediation.</param>
/// <param name="Rule">Optional rule identifier.</param>
/// <param name="ScenePath">Optional related scene path.</param>
/// <param name="NodePath">Optional related node path.</param>
public sealed record CameraValidationIssue(
    string Path,
    string Severity,
    string Message,
    string? SuggestedFix = null,
    string? Rule = null,
    string? ScenePath = null,
    string? NodePath = null);
