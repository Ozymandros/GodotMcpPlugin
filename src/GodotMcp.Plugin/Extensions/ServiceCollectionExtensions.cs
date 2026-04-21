using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Client;
using GodotMcp.Infrastructure.Configuration;
using GodotMcp.Infrastructure.Conversion;
using GodotMcp.Plugin.Mapping;
using GodotMcp.Plugin.Skills;

namespace GodotMcp.Plugin.Extensions;

/// <summary>
/// Extension methods for registering Godot MCP services with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Godot MCP plugin services to the service collection using configuration
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">The configuration containing Godot MCP settings</param>
    /// <param name="serviceKey">Optional service key for keyed services registration (supports multiple plugin instances)</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// This method registers all required services for the Godot MCP plugin:
    /// - Configuration options with validation
    /// - MCP stdio client (ModelContextProtocol SDK) for godot-mcp lifecycle
    /// - Parameter converter for type conversion
    /// - Function mapper for MCP tool to SK function mapping
    /// - MCP client for stdio communication
    /// - Godot plugin for Semantic Kernel integration
    ///
    /// Configuration is read from the "GodotMcp" section by default.
    /// Use the serviceKey parameter to register multiple plugin instances with different configurations.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddGodotMcp(configuration);
    /// // Or with keyed services for multiple instances:
    /// services.AddGodotMcp(configuration, serviceKey: "Godot-editor-1");
    /// services.AddGodotMcp(configuration, serviceKey: "Godot-editor-2");
    /// </code>
    /// </example>
    public static IServiceCollection AddGodotMcp(
        this IServiceCollection services,
        IConfiguration configuration,
        object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register configuration with validation
        var configSection = configuration.GetSection(GodotMcpOptions.SectionName);
        services.Configure<GodotMcpOptions>(options => configSection.Bind(options));

        // Enable validation on start
        services.AddOptions<GodotMcpOptions>().ValidateOnStart();

        // Register validator for configuration validation
        services.AddSingleton<IValidateOptions<GodotMcpOptions>, GodotMcpOptionsValidator>();

        // Register services based on whether keyed services are used
        if (serviceKey is null)
        {
            // Standard singleton registration
            RegisterServices(services);
        }
        else
        {
            // Keyed services registration for multiple plugin instances
            RegisterKeyedServices(services, serviceKey);
        }

        return services;
    }

    /// <summary>
    /// Adds Godot MCP plugin services to the service collection using inline configuration
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configureOptions">Action to configure Godot MCP options</param>
    /// <param name="serviceKey">Optional service key for keyed services registration (supports multiple plugin instances)</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// This overload allows inline configuration without requiring IConfiguration.
    /// All services are registered as singletons with appropriate lifetimes.
    /// Use the serviceKey parameter to register multiple plugin instances with different configurations.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddGodotMcp(options =>
    /// {
    ///     options.ExecutablePath = "godot-mcp";
    ///     options.ConnectionTimeoutSeconds = 30;
    ///     options.RequestTimeoutSeconds = 60;
    /// });
    /// // Or with keyed services:
    /// services.AddGodotMcp(options => { /* config */ }, serviceKey: "Godot-editor-1");
    /// </code>
    /// </example>
    public static IServiceCollection AddGodotMcp(
        this IServiceCollection services,
        Action<GodotMcpOptions> configureOptions,
        object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Register configuration with validation using OptionsBuilder pattern
        services.AddOptions<GodotMcpOptions>()
            .Configure(configureOptions)
            .ValidateOnStart();

        // Register validator for configuration validation
        services.AddSingleton<IValidateOptions<GodotMcpOptions>, GodotMcpOptionsValidator>();

        // Register services based on whether keyed services are used
        if (serviceKey is null)
        {
            // Standard singleton registration
            RegisterServices(services);
        }
        else
        {
            // Keyed services registration for multiple plugin instances
            RegisterKeyedServices(services, serviceKey);
        }

        return services;
    }

    /// <summary>
    /// Registers all Godot MCP services as standard singletons
    /// </summary>
    private static void RegisterServices(IServiceCollection services)
    {
        // Always ensure logging is registered (AddLogging is idempotent)
        services.AddLogging();

        // Register IParameterConverter as singleton with default Godot converters
        services.AddSingleton<IParameterConverter>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ParameterConverter>>();
            var converter = new ParameterConverter(logger);
            GodotTypeConverter.RegisterDefaults(converter);
            return converter;
        });

        // Register IFunctionMapper as singleton with FunctionMapper implementation
        services.AddSingleton<IFunctionMapper, FunctionMapper>();

        // Official MCP stdio client (GodotMCP.Server 1.2+ / tools/call)
        services.AddSingleton<IMcpProtocolClientFactory, McpSdkProtocolClientFactory>();
        services.AddSingleton<IMcpClient, StdioMcpClient>();

        // Register GodotPlugin as singleton
        services.AddSingleton<GodotPlugin>();

        // Register typed skill modules
        services.AddSingleton<SceneSkill>();
        services.AddSingleton<ProjectSkill>();
        services.AddSingleton<ResourceSkill>();
        services.AddSingleton<ScriptSkill>();
        services.AddSingleton<ImportSkill>();
        services.AddSingleton<CameraSkill>();
        services.AddSingleton<UiSkill>();
        services.AddSingleton<LightingSkill>();
        services.AddSingleton<PhysicsSkill>();
        services.AddSingleton<NavigationSkill>();
        services.AddSingleton<AdvancedLintSkill>();
        services.AddSingleton<PresetSkill>();
        services.AddSingleton<DocumentationSkill>();
    }

    /// <summary>
    /// Registers all Godot MCP services as keyed singletons for multiple plugin instances support
    /// </summary>
    /// <remarks>
    /// Keyed services are a .NET 10 feature that allows multiple instances of the same service type
    /// to be registered with different keys. This enables scenarios where multiple Godot editors
    /// need to be controlled simultaneously.
    /// </remarks>
    private static void RegisterKeyedServices(IServiceCollection services, object serviceKey)
    {
        // Always ensure logging is registered (AddLogging is idempotent)
        services.AddLogging();

        // Register IParameterConverter as keyed singleton with factory
        services.AddKeyedSingleton<IParameterConverter>(serviceKey, (sp, key) =>
        {
            var logger = sp.GetRequiredService<ILogger<ParameterConverter>>();
            var converter = new ParameterConverter(logger);
            GodotTypeConverter.RegisterDefaults(converter);
            return converter;
        });

        // Register IFunctionMapper as keyed singleton with factory
        services.AddKeyedSingleton<IFunctionMapper>(serviceKey, (sp, key) =>
        {
            var logger = sp.GetRequiredService<ILogger<FunctionMapper>>();
            return new FunctionMapper(logger);
        });

        services.AddKeyedSingleton<IMcpProtocolClientFactory>(serviceKey, (_, _) => new McpSdkProtocolClientFactory());
        services.AddKeyedSingleton<IMcpClient>(serviceKey, (sp, key) =>
        {
            var factory = sp.GetRequiredKeyedService<IMcpProtocolClientFactory>(key);
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = sp.GetRequiredService<ILogger<StdioMcpClient>>();
            var options = sp.GetRequiredService<IOptions<GodotMcpOptions>>();
            return new StdioMcpClient(factory, loggerFactory, logger, options);
        });

        // Register GodotPlugin as keyed singleton with factory that resolves keyed dependencies
        services.AddKeyedSingleton<GodotPlugin>(serviceKey, (sp, key) =>
        {
            var mcpClient = sp.GetRequiredKeyedService<IMcpClient>(key);
            var functionMapper = sp.GetRequiredKeyedService<IFunctionMapper>(key);
            var parameterConverter = sp.GetRequiredKeyedService<IParameterConverter>(key);
            var logger = sp.GetRequiredService<ILogger<GodotPlugin>>();
            return new GodotPlugin(mcpClient, functionMapper, parameterConverter, logger);
        });

        services.AddKeyedSingleton<SceneSkill>(serviceKey, (sp, key) =>
            new SceneSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<ProjectSkill>(serviceKey, (sp, key) =>
            new ProjectSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<ResourceSkill>(serviceKey, (sp, key) =>
            new ResourceSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<ScriptSkill>(serviceKey, (sp, key) =>
            new ScriptSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<ImportSkill>(serviceKey, (sp, key) =>
            new ImportSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<CameraSkill>(serviceKey, (sp, key) =>
            new CameraSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<UiSkill>(serviceKey, (sp, key) =>
            new UiSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<LightingSkill>(serviceKey, (sp, key) =>
            new LightingSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<PhysicsSkill>(serviceKey, (sp, key) =>
            new PhysicsSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<NavigationSkill>(serviceKey, (sp, key) =>
            new NavigationSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<AdvancedLintSkill>(serviceKey, (sp, key) =>
            new AdvancedLintSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<PresetSkill>(serviceKey, (sp, key) =>
            new PresetSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
        services.AddKeyedSingleton<DocumentationSkill>(serviceKey, (sp, key) =>
            new DocumentationSkill(sp.GetRequiredKeyedService<IMcpClient>(key)));
    }
}
