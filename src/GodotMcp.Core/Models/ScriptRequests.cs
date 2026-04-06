namespace GodotMcp.Core.Models;

/// <summary>
/// Script command request for creating a script file.
/// </summary>
/// <param name="Path">Script file path.</param>
/// <param name="Language">Script language (for example, CSharp or GDScript).</param>
/// <param name="BaseType">Godot base type for the script.</param>
/// <param name="ClassName">Optional class name.</param>
public sealed record ScriptCreateRequest(
    string Path,
    string Language,
    string BaseType,
    string? ClassName = null);

/// <summary>
/// Script command request for attaching a script to a node.
/// </summary>
/// <param name="ScenePath">Scene resource path.</param>
/// <param name="NodeName">Node name/path within the scene.</param>
/// <param name="ScriptPath">Script resource path.</param>
public sealed record ScriptAttachRequest(
    string ScenePath,
    string NodeName,
    string ScriptPath);

/// <summary>
/// Script command request for validating a script file.
/// </summary>
/// <param name="ScriptPath">Script resource path.</param>
/// <param name="IsCSharp">Whether validation should use C# rules.</param>
public sealed record ScriptValidateRequest(
    string ScriptPath,
    bool IsCSharp = false);

/// <summary>
/// Represents a created script.
/// </summary>
/// <param name="Path">Script resource path.</param>
/// <param name="Language">Script language.</param>
/// <param name="BaseType">Godot base type.</param>
/// <param name="ClassName">Optional script class name.</param>
public sealed record ScriptInfo(
    string Path,
    string Language,
    string BaseType,
    string? ClassName = null);

/// <summary>
/// Represents a script validation result.
/// </summary>
/// <param name="Success">Whether validation succeeded.</param>
/// <param name="Message">Optional validation message.</param>
/// <param name="Errors">Validation errors.</param>
/// <param name="Warnings">Validation warnings.</param>
public sealed record ScriptValidationResult(
    bool Success,
    string? Message = null,
    IReadOnlyList<string>? Errors = null,
    IReadOnlyList<string>? Warnings = null);
