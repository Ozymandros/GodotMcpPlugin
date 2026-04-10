namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a lint issue returned by lint MCP commands.
/// </summary>
/// <param name="Code">Issue code identifier.</param>
/// <param name="Severity">Issue severity.</param>
/// <param name="Message">Issue message.</param>
/// <param name="Path">Optional file or node path.</param>
public sealed record LintIssue(
    string Code,
    string Severity,
    string Message,
    string? Path = null);
