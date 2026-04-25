using System.Text.Json;

namespace GodotMcp.Core.Models;

/// <summary>
/// Normalized MCP tool outcome for documentation commands (maps success, message, suggestedRemediation, and payload).
/// </summary>
/// <typeparam name="TPayload">Structured data when <see cref="Success"/> is true.</typeparam>
public sealed record GodotMcpDocumentationToolResult<TPayload>(
    bool Success,
    string? Message,
    string? SuggestedRemediation,
    TPayload? Data)
    where TPayload : class;

/// <summary>
/// Payload for local system documentation search results.
/// </summary>
public sealed record QuerySystemDocumentationPayload(
    string? RepositoryRoot,
    JsonElement Manifest,
    JsonElement Markdown);

/// <summary>
/// One hit from the official Godot Engine documentation search.
/// </summary>
public sealed record GodotEngineDocumentationHit(
    string Type,
    string Title,
    string Path,
    string Version,
    string Url,
    IReadOnlyList<string> Snippets);

/// <summary>
/// Payload for Godot Engine documentation search results.
/// </summary>
public sealed record QueryGodotEngineDocumentationPayload(
    string? Source,
    string? Query,
    string? Version,
    int TotalCount,
    string? NextPage,
    IReadOnlyList<GodotEngineDocumentationHit> Hits);
