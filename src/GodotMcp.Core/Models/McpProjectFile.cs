namespace GodotMcp.Core.Models;

/// <summary>
/// Locates a project file using the same pair as GodotMCP.Server MCP tools:
/// <c>projectPath</c> (typically <c>res://</c> or an absolute project root) and
/// <c>fileName</c> (path relative to that root, POSIX-style).
/// </summary>
public readonly record struct McpProjectFile(string ProjectPath, string FileName);
