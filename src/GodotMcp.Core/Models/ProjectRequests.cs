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
public sealed record InitializeProjectRequest
{
    public string ProjectPath { get; init; }
    public string? ProjectName { get; init; }
    public string Language { get; init; } = "gd";
    public string GameType { get; init; } = "2d";
    public bool IncludeUi { get; init; }

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public InitializeProjectRequest(
        string projectPath,
        string? projectName = null,
        string language = "gd",
        string gameType = "2d",
        bool includeUi = false,
        string? projectRootPath = null)
    {
        ProjectPath = projectPath;
        ProjectName = projectName;
        Language = language;
        GameType = gameType;
        IncludeUi = includeUi;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for creating an actor scene.
/// </summary>
public sealed record CreateActorRequest
{
    public string ProjectPath { get; init; }
    public string ActorName { get; init; }
    public string Role { get; init; } = "enemy";
    public string? Language { get; init; }
    public string? GameType { get; init; }
    public bool CreateScript { get; init; } = true;
    public bool AddToMain { get; init; } = true;

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public CreateActorRequest(
        string projectPath,
        string actorName,
        string role = "enemy",
        string? language = null,
        string? gameType = null,
        bool createScript = true,
        bool addToMain = true,
        string? projectRootPath = null)
    {
        ProjectPath = projectPath;
        ActorName = actorName;
        Role = role;
        Language = language;
        GameType = gameType;
        CreateScript = createScript;
        AddToMain = addToMain;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for creating a spawnable obstacle scene.
/// </summary>
public sealed record CreateSpawnableRequest
{
    public string ProjectPath { get; init; }
    public string SpawnableName { get; init; }
    public string? Language { get; init; }
    public string? GameType { get; init; }
    public bool CreateScript { get; init; } = true;
    public bool WireToMain { get; init; } = true;

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public CreateSpawnableRequest(
        string projectPath,
        string spawnableName,
        string? language = null,
        string? gameType = null,
        bool createScript = true,
        bool wireToMain = true,
        string? projectRootPath = null)
    {
        ProjectPath = projectPath;
        SpawnableName = spawnableName;
        Language = language;
        GameType = gameType;
        CreateScript = createScript;
        WireToMain = wireToMain;
        _projectRootPath = projectRootPath;
    }
}

/// <summary>
/// Project command request for setting up UI scaffolding.
/// </summary>
public sealed record SetupUiRequest
{
    public string ProjectPath { get; init; }
    public string? Language { get; init; }
    public string? GameType { get; init; }

    private readonly string? _projectRootPath;

    public string ProjectRootPath => _projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath;

    public SetupUiRequest(
        string projectPath,
        string? language = null,
        string? gameType = null,
        string? projectRootPath = null)
    {
        ProjectPath = projectPath;
        Language = language;
        GameType = gameType;
        _projectRootPath = projectRootPath;
    }
}
