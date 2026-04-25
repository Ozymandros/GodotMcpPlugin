using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed wrappers for Godot MCP documentation tools.
/// </summary>
public static class McpClientDocumentationExtensions
{
    /// <summary>
    /// Searches local Godot MCP system documentation (DocFX manifest and/or Markdown under docs/).
    /// MCP tool name: <c>query_system_documentation</c>.
    /// </summary>
    public static async Task<GodotMcpDocumentationToolResult<QuerySystemDocumentationPayload>> QuerySystemDocumentationAsync(
        this IMcpClient client,
        QuerySystemDocumentationRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["projectPath"] = request.ProjectPath,
            ["query"] = request.Query,
            ["repositoryRoot"] = request.RepositoryRoot,
            ["source"] = request.Source
        };

        var raw = await client.SendAsync<QuerySystemDocumentationMcpResponse>(
            "query_system_documentation",
            parameters,
            cancellationToken).ConfigureAwait(false);

        return DocumentationMcpResultMapper.MapSystem(raw);
    }

    /// <summary>
    /// Searches official Godot Engine documentation via Read the Docs (requires network on the server).
    /// MCP tool name: <c>query_godot_engine_documentation</c>.
    /// </summary>
    public static async Task<GodotMcpDocumentationToolResult<QueryGodotEngineDocumentationPayload>> QueryGodotEngineDocumentationAsync(
        this IMcpClient client,
        QueryGodotEngineDocumentationRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["projectPath"] = request.ProjectPath,
            ["query"] = request.Query,
            ["version"] = request.Version,
            ["max_results"] = request.MaxResults
        };

        var raw = await client.SendAsync<QueryGodotEngineDocumentationMcpResponse>(
            "query_godot_engine_documentation",
            parameters,
            cancellationToken).ConfigureAwait(false);

        return DocumentationMcpResultMapper.MapEngine(raw);
    }
}
