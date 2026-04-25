namespace GodotMcp.Core.Models;

/// <summary>
/// Request for <c>query_system_documentation</c> (local DocFX / Markdown under the Godot MCP repository).
/// </summary>
public sealed class QuerySystemDocumentationRequest
{
    /// <param name="projectPath">Godot project directory (absolute or relative; normalized to an absolute path).</param>
    /// <param name="query">Case-insensitive substring filter; empty uses manifest-only behavior on the server.</param>
    /// <param name="repositoryRoot">Optional absolute path to the Godot MCP git repo root (folder containing docs/docfx.json).</param>
    /// <param name="source">Exactly <c>manifest</c>, <c>markdown</c>, or <c>both</c> (default).</param>
    public QuerySystemDocumentationRequest(
        string projectPath,
        string query = "",
        string? repositoryRoot = null,
        string source = DocumentationMcpValidation.SourceBoth)
    {
        ProjectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(projectPath);
        Query = query ?? "";
        RepositoryRoot = NormalizeOptionalAbsolutePath(repositoryRoot);
        Source = DocumentationMcpValidation.NormalizeSource(source);
    }

    /// <summary>Normalized absolute Godot project directory.</summary>
    public string ProjectPath { get; }

    public string Query { get; }

    /// <summary>Optional normalized absolute repository root.</summary>
    public string? RepositoryRoot { get; }

    /// <summary>Normalized source mode: manifest, markdown, or both.</summary>
    public string Source { get; }

    private static string? NormalizeOptionalAbsolutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path.Trim()));
    }
}

/// <summary>
/// Request for <c>query_godot_engine_documentation</c> (Read the Docs search API).
/// </summary>
public sealed class QueryGodotEngineDocumentationRequest
{
    /// <param name="projectPath">Godot project directory (absolute or relative; normalized).</param>
    /// <param name="query">Search terms (required).</param>
    /// <param name="version">RTD version label (default stable).</param>
    /// <param name="maxResults">Result cap; clamped to 1–40 on the client to match server expectations.</param>
    public QueryGodotEngineDocumentationRequest(
        string projectPath,
        string query,
        string version = "stable",
        int maxResults = 12)
    {
        ProjectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(projectPath);
        Query = DocumentationMcpValidation.ValidateEngineQuery(query);
        Version = DocumentationMcpValidation.NormalizeEngineVersion(version);
        MaxResults = DocumentationMcpValidation.ClampEngineMaxResults(maxResults);
    }

    public string ProjectPath { get; }

    public string Query { get; }

    public string Version { get; }

    public int MaxResults { get; }
}
