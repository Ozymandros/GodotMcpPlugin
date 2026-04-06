using System.Collections;
using System.Collections.Frozen;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GodotMcp.Infrastructure.Conversion;

/// <summary>
/// Converts between C# types and MCP parameter formats
/// </summary>
public sealed partial class ParameterConverter : IParameterConverter
{
    private FrozenDictionary<Type, object> _converters;
    private readonly ILogger<ParameterConverter> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterConverter"/> class
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic output</param>
    public ParameterConverter(ILogger<ParameterConverter> logger)
    {
        _logger = logger;

        // Configure JsonSerializerOptions with source generator context for complex type serialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                McpJsonSerializerContext.Default,
                new DefaultJsonTypeInfoResolver())
        };

        // Initialize with empty frozen dictionary (immutable, high-performance)
        _converters = FrozenDictionary<Type, object>.Empty;
    }

    /// <summary>
    /// Converts C# parameters to MCP format
    /// </summary>
    public IReadOnlyDictionary<string, object?> ConvertToMcp(
        IReadOnlyDictionary<string, object?> parameters,
        McpToolDefinition toolDefinition)
    {
        var converted = new Dictionary<string, object?>();

        foreach (var (key, value) in parameters)
        {
            if (value == null)
            {
                converted[key] = null;
                continue;
            }

            var valueType = value.GetType();

            // Check for custom converter
            if (_converters.TryGetValue(valueType, out var converter))
            {
                var converterType = typeof(ITypeConverter<>).MakeGenericType(valueType);
                var toMcpMethod = converterType.GetMethod("ToMcp");
                converted[key] = toMcpMethod?.Invoke(converter, [value]);

                LogCustomConverterUsed(key, valueType.Name);
            }
            // Handle primitives
            else if (IsPrimitive(valueType))
            {
                converted[key] = value;
            }
            // Handle collections
            else if (value is IEnumerable enumerable and not string)
            {
                converted[key] = ConvertCollection(enumerable);
            }
            // Handle complex objects - serialize to JSON
            else
            {
                converted[key] = JsonSerializer.SerializeToElement(value, _jsonOptions);
                LogComplexObjectSerialized(key, valueType.Name);
            }
        }

        LogParametersConverted(parameters.Count, converted.Count);

        return converted;
    }

    /// <summary>
    /// Converts MCP response to C# type
    /// </summary>
    public T? ConvertFromMcp<T>(McpResponse response)
    {
        if (response.Result == null)
        {
            return default;
        }

        var targetType = typeof(T);

        try
        {
            // Check for custom converter
            if (_converters.TryGetValue(targetType, out var converter))
            {
                var converterType = typeof(ITypeConverter<>).MakeGenericType(targetType);
                var fromMcpMethod = converterType.GetMethod("FromMcp");
                var result = (T?)fromMcpMethod?.Invoke(converter, [response.Result]);

                LogCustomConverterUsed("response", targetType.Name);

                return result;
            }

            // Handle JsonElement from System.Text.Json
            if (response.Result is JsonElement element)
            {
                var result = element.Deserialize<T>(_jsonOptions);
                LogResponseDeserialized(targetType.Name);
                return result;
            }

            // Direct cast for primitives
            if (IsPrimitive(targetType) && response.Result.GetType() == targetType)
            {
                return (T)response.Result;
            }

            // Serialize then deserialize for complex types using source generators
            var json = JsonSerializer.Serialize(response.Result, _jsonOptions);
            var deserializedResult = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            LogResponseDeserialized(targetType.Name);

            return deserializedResult;
        }
        catch (Exception ex)
        {
            LogConversionFailed(targetType.Name, ex);

            throw new TypeConversionException(
                $"Failed to convert MCP response to type {targetType.Name}",
                response.Result?.GetType(),
                targetType);
        }
    }

    /// <summary>
    /// Registers a custom type converter
    /// </summary>
    public void RegisterConverter<T>(ITypeConverter<T> converter)
    {
        // Create a new dictionary with the additional converter
        var newConverters = _converters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        newConverters[typeof(T)] = converter;

        // Replace with new frozen dictionary (immutable, high-performance)
        _converters = newConverters.ToFrozenDictionary();

        LogConverterRegistered(typeof(T).Name);
    }

    /// <summary>
    /// Checks if a type is a primitive type
    /// </summary>
    private static bool IsPrimitive(Type type)
    {
        return type.IsPrimitive
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid);
    }

    /// <summary>
    /// Converts a collection to MCP format using collection expressions and LINQ
    /// </summary>
    private object ConvertCollection(IEnumerable enumerable)
    {
        var list = new List<object?>();

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                list.Add(null);
            }
            else if (IsPrimitive(item.GetType()))
            {
                list.Add(item);
            }
            else
            {
                list.Add(JsonSerializer.SerializeToElement(item, _jsonOptions));
            }
        }

        return list;
    }

    // LoggerMessage source generator methods for structured logging
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Using custom converter for parameter '{ParameterName}' of type {TypeName}")]
    partial void LogCustomConverterUsed(string parameterName, string typeName);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message = "Serialized complex object for parameter '{ParameterName}' of type {TypeName}")]
    partial void LogComplexObjectSerialized(string parameterName, string typeName);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "Converted {InputCount} parameters to {OutputCount} MCP parameters")]
    partial void LogParametersConverted(int inputCount, int outputCount);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "Deserialized MCP response to type {TypeName}")]
    partial void LogResponseDeserialized(string typeName);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Failed to convert MCP response to type {TypeName}")]
    partial void LogConversionFailed(string typeName, Exception ex);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Information,
        Message = "Registered custom converter for type {TypeName}")]
    partial void LogConverterRegistered(string typeName);
}
