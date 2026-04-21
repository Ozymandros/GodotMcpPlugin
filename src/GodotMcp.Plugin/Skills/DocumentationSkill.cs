using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Godot MCP documentation query tools (same names and parameters as the server).
/// </summary>
public sealed class DocumentationSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Searches local Godot MCP system documentation (DocFX <c>_site/manifest.json</c> and/or Markdown under <c>docs/</c>).
    /// </summary>
    [KernelFunction("query_system_documentation")]
    [Description(
        "Searches local Godot MCP system documentation (DocFX manifest and/or Markdown). " +
        "Requires docs/docfx.json in the repo; for manifest search build with docfx first. " +
        "Optional GODOT_MCP_REPO_ROOT when the server runs outside the git repo.")]
    public Task<GodotMcpDocumentationToolResult<QuerySystemDocumentationPayload>> QuerySystemDocumentationAsync(
        [Description("Godot project directory (absolute or relative; normalized to absolute).")] string projectPath,
        [Description("Case-insensitive substring filter; empty uses manifest-only behavior (Markdown scan skipped).")] string query = "",
        [Description("Optional absolute path to Godot MCP git repo root (folder containing docs/docfx.json).")] string? repositoryRoot = null,
        [Description("Exactly manifest, markdown, or both (default).")] string source = DocumentationMcpValidation.SourceBoth,
        CancellationToken cancellationToken = default) =>
        _mcp.QuerySystemDocumentationAsync(
            new QuerySystemDocumentationRequest(projectPath, query, repositoryRoot, source),
            cancellationToken);

    /// <summary>
    /// Searches official Godot Engine documentation via Read the Docs (HTTPS; requires network on the MCP server).
    /// </summary>
    [KernelFunction("query_godot_engine_documentation")]
    [Description("Searches official Godot Engine documentation (Read the Docs API). Requires outbound HTTPS to docs.godotengine.org on the server.")]
    public Task<GodotMcpDocumentationToolResult<QueryGodotEngineDocumentationPayload>> QueryGodotEngineDocumentationAsync(
        [Description("Godot project directory (absolute or relative; normalized to absolute).")] string projectPath,
        [Description("Search terms (class names, topics, methods, etc.).")] string query,
        [Description("Documentation version label (e.g. stable, latest); max 64 chars; no URL schemes.")] string version = "stable",
        [Description("Maximum hits to return (1–40; clamped client-side).")] int maxResults = 12,
        CancellationToken cancellationToken = default) =>
        _mcp.QueryGodotEngineDocumentationAsync(
            new QueryGodotEngineDocumentationRequest(projectPath, query, version, maxResults),
            cancellationToken);
}
