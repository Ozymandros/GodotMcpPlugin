namespace GodotMcp.Core.Models;

/// <summary>
/// Lint command request for advanced scene linting.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record LintSceneAdvancedRequest(string ScenePath);

/// <summary>
/// Lint command request for advanced project linting.
/// </summary>
/// <param name="ProjectPath">The project path.</param>
public sealed record LintProjectAdvancedRequest(string ProjectPath);

/// <summary>
/// Represents an advanced lint result payload.
/// </summary>
/// <param name="Issues">Detected lint issues.</param>
/// <param name="Success">Whether lint completed successfully.</param>
public sealed record LintResult(
    IReadOnlyList<LintIssue> Issues,
    bool Success = true);
