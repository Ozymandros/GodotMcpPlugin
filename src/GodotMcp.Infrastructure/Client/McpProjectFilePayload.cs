using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Maps <see cref="McpProjectFile"/> to GodotMCP.Server tool argument names.
/// </summary>
internal static class McpProjectFilePayload
{
    public static Dictionary<string, object?> ToDictionary(McpProjectFile file) =>
        new()
        {
            ["projectPath"] = file.ProjectPath,
            ["fileName"] = file.FileName
        };
}
