using System.Text.Json.Serialization;

namespace GodotMcp.Core.Models;

/// <summary>
/// JSON serializer context used where strongly typed source-generated access is required.
/// </summary>
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(QuerySystemDocumentationMcpResponse))]
[JsonSerializable(typeof(QueryGodotEngineDocumentationMcpResponse))]
[JsonSerializable(typeof(GodotEngineDocumentationHitMcp))]
[JsonSerializable(typeof(List<GodotEngineDocumentationHitMcp>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Default)]
public partial class McpJsonSerializerContext : JsonSerializerContext
{
}
