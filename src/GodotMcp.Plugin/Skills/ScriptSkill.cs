using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Script MCP commands.
/// </summary>
public sealed class ScriptSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    [KernelFunction("create_script")]
    [Description("Creates a script file.")]
    public Task<ScriptInfo?> CreateScriptAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Script file path relative to project root (e.g. scripts/player.cs).")] string fileName,
        [Description("Script language ('gd' or 'cs' per server).")] string language,
        [Description("Godot base type for the script.")] string baseType,
        [Description("Optional class name.")] string? className = null,
        CancellationToken cancellationToken = default) =>
        _mcp.ScriptCreateAsync(
            new ScriptCreateRequest(new McpProjectFile(projectPath, fileName), language, baseType, className),
            cancellationToken);

    [KernelFunction("attach_script")]
    [Description("Attaches a script to a scene node (scene and script must use the same projectPath).")]
    public Task<SceneCommandResult?> AttachScriptAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root (attach_script fileName).")] string fileName,
        [Description("Target node name in the scene.")] string nodeName,
        [Description("Script file path relative to project root (attach_script scriptFileName).")] string scriptFileName,
        CancellationToken cancellationToken = default) =>
        _mcp.ScriptAttachAsync(
            new ScriptAttachRequest(
                new McpProjectFile(projectPath, fileName),
                nodeName,
                new McpProjectFile(projectPath, scriptFileName)),
            cancellationToken);

    [KernelFunction("validate_script")]
    [Description("Validates a script file.")]
    public Task<ScriptValidationResult?> ValidateScriptAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Script file path relative to project root.")] string fileName,
        [Description("Whether validation should use C# rules.")] bool isCSharp = false,
        CancellationToken cancellationToken = default) =>
        _mcp.ScriptValidateAsync(new ScriptValidateRequest(new McpProjectFile(projectPath, fileName), isCSharp), cancellationToken);
}
