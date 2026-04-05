using System.Text.Json;
using GodotMcp.Core.Models;
using ModelContextProtocol.Client;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Maps MCP SDK tool metadata to <see cref="McpToolDefinition"/> used by Semantic Kernel registration.
/// </summary>
internal static class GodotMcpToolDefinitionMapper
{
    public static McpToolDefinition FromClientTool(McpClientTool tool)
    {
        var parameters = ParseInputSchema(tool.JsonSchema);
        McpReturnType? ret = null;
        if (tool.ReturnJsonSchema is { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null })
        {
            ret = new McpReturnType("object", "Tool result");
        }

        return new McpToolDefinition(
            tool.Name,
            tool.Description ?? string.Empty,
            parameters,
            ret);
    }

    private static IReadOnlyDictionary<string, McpParameterDefinition> ParseInputSchema(JsonElement schemaElement)
    {
        if (schemaElement.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new Dictionary<string, McpParameterDefinition>(StringComparer.Ordinal);
        }

        if (!schemaElement.TryGetProperty("properties", out var propsElement))
        {
            return new Dictionary<string, McpParameterDefinition>(StringComparer.Ordinal);
        }

        var parameters = new Dictionary<string, McpParameterDefinition>(StringComparer.Ordinal);

        foreach (var prop in propsElement.EnumerateObject())
        {
            var paramName = prop.Name;
            var paramType = prop.Value.TryGetProperty("type", out var typeProp)
                ? typeProp.GetString() ?? "string"
                : "string";
            var paramDesc = prop.Value.TryGetProperty("description", out var paramDescProp)
                ? paramDescProp.GetString()
                : null;

            var required = false;
            if (schemaElement.TryGetProperty("required", out var requiredElement))
            {
                foreach (var reqName in requiredElement.EnumerateArray())
                {
                    if (reqName.GetString() == paramName)
                    {
                        required = true;
                        break;
                    }
                }
            }

            object? defaultValue = null;
            if (prop.Value.TryGetProperty("default", out var defaultProp))
            {
                defaultValue = defaultProp.ValueKind switch
                {
                    JsonValueKind.String => defaultProp.GetString(),
                    JsonValueKind.Number => defaultProp.TryGetInt32(out var i) ? i : defaultProp.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => defaultProp.GetRawText()
                };
            }

            parameters[paramName] = new McpParameterDefinition(
                paramName,
                paramType,
                paramDesc,
                required,
                defaultValue);
        }

        return parameters;
    }
}
