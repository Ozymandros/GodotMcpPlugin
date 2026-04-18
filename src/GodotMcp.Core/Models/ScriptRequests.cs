namespace GodotMcp.Core.Models;

/// <summary>
/// Script command request for creating a script file.
/// </summary>
public sealed record ScriptCreateRequest(
    McpProjectFile Script,
    string Language,
    string BaseType,
    string? ClassName = null);

/// <summary>
/// Script command request for attaching a script to a scene node.
/// </summary>
public sealed record ScriptAttachRequest(
    McpProjectFile Scene,
    string NodeName,
    McpProjectFile Script);

/// <summary>
/// Script command request for validating a script file.
/// </summary>
public sealed record ScriptValidateRequest(McpProjectFile Script, bool IsCSharp = false);

/// <summary>
/// Represents a created script.
/// </summary>
public sealed record ScriptInfo(
    string Path,
    string Language,
    string BaseType,
    string? ClassName = null);

/// <summary>
/// Represents a script validation result.
/// </summary>
public sealed record ScriptValidationResult(
    bool Success,
    string? Message = null,
    IReadOnlyList<string>? Errors = null,
    IReadOnlyList<string>? Warnings = null);
