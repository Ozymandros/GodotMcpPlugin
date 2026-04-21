using System.Text.Json;
using System.Text.Json.Serialization;

namespace GodotMcp.Core.Models;

/// <summary>
/// Deserialization shape for <c>query_system_documentation</c> MCP tool results.
/// </summary>
public sealed class QuerySystemDocumentationMcpResponse
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? SuggestedRemediation { get; set; }

    public string? RepositoryRoot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement Manifest { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement Markdown { get; set; }
}

/// <summary>
/// Deserialization shape for <c>query_godot_engine_documentation</c> MCP tool results.
/// </summary>
public sealed class QueryGodotEngineDocumentationMcpResponse
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? SuggestedRemediation { get; set; }

    public string? Source { get; set; }

    public string? Query { get; set; }

    public string? Version { get; set; }

    public int TotalCount { get; set; }

    public string? NextPage { get; set; }

    public List<GodotEngineDocumentationHitMcp>? Hits { get; set; }
}

/// <summary>
/// Hit row as returned by the server (Godot Engine docs search).
/// </summary>
public sealed class GodotEngineDocumentationHitMcp
{
    public string Type { get; set; } = "";

    public string Title { get; set; } = "";

    public string Path { get; set; } = "";

    public string Version { get; set; } = "";

    public string Url { get; set; } = "";

    public List<string>? Snippets { get; set; }
}
