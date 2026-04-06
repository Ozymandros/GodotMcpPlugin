using Microsoft.SemanticKernel;

namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Maps MCP tools to Semantic Kernel functions
/// </summary>
public interface IFunctionMapper
{
    /// <summary>
    /// Creates SK function metadata from MCP tool definition
    /// </summary>
    /// <param name="toolDefinition">The MCP tool definition to map</param>
    /// <returns>Kernel function metadata for the tool</returns>
    KernelFunctionMetadata MapToKernelFunction(McpToolDefinition toolDefinition);

    /// <summary>
    /// Gets the MCP tool definition for a function name
    /// </summary>
    /// <param name="functionName">The name of the function to look up</param>
    /// <returns>The tool definition if found, otherwise null</returns>
    McpToolDefinition? GetToolDefinition(string functionName);

    /// <summary>
    /// Gets the names of all registered tools
    /// </summary>
    /// <returns>A collection of registered tool names</returns>
    IEnumerable<string> GetRegisteredToolNames();

    /// <summary>
    /// Gets all registered tool definitions, sorted by name for deterministic ordering
    /// </summary>
    /// <returns>A read-only list of all registered tool definitions</returns>
    IReadOnlyList<McpToolDefinition> GetRegisteredTools();

    /// <summary>
    /// Registers all discovered tools
    /// </summary>
    /// <param name="tools">The list of tools to register</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RegisterToolsAsync(
        IReadOnlyList<McpToolDefinition> tools,
        CancellationToken cancellationToken = default);
}
