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

/// <summary>
/// Project command request for setting a project config value.
/// </summary>
public sealed record SetProjectConfigRequest
{
    public string Key { get; init; }
    public string Value { get; init; }
    public string? Section { get; init; }

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public SetProjectConfigRequest(string key, string value, string? section = null, string? projectRootPath = null)
    {
        Key = key;
        Value = value;
        Section = section;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for removing a project config key.
/// </summary>
public sealed record RemoveProjectConfigRequest
{
    public string Key { get; init; }
    public string? Section { get; init; }

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public RemoveProjectConfigRequest(string key, string? section = null, string? projectRootPath = null)
    {
        Key = key;
        Section = section;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for initializing a project with Main scene structure.
/// </summary>
/// <param name="ProjectPath">Project directory (absolute path or path relative to the configured project root).</param>
/// <param name="ProjectName">The name of the Godot project.</param>
/// <param name="Language">Script language (<c>gd</c> or <c>cs</c>). Defaults to <c>gd</c>.</param>
/// <param name="GameType">Game dimension (<c>2d</c> or <c>3d</c>). Defaults to <c>2d</c>.</param>
/// <param name="IncludeUi">Whether to include CanvasLayer HUD with score Label and restart Button.</param>
public sealed record InitializeProjectRequest(
    string ProjectPath,
    string? ProjectName = null,
    string Language = "gd",
    string GameType = "2d",
    bool IncludeUi = false);

/// <summary>
/// Project command request for creating an actor scene.
/// </summary>
/// <param name="ProjectPath">Project directory (absolute path or path relative to the configured project root).</param>
/// <param name="ActorName">Actor name used for the scene file and node name.</param>
/// <param name="Role">Actor role: <c>player</c>, <c>enemy</c>, or <c>npc</c>. Defaults to <c>enemy</c>.</param>
/// <param name="Language">Script language (<c>gd</c> or <c>cs</c>). Defaults to project metadata, then <c>gd</c>.</param>
/// <param name="GameType">Game dimension (<c>2d</c> or <c>3d</c>). Defaults to project metadata, then <c>2d</c>.</param>
/// <param name="CreateScript">Whether to create a script for this actor.</param>
/// <param name="AddToMain">Whether to instantiate this actor into Main.tscn.</param>
public sealed record CreateActorRequest(
    string ProjectPath,
    string ActorName,
    string Role = "enemy",
    string? Language = null,
    string? GameType = null,
    bool CreateScript = true,
    bool AddToMain = true);

/// <summary>
/// Project command request for creating a spawnable obstacle scene.
/// </summary>
/// <param name="ProjectPath">Project directory (absolute path or path relative to the configured project root).</param>
/// <param name="SpawnableName">Spawnable name used for the scene file and export variable.</param>
/// <param name="Language">Script language (<c>gd</c> or <c>cs</c>). Defaults to project metadata.</param>
/// <param name="GameType">Game dimension (<c>2d</c> or <c>3d</c>). Defaults to project metadata.</param>
/// <param name="CreateScript">Whether to create a script for this spawnable.</param>
/// <param name="WireToMain">Whether to add PackedScene export and signal wiring to the Main script.</param>
public sealed record CreateSpawnableRequest(
    string ProjectPath,
    string SpawnableName,
    string? Language = null,
    string? GameType = null,
    bool CreateScript = true,
    bool WireToMain = true);

/// <summary>
/// Project command request for setting up UI scaffolding.
/// </summary>
/// <param name="ProjectPath">Project directory (absolute path or path relative to the configured project root).</param>
/// <param name="Language">Script language (<c>gd</c> or <c>cs</c>). Defaults to project metadata.</param>
/// <param name="GameType">Game dimension (<c>2d</c> or <c>3d</c>). Defaults to project metadata.</param>
public sealed record SetupUiRequest(
    string ProjectPath,
    string? Language = null,
    string? GameType = null);
