using System.Text.Json;

namespace GodotMcp.Infrastructure.Conversion;

/// <summary>
/// Registers default converters for Godot-specific value objects.
/// </summary>
public static class GodotTypeConverter
{
    /// <summary>
    /// Registers the built-in Godot converters on the provided parameter converter.
    /// </summary>
    public static void RegisterDefaults(IParameterConverter parameterConverter)
    {
        ArgumentNullException.ThrowIfNull(parameterConverter);

        parameterConverter.RegisterConverter(new NodePathConverter());
        parameterConverter.RegisterConverter(new ResourceReferenceConverter());
        parameterConverter.RegisterConverter(new CallableReferenceConverter());
    }

    private sealed class NodePathConverter : ITypeConverter<NodePath>
    {
        public object? ToMcp(NodePath value) => value.Value;

        public NodePath FromMcp(object? mcpValue)
            => mcpValue is string value ? new NodePath(value) : default;
    }

    private sealed class ResourceReferenceConverter : ITypeConverter<ResourceReference>
    {
        public object? ToMcp(ResourceReference value) => new Dictionary<string, object?>
        {
            ["path"] = value.Path
        };

        public ResourceReference FromMcp(object? mcpValue)
        {
            if (mcpValue is JsonElement element && element.TryGetProperty("path", out var pathElement))
            {
                return new ResourceReference(pathElement.GetString() ?? string.Empty);
            }

            if (mcpValue is IReadOnlyDictionary<string, object?> dict
                && dict.TryGetValue("path", out var pathObj)
                && pathObj is string path)
            {
                return new ResourceReference(path);
            }

            return default;
        }
    }

    private sealed class CallableReferenceConverter : ITypeConverter<CallableReference>
    {
        public object? ToMcp(CallableReference value)
            => string.IsNullOrWhiteSpace(value.Target)
                ? value.Method
                : new Dictionary<string, object?>
                {
                    ["method"] = value.Method,
                    ["object"] = value.Target
                };

        public CallableReference FromMcp(object? mcpValue)
        {
            if (mcpValue is string method)
            {
                return new CallableReference(method);
            }

            if (mcpValue is JsonElement element
                && element.TryGetProperty("method", out var methodElement))
            {
                var parsedMethod = methodElement.GetString() ?? string.Empty;
                var target = element.TryGetProperty("object", out var targetElement)
                    ? targetElement.GetString()
                    : null;
                return new CallableReference(parsedMethod, target);
            }

            return default;
        }
    }
}
