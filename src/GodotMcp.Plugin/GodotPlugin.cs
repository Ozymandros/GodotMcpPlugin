using System.ComponentModel;
using GodotMcp.Plugin.Extensions;
using GodotMcp.Plugin.Validation;

namespace GodotMcp.Plugin;

/// <summary>
/// Semantic Kernel plugin for godot-mcp integration that enables Semantic Kernel applications
/// to invoke Godot engine functionalities through the Model Context Protocol (MCP).
/// </summary>
/// <remarks>
/// <para>
/// The GodotPlugin acts as a bridge between Semantic Kernel and Godot Editor, allowing AI agents
/// to interact with Godot environments through MCP tools. The plugin automatically discovers
/// available Godot functions and registers them as Semantic Kernel functions.
/// </para>
/// <para>
/// The plugin requires the godot-mcp process to be available and configured. It manages the
/// connection lifecycle, parameter conversion, and error handling automatically.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage with Dependency Injection:</strong></para>
/// <code>
/// // Configure services
/// var services = new ServiceCollection();
/// services.AddLogging();
/// services.AddGodotMcp(options =>
/// {
///     options.ExecutablePath = "godot-mcp";
///     options.ConnectionTimeoutSeconds = 30;
///     options.RequestTimeoutSeconds = 60;
/// });
/// 
/// var serviceProvider = services.BuildServiceProvider();
/// 
/// // Get the plugin and initialize
/// var plugin = serviceProvider.GetRequiredService&lt;GodotPlugin&gt;();
/// await plugin.InitializeAsync();
/// 
/// // Invoke a Godot tool
/// var result = await plugin.InvokeToolAsync(
///     "Godot_create_scene",
///     new Dictionary&lt;string, object?&gt;
///     {
///         ["sceneName"] = "MyNewScene",
///         ["addToHierarchy"] = true
///     });
/// </code>
/// 
/// <para><strong>Using with Semantic Kernel:</strong></para>
/// <code>
/// // Create a kernel with Godot functions registered
/// var kernel = await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider);
/// 
/// // Use Godot functions in prompts
/// var result = await kernel.InvokePromptAsync(
///     "Create a new Godot scene called {{$sceneName}} and add a cube at position (0, 1, 0)",
///     new KernelArguments
///     {
///         ["sceneName"] = "DemoScene"
///     });
/// </code>
/// 
/// <para><strong>Manual Tool Invocation:</strong></para>
/// <code>
/// // Invoke specific Godot tools directly
/// var createResult = await plugin.InvokeToolAsync(
///     "Godot_create_gameobject",
///     new Dictionary&lt;string, object?&gt;
///     {
///         ["name"] = "Player",
///         ["position"] = new { x = 0, y = 0, z = 0 },
///         ["components"] = new[] { "Rigidbody", "BoxCollider" }
///     });
/// 
/// var sceneInfo = await plugin.InvokeToolAsync(
///     "Godot_get_scene_info",
///     new Dictionary&lt;string, object?&gt;());
/// </code>
/// 
/// <para><strong>Error Handling:</strong></para>
/// <code>
/// try
/// {
///     await plugin.InvokeToolAsync("Godot_invalid_tool", new Dictionary&lt;string, object?&gt;());
/// }
/// catch (GodotMcpException ex)
/// {
///     Console.WriteLine($"Godot MCP error: {ex.Message}");
/// }
/// catch (NetworkException ex)
/// {
///     Console.WriteLine($"Connection error: {ex.Message}");
/// }
/// catch (TimeoutException ex)
/// {
///     Console.WriteLine($"Operation timed out: {ex.Message}");
/// }
/// </code>
/// </example>
public sealed partial class GodotPlugin(
    IMcpClient mcpClient,
    IFunctionMapper functionMapper,
    IParameterConverter parameterConverter,
    ILogger<GodotPlugin> logger)
{
    private const string Version = "1.1.1";

    private readonly IMcpClient _mcpClient = mcpClient;
    private readonly IFunctionMapper _functionMapper = functionMapper;
    private readonly IParameterConverter _parameterConverter = parameterConverter;
    private readonly ILogger<GodotPlugin> _logger = logger;

    /// <summary>
    /// Initializes the plugin by discovering available tools from the godot-mcp server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous initialization operation</returns>
    /// <remarks>
    /// This method must be called before invoking any Godot tools. It establishes a connection
    /// to the godot-mcp process, discovers all available tools, and registers them with the
    /// function mapper for later invocation.
    /// </remarks>
    /// <exception cref="NetworkException">Thrown when connection to the godot-mcp server fails</exception>
    /// <exception cref="GodotMcp.Core.Exceptions.TimeoutException">Thrown when the connection or discovery operation times out</exception>
    /// <example>
    /// <code>
    /// var plugin = serviceProvider.GetRequiredService&lt;GodotPlugin&gt;();
    /// await plugin.InitializeAsync();
    /// Console.WriteLine("Plugin initialized and ready to use");
    /// </code>
    /// </example>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        LogInitializing();

        await _mcpClient.ConnectAsync(cancellationToken);
        
        var tools = await _mcpClient.ListToolsAsync(cancellationToken);
        await _functionMapper.RegisterToolsAsync(tools, cancellationToken);

        LogInitialized(tools.Count);
    }

    /// <summary>
    /// Invokes a Godot tool through the MCP protocol with automatic parameter conversion and validation.
    /// </summary>
    /// <param name="toolName">The name of the Godot tool to invoke</param>
    /// <param name="parameters">The parameters for the tool</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation with the tool result</returns>
    /// <remarks>
    /// <para>
    /// This method validates the tool name and parameters before invoking the tool. Parameters are
    /// automatically converted to the appropriate MCP format. The method includes retry logic for
    /// transient failures and comprehensive error handling.
    /// </para>
    /// <para>
    /// Tool names and available parameters are discovered during initialization. Use the
    /// InitializeAsync method before calling this method.
    /// </para>
    /// </remarks>
    /// <exception cref="GodotMcpException">Thrown when the tool is not found or validation fails</exception>
    /// <exception cref="NetworkException">Thrown when communication with the godot-mcp server fails</exception>
    /// <exception cref="GodotMcp.Core.Exceptions.TimeoutException">Thrown when the operation times out</exception>
    /// <exception cref="McpServerException">Thrown when the godot-mcp server returns an error</exception>
    /// <example>
    /// <para><strong>Creating a GameObject:</strong></para>
    /// <code>
    /// var result = await plugin.InvokeToolAsync(
    ///     "Godot_create_gameobject",
    ///     new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["name"] = "Player",
    ///         ["position"] = new { x = 0, y = 1, z = 0 },
    ///         ["rotation"] = new { x = 0, y = 0, z = 0 },
    ///         ["scale"] = new { x = 1, y = 1, z = 1 }
    ///     });
    /// </code>
    /// 
    /// <para><strong>Creating a Scene:</strong></para>
    /// <code>
    /// var sceneResult = await plugin.InvokeToolAsync(
    ///     "Godot_create_scene",
    ///     new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["sceneName"] = "Level1",
    ///         ["addToHierarchy"] = true
    ///     });
    /// </code>
    /// 
    /// <para><strong>Querying Scene Information:</strong></para>
    /// <code>
    /// var sceneInfo = await plugin.InvokeToolAsync(
    ///     "Godot_get_scene_info",
    ///     new Dictionary&lt;string, object?&gt;());
    /// Console.WriteLine($"Active scene: {sceneInfo}");
    /// </code>
    /// </example>
    [KernelFunction("invoke_godot_tool")]
    [Description("Invokes a Godot tool through the MCP protocol")]
    public async Task<object?> InvokeToolAsync(
        [Description("The name of the Godot tool to invoke")] string toolName,
        [Description("The parameters for the tool")] Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        // Validate tool name against registered tools
        var registeredTools = _functionMapper.GetRegisteredToolNames().ToHashSet(StringComparer.Ordinal);
        var normalizedToolName = ResolveRegisteredToolName(toolName, registeredTools);
        InputValidator.ValidateToolName(normalizedToolName, registeredTools);

        var toolDefinition = _functionMapper.GetToolDefinition(normalizedToolName)
            ?? throw new GodotMcpException($"Tool not found: {normalizedToolName}");

        // Validate parameters against tool definition
        InputValidator.ValidateParameters(parameters, toolDefinition);

        LogInvokingTool(normalizedToolName);

        try
        {
            var mcpParameters = _parameterConverter.ConvertToMcp(parameters, toolDefinition);
            
            var response = await _mcpClient.InvokeToolAsync(normalizedToolName, mcpParameters, cancellationToken);
            
            return response.Result;
        }
        catch (Exception ex) when (ex is not GodotMcpException)
        {
            // Sanitize error message to prevent information disclosure
            var sanitizedMessage = InputValidator.SanitizeErrorMessage(ex.Message);
            LogToolInvocationFailed(normalizedToolName, sanitizedMessage);
            throw new GodotMcpException(sanitizedMessage, ex);
        }
    }

    /// <summary>
    /// Validates whether the provided path looks like a Godot project root.
    /// </summary>
    /// <param name="projectPath">Path to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if <c>project.godot</c> exists at the specified path, otherwise false.</returns>
    [KernelFunction("validate_godot_project")]
    [Description("Checks if a directory contains a valid Godot project (project.godot).")]
    public Task<bool> ValidateGodotProjectAsync(
        [Description("Absolute or relative path to a Godot project directory.")] string projectPath,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            return Task.FromResult(false);
        }

        var settingsFilePath = Path.Combine(projectPath, "project.godot");
        return Task.FromResult(File.Exists(settingsFilePath));
    }

    /// <summary>
    /// Gets Godot/server version information through the MCP server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Version payload returned by the <c>get_godot_version</c> tool.</returns>
    [KernelFunction("get_godot_version")]
    [Description("Gets Godot and MCP server version information from the connected server.")]
    public async Task<object?> GetGodotVersionAsync(CancellationToken cancellationToken = default)
    {
        if (_mcpClient.State == ConnectionState.Disconnected)
        {
            await _mcpClient.ConnectAsync(cancellationToken);
        }

        var response = await _mcpClient.InvokeToolAsync("get_godot_version", new Dictionary<string, object?>(), cancellationToken);
        return response.Result;
    }

    /// <summary>
    /// Creates a Semantic Kernel instance with all Godot functions automatically registered
    /// as individual kernel functions under a single "Godot" plugin.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies from</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation with the configured Kernel</returns>
    /// <remarks>
    /// <para>
    /// Each discovered MCP tool is registered as its own <see cref="KernelFunction"/> with
    /// per-parameter metadata derived from the tool definition. This allows LLMs to discover
    /// and invoke tools individually (e.g. <c>Godot_create_scene(sceneName, addToHierarchy)</c>)
    /// rather than through a single generic router.
    /// </para>
    /// <para>
    /// For more control over registration, use <see cref="GodotMcp.Plugin.Extensions.GodotMcpKernelExtensions.RegisterGodotTools(Microsoft.SemanticKernel.Kernel,GodotMcp.Plugin.GodotPlugin,GodotMcp.Core.Interfaces.IFunctionMapper,string)"/>
    /// on an existing kernel instance.
    /// </para>
    /// </remarks>
    /// <exception cref="NetworkException">Thrown when connection to the godot-mcp server fails</exception>
    /// <exception cref="GodotMcp.Core.Exceptions.TimeoutException">Thrown when initialization times out</exception>
    /// <example>
    /// <code>
    /// var kernel = await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider);
    /// 
    /// // Each tool is now a distinct function the LLM can call
    /// var func = kernel.Plugins["Godot"]["Godot_create_scene"];
    /// var result = await kernel.InvokeAsync(func, new KernelArguments
    /// {
    ///     ["sceneName"] = "TestScene",
    ///     ["addToHierarchy"] = true
    /// });
    /// </code>
    /// </example>
    public static async Task<Kernel> CreateKernelWithGodotAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var plugin = serviceProvider.GetRequiredService<GodotPlugin>();
        await plugin.InitializeAsync(cancellationToken);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
        var kernel = kernelBuilder.Build();

        kernel.RegisterGodotTools(plugin, plugin._functionMapper, "godot");

        return kernel;
    }

    // LoggerMessage source generator methods for high-performance structured logging
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Initializing Godot plugin")]
    private partial void LogInitializing();

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Godot plugin initialized with {ToolCount} tools")]
    private partial void LogInitialized(int toolCount);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "Invoking Godot tool: {ToolName}")]
    private partial void LogInvokingTool(string toolName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Tool invocation failed for {ToolName}: {ErrorMessage}")]
    private partial void LogToolInvocationFailed(string toolName, string errorMessage);

    private static string ResolveRegisteredToolName(string toolName, ISet<string> registeredToolNames)
    {
        const string prefix = "godot_";
        if (registeredToolNames.Contains(toolName))
        {
            return toolName;
        }

        if (toolName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = toolName[prefix.Length..];
            if (registeredToolNames.Contains(trimmed))
            {
                return trimmed;
            }
        }

        return toolName;
    }
}
