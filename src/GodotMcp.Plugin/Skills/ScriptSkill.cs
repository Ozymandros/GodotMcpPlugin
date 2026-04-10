using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Script MCP commands.
/// </summary>
public sealed class ScriptSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Creates a script file.
    /// </summary>
    [KernelFunction("create_script")]
    [Description("Creates a script file.")]
    public Task<ScriptInfo?> CreateScriptAsync(
        [Description("Script file path.")] string path,
        [Description("Script language (for example, CSharp or GDScript).")]
        string language,
        [Description("Godot base type for the script.")] string baseType,
        [Description("Optional class name.")] string? className = null,
        CancellationToken cancellationToken = default) =>
        _mcp.ScriptCreateAsync(new ScriptCreateRequest(path, language, baseType, className), cancellationToken);

    /// <summary>
    /// Attaches a script to a scene node.
    /// </summary>
    [KernelFunction("attach_script")]
    [Description("Attaches a script to a scene node.")]
    public Task<SceneCommandResult?> AttachScriptAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Node name/path in the scene.")] string nodeName,
        [Description("Script resource path.")] string scriptPath,
        CancellationToken cancellationToken = default) =>
        _mcp.ScriptAttachAsync(new ScriptAttachRequest(scenePath, nodeName, scriptPath), cancellationToken);

    /// <summary>
    /// Validates a script file.
    /// </summary>
    [KernelFunction("validate_script")]
    [Description("Validates a script file.")]
    public Task<ScriptValidationResult?> ValidateScriptAsync(
        [Description("Script resource path.")] string scriptPath,
        [Description("Whether validation should use C# rules.")] bool isCSharp = false,
        CancellationToken cancellationToken = default) =>
        _mcp.ScriptValidateAsync(new ScriptValidateRequest(scriptPath, isCSharp), cancellationToken);
}
