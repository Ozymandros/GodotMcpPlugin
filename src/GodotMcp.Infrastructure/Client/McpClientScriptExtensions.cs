using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Script wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientScriptExtensions
{
    /// <summary>
    /// Creates a script file.
    /// </summary>
    public static Task<ScriptInfo?> ScriptCreateAsync(
        this IMcpClient client,
        ScriptCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ScriptInfo>(
            "create_script",
            new Dictionary<string, object?>
            {
                ["path"] = request.Path,
                ["language"] = request.Language,
                ["baseType"] = request.BaseType,
                ["className"] = request.ClassName
            },
            cancellationToken);
    }

    /// <summary>
    /// Attaches a script to a scene node.
    /// </summary>
    public static Task<SceneCommandResult?> ScriptAttachAsync(
        this IMcpClient client,
        ScriptAttachRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<SceneCommandResult>(
            "attach_script",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodeName"] = request.NodeName,
                ["scriptPath"] = request.ScriptPath
            },
            cancellationToken);
    }

    /// <summary>
    /// Validates a script file.
    /// </summary>
    public static Task<ScriptValidationResult?> ScriptValidateAsync(
        this IMcpClient client,
        ScriptValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ScriptValidationResult>(
            "validate_script",
            new Dictionary<string, object?>
            {
                ["scriptPath"] = request.ScriptPath,
                ["isCSharp"] = request.IsCSharp
            },
            cancellationToken);
    }
}
