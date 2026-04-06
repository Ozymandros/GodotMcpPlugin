namespace GodotMcp.Core.Models;

/// <summary>
/// Project command request for creating a Godot project.
/// </summary>
/// <param name="ProjectName">Project name.</param>
public sealed record CreateGodotProjectRequest(string ProjectName);

/// <summary>
/// Project command request for configuring an autoload entry.
/// </summary>
/// <param name="Key">Autoload key/name.</param>
/// <param name="Value">Autoload script/resource path.</param>
/// <param name="Enabled">Whether the autoload entry is enabled.</param>
public sealed record ConfigureAutoloadRequest(
    string Key,
    string Value,
    bool Enabled = true);

/// <summary>
/// Project command request for adding a plugin.
/// </summary>
/// <param name="PluginName">Plugin name to add.</param>
public sealed record AddPluginRequest(string PluginName);

/// <summary>
/// Represents a project-level operation result.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional operation message.</param>
public sealed record ProjectOperationResult(
    bool Success,
    string? Message = null);
