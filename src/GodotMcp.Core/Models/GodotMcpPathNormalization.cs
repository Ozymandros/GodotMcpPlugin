namespace GodotMcp.Core.Models;

/// <summary>
/// Public entry points for path normalization shared across MCP request models.
/// </summary>
public static class GodotMcpPathNormalization
{
    /// <summary>
    /// Returns a canonical absolute filesystem path for a Godot project directory.
    /// </summary>
    public static string NormalizeProjectDirectory(string projectPath) =>
        McpPathNormalizer.NormalizeProjectRoot(projectPath);
}
