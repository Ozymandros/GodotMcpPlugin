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
        var d = McpProjectFilePayload.ToDictionary(request.Script);
        d["language"] = request.Language;
        d["baseType"] = request.BaseType;
        d["className"] = request.ClassName;
        return client.SendAsync<ScriptInfo>("create_script", d, cancellationToken);
    }

    /// <summary>
    /// Attaches a script to a scene node.
    /// </summary>
    public static Task<SceneCommandResult?> ScriptAttachAsync(
        this IMcpClient client,
        ScriptAttachRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.Scene.ProjectPath, request.Script.ProjectPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("Scene and script must use the same projectPath (GodotMCP.Server attach_script contract).");
        }

        return client.SendAsync<SceneCommandResult>(
            "attach_script",
            new Dictionary<string, object?>
            {
                ["projectPath"] = request.Scene.ProjectPath,
                ["fileName"] = request.Scene.FileName,
                ["nodeName"] = request.NodeName,
                ["scriptFileName"] = request.Script.FileName
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
        var d = McpProjectFilePayload.ToDictionary(request.Script);
        d["isCSharp"] = request.IsCSharp;
        return client.SendAsync<ScriptValidationResult>("validate_script", d, cancellationToken);
    }
}
