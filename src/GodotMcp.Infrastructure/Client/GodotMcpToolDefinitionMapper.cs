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
        var requiredNames = ParseRequiredNames(schemaElement);

        foreach (var prop in propsElement.EnumerateObject())
        {
            var paramName = prop.Name;
            var paramType = ParseParameterType(prop.Value);
            var paramDesc = prop.Value.TryGetProperty("description", out var paramDescProp)
                ? paramDescProp.GetString()
                : null;
            var required = requiredNames.Contains(paramName);

            object? defaultValue = null;
            if (prop.Value.TryGetProperty("default", out var defaultProp))
            {
                defaultValue = ParseJsonValue(defaultProp);
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

    private static HashSet<string> ParseRequiredNames(JsonElement schemaElement)
    {
        var requiredNames = new HashSet<string>(StringComparer.Ordinal);

        if (!schemaElement.TryGetProperty("required", out var requiredElement)
            || requiredElement.ValueKind != JsonValueKind.Array)
        {
            return requiredNames;
        }

        foreach (var reqName in requiredElement.EnumerateArray())
        {
            AddRequiredNameCandidate(requiredNames, reqName);
        }

        return requiredNames;
    }

    private static string ParseParameterType(JsonElement parameterSchema)
    {
        if (parameterSchema.TryGetProperty("type", out var typeElement))
        {
            var declaredType = ParseTypeElement(typeElement);
            if (declaredType is not null)
            {
                return declaredType;
            }
        }

        foreach (var compositionKey in new[] { "oneOf", "anyOf", "allOf" })
        {
            if (!parameterSchema.TryGetProperty(compositionKey, out var composed)
                || composed.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var option in composed.EnumerateArray())
            {
                var optionType = ParseParameterType(option);
                if (!string.Equals(optionType, "object", StringComparison.Ordinal)
                    && !string.Equals(optionType, "null", StringComparison.Ordinal))
                {
                    return optionType;
                }
            }
        }

        if (parameterSchema.TryGetProperty("enum", out var enumElement)
            && enumElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var enumValue in enumElement.EnumerateArray())
            {
                var enumType = enumValue.ValueKind switch
                {
                    JsonValueKind.String => "string",
                    JsonValueKind.Number => "number",
                    JsonValueKind.True or JsonValueKind.False => "boolean",
                    JsonValueKind.Array => "array",
                    JsonValueKind.Object => "object",
                    _ => null
                };

                if (enumType is not null)
                {
                    return enumType;
                }
            }
        }

        if (parameterSchema.TryGetProperty("items", out _))
        {
            return "array";
        }

        if (parameterSchema.TryGetProperty("properties", out _))
        {
            return "object";
        }

        return "object";
    }

    private static string? ParseTypeElement(JsonElement typeElement)
    {
        if (typeElement.ValueKind == JsonValueKind.String)
        {
            return NormalizeSchemaTypeName(typeElement.GetString());
        }

        if (typeElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in typeElement.EnumerateArray())
        {
            var normalized = NormalizeSchemaTypeName(item.GetString());
            if (normalized is not null && !string.Equals(normalized, "null", StringComparison.Ordinal))
            {
                return normalized;
            }
        }

        return null;
    }

    private static string? NormalizeSchemaTypeName(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        return type.Trim().ToLowerInvariant() switch
        {
            "int" => "integer",
            "bool" => "boolean",
            var normalized => normalized
        };
    }

    private static void AddRequiredNameCandidate(HashSet<string> requiredNames, JsonElement candidate)
    {
        // Defensive parsing: some servers emit non-standard "required" payloads where
        // entries can be arrays (e.g. ["paramA", "paramB"]) instead of plain strings.
        // We flatten arrays and only keep string entries.
        switch (candidate.ValueKind)
        {
            case JsonValueKind.String:
            {
                var name = candidate.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    requiredNames.Add(name);
                }

                break;
            }
            case JsonValueKind.Array:
                foreach (var nested in candidate.EnumerateArray())
                {
                    AddRequiredNameCandidate(requiredNames, nested);
                }

                break;
        }
    }

    private static object? ParseJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i)
                ? i
                : element.TryGetInt64(out var l)
                    ? l
                    : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Object => ParseObject(element),
            JsonValueKind.Array => ParseArray(element),
            _ => element.GetRawText()
        };
    }

    private static Dictionary<string, object?> ParseObject(JsonElement element)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ParseJsonValue(property.Value);
        }

        return result;
    }

    private static List<object?> ParseArray(JsonElement element)
    {
        var result = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            result.Add(ParseJsonValue(item));
        }

        return result;
    }
}
