using Microsoft.SemanticKernel;
using GodotMcp.Core.Interfaces;

namespace GodotMcp.Plugin.Extensions;

/// <summary>
/// Extension methods for registering discovered Godot MCP tools on a Semantic Kernel instance.
/// Each MCP tool becomes a first-class <see cref="KernelFunction"/> with parameter metadata
/// that LLMs can discover and invoke individually.
/// </summary>
public static class GodotMcpKernelExtensions
{
    /// <summary>
    /// Registers all discovered Godot MCP tools as individual kernel functions under a single plugin.
    /// </summary>
    /// <param name="kernel">The kernel to register tools on</param>
    /// <param name="plugin">An initialized <see cref="GodotPlugin"/> instance</param>
    /// <param name="functionMapper">The function mapper containing registered tool definitions</param>
    /// <param name="pluginName">The plugin name under which all tools are grouped (default: "Godot")</param>
    /// <returns>The registered <see cref="KernelPlugin"/> containing all tool functions</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no tools are registered. Call <see cref="GodotPlugin.InitializeAsync"/> first.
    /// </exception>
    public static KernelPlugin RegisterGodotTools(
        this Kernel kernel,
        GodotPlugin plugin,
        IFunctionMapper functionMapper,
        string pluginName = "godot")
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(functionMapper);

        var tools = functionMapper.GetRegisteredTools();
        if (tools.Count == 0)
        {
            throw new InvalidOperationException(
                "No tools registered. Call GodotPlugin.InitializeAsync before registering tools on a kernel.");
        }

        var functions = new List<KernelFunction>(tools.Count);
        foreach (var tool in tools)
        {
            var metadata = functionMapper.MapToKernelFunction(tool);
            var capturedToolName = tool.Name;

            var functionName = tool.Name.StartsWith("godot_", StringComparison.OrdinalIgnoreCase)
                ? $"godot_{tool.Name[6..]}"
                : $"godot_{tool.Name}";

            var function = KernelFunctionFactory.CreateFromMethod(
                new Func<KernelArguments, CancellationToken, Task<object?>>(
                    async (args, ct) =>
                    {
                        var parameters = args.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value);
                        return await plugin.InvokeToolAsync(capturedToolName, parameters, ct);
                    }),
                new KernelFunctionFromMethodOptions
                {
                    FunctionName = functionName,
                    Description = tool.Description,
                    Parameters = metadata.Parameters,
                    ReturnParameter = metadata.ReturnParameter
                });

            functions.Add(function);
        }

        return kernel.Plugins.AddFromFunctions(pluginName, functions);
    }

    /// <summary>
    /// Registers all discovered Godot MCP tools by resolving services from the provider.
    /// Convenience overload that resolves <see cref="GodotPlugin"/> and <see cref="IFunctionMapper"/>
    /// from dependency injection.
    /// </summary>
    /// <param name="kernel">The kernel to register tools on</param>
    /// <param name="serviceProvider">The service provider to resolve dependencies from</param>
    /// <param name="pluginName">The plugin name under which all tools are grouped (default: "Godot")</param>
    /// <returns>The registered <see cref="KernelPlugin"/> containing all tool functions</returns>
    public static KernelPlugin RegisterGodotTools(
        this Kernel kernel,
        IServiceProvider serviceProvider,
        string pluginName = "godot")
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var plugin = serviceProvider.GetRequiredService<GodotPlugin>();
        var functionMapper = serviceProvider.GetRequiredService<IFunctionMapper>();
        return kernel.RegisterGodotTools(plugin, functionMapper, pluginName);
    }
}
