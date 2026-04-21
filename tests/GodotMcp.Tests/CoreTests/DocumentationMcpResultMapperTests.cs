using System.Text.Json;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.CoreTests;

public class DocumentationMcpResultMapperTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void MapSystem_Failure_ReturnsNoData()
    {
        var raw = JsonSerializer.Deserialize<QuerySystemDocumentationMcpResponse>(
            """{"success":false,"message":"no repo","suggestedRemediation":"set root"}""",
            JsonOptions)!;

        var mapped = DocumentationMcpResultMapper.MapSystem(raw);

        Assert.False(mapped.Success);
        Assert.Equal("no repo", mapped.Message);
        Assert.Equal("set root", mapped.SuggestedRemediation);
        Assert.Null(mapped.Data);
    }

    [Fact]
    public void MapEngine_Success_MapsHits()
    {
        var raw = JsonSerializer.Deserialize<QueryGodotEngineDocumentationMcpResponse>(
            """
            {
              "success": true,
              "source": "rtd",
              "query": "Node",
              "version": "stable",
              "totalCount": 1,
              "hits": [
                {
                  "type": "page",
                  "title": "Node",
                  "path": "classes/class_node",
                  "version": "stable",
                  "url": "https://docs.godotengine.org/en/stable/classes/class_node.html",
                  "snippets": ["class <b>Node</b>"]
                }
              ]
            }
            """,
            JsonOptions)!;

        var mapped = DocumentationMcpResultMapper.MapEngine(raw);

        Assert.True(mapped.Success);
        Assert.NotNull(mapped.Data);
        Assert.Single(mapped.Data!.Hits);
        Assert.Equal("Node", mapped.Data.Hits[0].Title);
        Assert.Contains("Node", mapped.Data.Hits[0].Snippets[0], StringComparison.Ordinal);
    }

    [Fact]
    public void MapSystem_Null_ReturnsFailure()
    {
        var mapped = DocumentationMcpResultMapper.MapSystem(null);
        Assert.False(mapped.Success);
        Assert.Contains("Empty", mapped.Message, StringComparison.OrdinalIgnoreCase);
    }
}
