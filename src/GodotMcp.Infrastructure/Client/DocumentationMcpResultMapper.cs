using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

public static class DocumentationMcpResultMapper
{
    public static GodotMcpDocumentationToolResult<QuerySystemDocumentationPayload> MapSystem(
        QuerySystemDocumentationMcpResponse? raw)
    {
        if (raw is null)
        {
            return new GodotMcpDocumentationToolResult<QuerySystemDocumentationPayload>(
                false,
                "Empty response from MCP server.",
                null,
                null);
        }

        if (!raw.Success)
        {
            return new GodotMcpDocumentationToolResult<QuerySystemDocumentationPayload>(
                false,
                raw.Message,
                raw.SuggestedRemediation,
                null);
        }

        var data = new QuerySystemDocumentationPayload(
            raw.RepositoryRoot,
            raw.Manifest,
            raw.Markdown);

        return new GodotMcpDocumentationToolResult<QuerySystemDocumentationPayload>(
            true,
            raw.Message,
            raw.SuggestedRemediation,
            data);
    }

    public static GodotMcpDocumentationToolResult<QueryGodotEngineDocumentationPayload> MapEngine(
        QueryGodotEngineDocumentationMcpResponse? raw)
    {
        if (raw is null)
        {
            return new GodotMcpDocumentationToolResult<QueryGodotEngineDocumentationPayload>(
                false,
                "Empty response from MCP server.",
                null,
                null);
        }

        if (!raw.Success)
        {
            return new GodotMcpDocumentationToolResult<QueryGodotEngineDocumentationPayload>(
                false,
                raw.Message,
                raw.SuggestedRemediation,
                null);
        }

        List<GodotEngineDocumentationHit> hits;
        if (raw.Hits is null)
        {
            hits = [];
        }
        else
        {
            hits = raw.Hits.Select(static h => new GodotEngineDocumentationHit(
                h.Type,
                h.Title,
                h.Path,
                h.Version,
                h.Url,
                h.Snippets ?? (IReadOnlyList<string>)Array.Empty<string>())).ToList();
        }

        var data = new QueryGodotEngineDocumentationPayload(
            raw.Source,
            raw.Query,
            raw.Version,
            raw.TotalCount,
            raw.NextPage,
            hits);

        return new GodotMcpDocumentationToolResult<QueryGodotEngineDocumentationPayload>(
            true,
            raw.Message,
            raw.SuggestedRemediation,
            data);
    }
}
