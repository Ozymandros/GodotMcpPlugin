namespace GodotMcp.Core.Models;

/// <summary>
/// Default project path when the host does not supply one. Must be an absolute filesystem path
/// to the Godot project directory (the folder containing <c>project.godot</c>).
/// </summary>
public static class GodotMcpPathDefaults
{
    /// <summary>
    /// Placeholder root for development; production integrations should pass a real project directory.
    /// </summary>
    public const string DefaultProjectRootPath = @"C:\GodotProjects\McpDefaultProject";
}
