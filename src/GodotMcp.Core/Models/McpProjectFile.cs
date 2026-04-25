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

    public string ProjectPath { get; }

    public string FileName { get; }
}
