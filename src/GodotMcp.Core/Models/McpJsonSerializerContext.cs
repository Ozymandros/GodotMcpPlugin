using System.Text.Json.Serialization;

namespace GodotMcp.Core.Models;

/// <summary>
/// JSON serializer context used where strongly typed source-generated access is required.
/// </summary>
[JsonSerializable(typeof(object))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Default)]
public partial class McpJsonSerializerContext : JsonSerializerContext
{
}
