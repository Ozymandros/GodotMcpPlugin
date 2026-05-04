using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Project wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientProjectExtensions
{
    /// <summary>
    /// Creates a new Godot project.
    /// </summary>
    public static Task<ProjectInfo?> CreateGodotProjectAsync(
        this IMcpClient client,
        CreateGodotProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectRootPath);
        return client.SendAsync<ProjectInfo>(
            "create_godot_project",
            new Dictionary<string, object?>
            {
                ["projectPath"] = projectPath,
                ["projectName"] = request.ProjectName
            },
            cancellationToken);
    }

    /// <summary>
    /// Gets information for the current Godot project.
    /// </summary>
    public static Task<ProjectInfo?> GetProjectInfoAsync(
        this IMcpClient client,
        string? projectRootPath = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(projectRootPath ?? GodotMcpPathDefaults.DefaultProjectRootPath);
        return client.SendAsync<ProjectInfo>(
            "get_project_info",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
            cancellationToken);
    }

    /// <summary>
    /// Configures an autoload entry in project settings.
    /// </summary>
    public static Task<ProjectOperationResult?> ConfigureAutoloadAsync(
        this IMcpClient client,
        ConfigureAutoloadRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectRootPath);
        return client.SendAsync<ProjectOperationResult>(
            "configure_autoload",
            new Dictionary<string, object?>
            {
                ["projectPath"] = projectPath,
                ["key"] = request.Key,
                ["value"] = request.Value,
                ["enabled"] = request.Enabled
            },
            cancellationToken);
    }

    /// <summary>
    /// Adds a plugin to the project.
    /// </summary>
    public static Task<ProjectOperationResult?> AddPluginAsync(
        this IMcpClient client,
        AddPluginRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectRootPath);
        return client.SendAsync<ProjectOperationResult>(
            "add_plugin",
            new Dictionary<string, object?>
            {
                ["projectPath"] = projectPath,
                ["pluginName"] = request.PluginName
            },
            cancellationToken);
    }

    /// <summary>
    /// Sets a project config value in project.godot.
    /// </summary>
    public static Task<ProjectOperationResult?> SetProjectConfigAsync(
        this IMcpClient client,
        SetProjectConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectRootPath);
        var parameters = new Dictionary<string, object?>
        {
            ["projectPath"] = projectPath,
            ["key"] = request.Key,
            ["value"] = request.Value
        };
        if (!string.IsNullOrWhiteSpace(request.Section))
        {
            parameters["section"] = request.Section;
        }
        return client.SendAsync<ProjectOperationResult>(
            "project.set_config_value",
            parameters,
            cancellationToken);
    }

    /// <summary>
    /// Removes a project config key from project.godot.
    /// </summary>
    public static Task<ProjectOperationResult?> RemoveProjectConfigAsync(
        this IMcpClient client,
        RemoveProjectConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectRootPath);
        var parameters = new Dictionary<string, object?>
        {
            ["projectPath"] = projectPath,
            ["key"] = request.Key
        };
        if (!string.IsNullOrWhiteSpace(request.Section))
        {
            parameters["section"] = request.Section;
        }
        return client.SendAsync<ProjectOperationResult>(
            "project.remove_config_key",
            parameters,
            cancellationToken);
    }
}
