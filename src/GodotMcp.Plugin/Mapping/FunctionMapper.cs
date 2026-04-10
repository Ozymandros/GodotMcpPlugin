using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Plugin.Mapping;

/// <summary>
/// Maps MCP tools to Semantic Kernel functions
/// </summary>
public sealed partial class FunctionMapper : IFunctionMapper
{
    private FrozenDictionary<string, McpToolDefinition> _toolDefinitions = FrozenDictionary<string, McpToolDefinition>.Empty;
    private readonly ILogger<FunctionMapper> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionMapper"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public FunctionMapper(ILogger<FunctionMapper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts an MCP tool definition into a Semantic Kernel function metadata.
    /// </summary>
    public KernelFunctionMetadata MapToKernelFunction(McpToolDefinition toolDefinition)
    {
        LogMappingFunction(toolDefinition.Name);

        var parameters = toolDefinition.Parameters
            .Select(p => new KernelParameterMetadata(p.Value.Name)
            {
                Description = p.Value.Description,
                IsRequired = p.Value.Required,
                DefaultValue = p.Value.DefaultValue,
                ParameterType = MapMcpTypeToClrType(p.Value.Type)
            })
            .ToList();

        var returnParameter = toolDefinition.ReturnType != null
            ? new KernelReturnParameterMetadata
            {
                Description = toolDefinition.ReturnType.Description,
                ParameterType = MapMcpTypeToClrType(toolDefinition.ReturnType.Type)
            }
            : new KernelReturnParameterMetadata(); // Use default empty return parameter instead of null

        return new KernelFunctionMetadata(toolDefinition.Name)
        {
            Description = toolDefinition.Description,
            Parameters = parameters,
            ReturnParameter = returnParameter
        };
    }

    /// <summary>
    /// Gets the MCP tool definition for the given function name, if registered.
    /// </summary>
    public McpToolDefinition? GetToolDefinition(string functionName)
    {
        return _toolDefinitions.TryGetValue(functionName, out var definition)
            ? definition
            : null;
    }

    /// <summary>
    /// Gets the names of all registered tools
    /// </summary>
    /// <returns>A collection of registered tool names</returns>
    public IEnumerable<string> GetRegisteredToolNames()
    {
        return _toolDefinitions.Keys;
    }

    /// <inheritdoc />
    public IReadOnlyList<McpToolDefinition> GetRegisteredTools()
    {
        return _toolDefinitions.Values.OrderBy(t => t.Name, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Registers a set of MCP tool definitions for mapping.
    /// </summary>
    public Task RegisterToolsAsync(
        IReadOnlyList<McpToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        var toolDictionary = tools.ToDictionary(t => t.Name, t => t);
        _toolDefinitions = toolDictionary.ToFrozenDictionary();

        LogToolsRegistered(tools.Count);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maps MCP type string to CLR type using pattern matching
    /// </summary>
    /// <param name="mcpType">The MCP type string</param>
    /// <returns>The corresponding CLR type</returns>
    private static Type MapMcpTypeToClrType(string mcpType)
    {
        if (string.IsNullOrWhiteSpace(mcpType))
        {
            return typeof(object);
        }

        var candidates = mcpType
            .Split(['|', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLowerInvariant())
            .Where(t => t != "null");

        foreach (var candidate in candidates)
        {
            switch (candidate)
            {
                case "string":
                    return typeof(string);
                case "number":
                    return typeof(double);
                case "integer":
                case "int":
                    return typeof(int);
                case "boolean":
                case "bool":
                    return typeof(bool);
                case "object":
                    return typeof(object);
                case "array":
                    return typeof(object[]);
            }
        }

        return typeof(object);
    }

    // LoggerMessage source generator methods for high-performance structured logging
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Mapping MCP tool to kernel function: {ToolName}")]
    private partial void LogMappingFunction(string toolName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Registered {ToolCount} tools")]
    private partial void LogToolsRegistered(int toolCount);
}
