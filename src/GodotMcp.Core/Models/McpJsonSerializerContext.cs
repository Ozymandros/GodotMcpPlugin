using System.Text.Json.Serialization;

namespace GodotMcp.Core.Models;

/// <summary>
/// JSON serializer context for MCP domain models with source generation support
/// </summary>
[JsonSerializable(typeof(McpToolDefinition))]
[JsonSerializable(typeof(McpParameterDefinition))]
[JsonSerializable(typeof(McpReturnType))]
[JsonSerializable(typeof(McpRequest))]
[JsonSerializable(typeof(McpResponse))]
[JsonSerializable(typeof(McpError))]
[JsonSerializable(typeof(ProcessInfo))]
[JsonSerializable(typeof(ConnectionState))]
[JsonSerializable(typeof(ProcessState))]
[JsonSerializable(typeof(Dictionary<string, McpParameterDefinition>))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, McpParameterDefinition>))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, object?>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(long[]))]
[JsonSerializable(typeof(float[]))]
[JsonSerializable(typeof(double[]))]
[JsonSerializable(typeof(bool[]))]
[JsonSerializable(typeof(object[]))]
[JsonSerializable(typeof(Vector2))]
[JsonSerializable(typeof(Vector3))]
[JsonSerializable(typeof(Color))]
[JsonSerializable(typeof(NodePath))]
[JsonSerializable(typeof(ResourceReference))]
[JsonSerializable(typeof(CallableReference))]
[JsonSerializable(typeof(ComponentDefinition))]
[JsonSerializable(typeof(GameObjectDefinition))]
[JsonSerializable(typeof(MaterialDefinition))]
[JsonSerializable(typeof(ProjectInfo))]
[JsonSerializable(typeof(IReadOnlyList<ComponentDefinition>))]
[JsonSerializable(typeof(IReadOnlyList<string>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization)]
public partial class McpJsonSerializerContext : JsonSerializerContext
{
}
