namespace GodotMcp.Core.Models;

/// <summary>
/// Project command request for creating a Godot project.
/// </summary>
/// <param name="ProjectName">Project name.</param>
/// <param name="ProjectRootPath">Absolute filesystem path to the Godot project root.</param>
public sealed record CreateGodotProjectRequest
{
    public string ProjectName { get; init; }

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public CreateGodotProjectRequest(string projectName, string? projectRootPath = null)
    {
        ProjectName = projectName;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for configuring an autoload entry.
/// </summary>
/// <param name="Key">Autoload key/name.</param>
/// <param name="Value">Autoload script/resource path.</param>
/// <param name="Enabled">Whether the autoload entry is enabled.</param>
/// <param name="ProjectRootPath">Absolute filesystem path to the Godot project root.</param>
public sealed record ConfigureAutoloadRequest
{
    public string Key { get; init; }
    public string Value { get; init; }
    public bool Enabled { get; init; } = true;

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public ConfigureAutoloadRequest(string key, string value, bool enabled = true, string? projectRootPath = null)
    {
        Key = key;
        Value = value;
        Enabled = enabled;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for adding a plugin.
/// </summary>
/// <param name="PluginName">Plugin name to add.</param>
/// <param name="ProjectRootPath">Absolute filesystem path to the Godot project root.</param>
public sealed record AddPluginRequest
{
    public string PluginName { get; init; }

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public AddPluginRequest(string pluginName, string? projectRootPath = null)
    {
        PluginName = pluginName;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Represents a project-level operation result.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional operation message.</param>
public sealed record ProjectOperationResult(
    bool Success,
    string? Message = null);
