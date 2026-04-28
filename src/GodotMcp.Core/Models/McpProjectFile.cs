namespace GodotMcp.Core.Models;

/// <summary>
/// Locates a project file using MCP tool payload fields:
/// <c>projectPath</c> (absolute filesystem path to the project root) and
/// <c>fileName</c> (path relative to that root, normalized as POSIX-style).
/// </summary>
public readonly record struct McpProjectFile
{
    public McpProjectFile(string projectPath, string fileName)
    {
        ProjectPath = McpPathNormalizer.NormalizeProjectRoot(projectPath);
        FileName = McpPathNormalizer.NormalizeProjectRelativeFileName(ProjectPath, fileName);
    }

    /// <summary>
    /// Creates a scene file reference following the MCP 1.7.2 scene contract:
    /// <c>projectPath + /scenes/ + fileName</c>, with strict <c>.tscn</c> validation.
    /// </summary>
    public static McpProjectFile ForScene(string projectPath, string fileName)
    {
        var normalizedRoot = McpPathNormalizer.NormalizeProjectRoot(projectPath);
        var normalizedSceneFile = McpPathNormalizer.NormalizeSceneFileName(normalizedRoot, fileName);
        return new McpProjectFile(normalizedRoot, normalizedSceneFile);
    }

    public string ProjectPath { get; }

    public string FileName { get; }
}
