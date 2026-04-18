namespace GodotMcp.Core.Models;

/// <summary>
/// Lint command request for advanced scene linting.
/// </summary>
public sealed record LintSceneAdvancedRequest(McpProjectFile Scene);

/// <summary>
/// Lint command request for advanced project linting.
/// </summary>
public sealed record LintProjectAdvancedRequest(string ProjectPath);

/// <summary>
/// Lint command request for project linting.
/// </summary>
public sealed record LintProjectRequest(string ProjectPath);

/// <summary>
/// Represents an advanced lint result payload.
/// </summary>
public sealed record LintResult(
    IReadOnlyList<LintIssue> Issues,
    bool Success = true);
