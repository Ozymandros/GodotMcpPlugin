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
}
