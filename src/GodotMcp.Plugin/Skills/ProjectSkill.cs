using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Project MCP commands.
/// </summary>
public sealed class ProjectSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Creates a new Godot project.
    /// </summary>
    [KernelFunction("create_godot_project")]
    [Description("Creates a new Godot project.")]
    public Task<ProjectInfo?> CreateGodotProjectAsync(
        [Description("Project name.")] string projectName,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateGodotProjectAsync(
            new CreateGodotProjectRequest(projectName, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Gets information for the current Godot project.
    /// </summary>
    [KernelFunction("get_project_info")]
    [Description("Gets information for the current Godot project.")]
    public Task<ProjectInfo?> GetProjectInfoAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.GetProjectInfoAsync(projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath, cancellationToken);

    /// <summary>
    /// Configures an autoload entry.
    /// </summary>
    [KernelFunction("configure_autoload")]
    [Description("Configures an autoload entry in project settings.")]
    public Task<ProjectOperationResult?> ConfigureAutoloadAsync(
        [Description("Autoload key/name.")] string key,
        [Description("Autoload script/resource path.")] string value,
        [Description("Whether the autoload entry is enabled.")] bool enabled = true,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.ConfigureAutoloadAsync(
            new ConfigureAutoloadRequest(key, value, enabled, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Adds a plugin to the project.
    /// </summary>
    [KernelFunction("add_plugin")]
    [Description("Adds a plugin to the project.")]
    public Task<ProjectOperationResult?> AddPluginAsync(
        [Description("Plugin name to add.")] string pluginName,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.AddPluginAsync(
            new AddPluginRequest(pluginName, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Sets a project config value in project.godot.
    /// </summary>
    [KernelFunction("set_project_config")]
    [Description("Sets a project config value in project.godot.")]
    public Task<ProjectOperationResult?> SetProjectConfigAsync(
        [Description("Config key (e.g. 'application/config/name').")] string key,
        [Description("Config value to set.")] string value,
        [Description("Optional config section (e.g. 'application').")] string? section = null,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SetProjectConfigAsync(
            new SetProjectConfigRequest(key, value, section, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Removes a project config key from project.godot.
    /// </summary>
    [KernelFunction("remove_project_config")]
    [Description("Removes a project config key from project.godot.")]
    public Task<ProjectOperationResult?> RemoveProjectConfigAsync(
        [Description("Config key to remove.")] string key,
        [Description("Optional config section.")] string? section = null,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.RemoveProjectConfigAsync(
            new RemoveProjectConfigRequest(key, section, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Initializes a project with Main scene, Level, ObstacleContainer, and optional UI scaffold.
    /// </summary>
    [KernelFunction("initialize_project")]
    [Description("Creates a Main-first endless runner project structure with Main scene, Level, ObstacleContainer, and optional UI scaffold.")]
    public Task<ProjectOperationResult?> InitializeProjectAsync(
        [Description("Project directory (absolute path or path relative to the configured project root).")] string projectPath,
        [Description("The name of the Godot project.")] string? projectName = null,
        [Description("Script language ('gd' for GDScript, 'cs' for C#). Defaults to 'gd'.")] string language = "gd",
        [Description("Game dimension ('2d' or '3d'). Defaults to '2d'.")] string gameType = "2d",
        [Description("Include CanvasLayer HUD with score Label and restart Button.")] bool includeUi = false,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.InitializeProjectAsync(
            new InitializeProjectRequest(projectPath, projectName, language, gameType, includeUi, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Creates an actor scene with optional script and wiring to Main.tscn.
    /// </summary>
    [KernelFunction("create_actor")]
    [Description("Creates an actor scene and optionally instantiates it into Main.tscn. Supports player camera logic.")]
    public Task<ProjectOperationResult?> CreateActorAsync(
        [Description("Project directory (absolute path or path relative to the configured project root).")] string projectPath,
        [Description("Actor name (used for scene file and node name).")] string actorName,
        [Description("Actor role: 'player' for player-controlled, 'enemy' for AI, 'npc' for non-player character.")] string role = "enemy",
        [Description("Script language ('gd' or 'cs'). Defaults to 'gd' or project metadata.")] string? language = null,
        [Description("Game dimension ('2d' or '3d'). Defaults to '2d' or project metadata.")] string? gameType = null,
        [Description("Whether to create a script for this actor.")] bool createScript = true,
        [Description("Whether to instantiate this actor into Main.tscn.")] bool addToMain = true,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateActorAsync(
            new CreateActorRequest(projectPath, actorName, role, language, gameType, createScript, addToMain, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Creates a spawnable obstacle scene with optional wiring to Main script.
    /// </summary>
    [KernelFunction("create_spawnable")]
    [Description("Creates a spawnable obstacle scene with Area2D/Area3D, collision shape, and off-screen cleanup. Injects PackedScene export and signal wiring into Main script.")]
    public Task<ProjectOperationResult?> CreateSpawnableAsync(
        [Description("Project directory (absolute path or path relative to the configured project root).")] string projectPath,
        [Description("Spawnable name (used for scene file and export variable).")] string spawnableName,
        [Description("Script language ('gd' or 'cs'). Defaults to project metadata.")] string? language = null,
        [Description("Game dimension ('2d' or '3d'). Defaults to project metadata.")] string? gameType = null,
        [Description("Whether to create a script for this spawnable.")] bool createScript = true,
        [Description("Whether to add PackedScene export and signal to Main script.")] bool wireToMain = true,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.CreateSpawnableAsync(
            new CreateSpawnableRequest(projectPath, spawnableName, language, gameType, createScript, wireToMain, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);

    /// <summary>
    /// Sets up UI scaffolding with CanvasLayer HUD, UiManager script, and Main wiring.
    /// </summary>
    [KernelFunction("setup_ui")]
    [Description("Sets up a CanvasLayer HUD with score Label and restart Button, plus UiManager script. Wires to Main script for score/restart updates.")]
    public Task<ProjectOperationResult?> SetupUiAsync(
        [Description("Project directory (absolute path or path relative to the configured project root).")] string projectPath,
        [Description("Script language ('gd' or 'cs'). Defaults to project metadata.")] string? language = null,
        [Description("Game dimension ('2d' or '3d'). Defaults to project metadata.")] string? gameType = null,
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string? projectRootPath = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SetupUiAsync(
            new SetupUiRequest(projectPath, language, gameType, projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath),
            cancellationToken);
}
