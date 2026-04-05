using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Client;
using GodotMcp.Infrastructure.Configuration;
using GodotMcp.Infrastructure.Conversion;
using GodotMcp.Plugin;
using GodotMcp.Plugin.Extensions;
using GodotMcp.Plugin.Mapping;

namespace GodotMcp.Tests.PluginTests;

/// <summary>
/// Unit tests for ServiceCollectionExtensions
/// **Validates: Requirements 14.7, 7.1, 7.2, 6.8**
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region AddGodotMcp with IConfiguration Tests

    [Fact]
    public void AddGodotMcp_WithConfiguration_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp",
                ["GodotMcp:ConnectionTimeoutSeconds"] = "30",
                ["GodotMcp:RequestTimeoutSeconds"] = "60"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify all required services can be resolved
        Assert.NotNull(serviceProvider.GetService<IMcpProtocolClientFactory>());
        Assert.NotNull(serviceProvider.GetService<IParameterConverter>());
        Assert.NotNull(serviceProvider.GetService<IFunctionMapper>());
        Assert.NotNull(serviceProvider.GetService<IMcpClient>());
        Assert.NotNull(serviceProvider.GetService<GodotPlugin>());
    }

    [Fact]
    public void AddGodotMcp_WithConfiguration_ReadsSettingsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "custom-godot-mcp",
                ["GodotMcp:ConnectionTimeoutSeconds"] = "45",
                ["GodotMcp:RequestTimeoutSeconds"] = "90",
                ["GodotMcp:MaxRetryAttempts"] = "5",
                ["GodotMcp:BackoffStrategy"] = "Linear",
                ["GodotMcp:InitialRetryDelayMs"] = "2000",
                ["GodotMcp:EnableProcessPooling"] = "false",
                ["GodotMcp:MaxIdleTimeSeconds"] = "600",
                ["GodotMcp:EnableMessageLogging"] = "true"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value;

        // Assert
        Assert.Equal("custom-godot-mcp", options.ExecutablePath);
        Assert.Equal(45, options.ConnectionTimeoutSeconds);
        Assert.Equal(90, options.RequestTimeoutSeconds);
        Assert.Equal(5, options.MaxRetryAttempts);
        Assert.Equal(BackoffStrategy.Linear, options.BackoffStrategy);
        Assert.Equal(2000, options.InitialRetryDelayMs);
        Assert.False(options.EnableProcessPooling);
        Assert.Equal(600, options.MaxIdleTimeSeconds);
        Assert.True(options.EnableMessageLogging);
    }

    [Fact]
    public void AddGodotMcp_WithConfiguration_UsesDefaultValuesWhenNotSpecified()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value;

        // Assert - Verify default values
        Assert.Equal("godot-mcp", options.ExecutablePath);
        Assert.Equal(30, options.ConnectionTimeoutSeconds);
        Assert.Equal(60, options.RequestTimeoutSeconds);
        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.Equal(BackoffStrategy.Exponential, options.BackoffStrategy);
        Assert.Equal(1000, options.InitialRetryDelayMs);
        Assert.True(options.EnableProcessPooling);
        Assert.Equal(300, options.MaxIdleTimeSeconds);
        Assert.False(options.EnableMessageLogging);
    }

    [Fact]
    public void AddGodotMcp_WithConfiguration_ThrowsWhenConfigurationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddGodotMcp((IConfiguration)null!));
    }

    [Fact]
    public void AddGodotMcp_WithConfiguration_ThrowsWhenServicesIsNull()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IServiceCollection)null!).AddGodotMcp(configuration));
    }

    #endregion

    #region AddGodotMcp with Action<GodotMcpOptions> Tests

    [Fact]
    public void AddGodotMcp_WithAction_ConfiguresOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddGodotMcp(options =>
        {
            options.ExecutablePath = "action-godot-mcp";
            options.ConnectionTimeoutSeconds = 20;
            options.RequestTimeoutSeconds = 40;
            options.MaxRetryAttempts = 2;
            options.BackoffStrategy = BackoffStrategy.Linear;
            options.InitialRetryDelayMs = 500;
            options.EnableProcessPooling = false;
            options.MaxIdleTimeSeconds = 120;
            options.EnableMessageLogging = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value;

        // Assert
        Assert.Equal("action-godot-mcp", options.ExecutablePath);
        Assert.Equal(20, options.ConnectionTimeoutSeconds);
        Assert.Equal(40, options.RequestTimeoutSeconds);
        Assert.Equal(2, options.MaxRetryAttempts);
        Assert.Equal(BackoffStrategy.Linear, options.BackoffStrategy);
        Assert.Equal(500, options.InitialRetryDelayMs);
        Assert.False(options.EnableProcessPooling);
        Assert.Equal(120, options.MaxIdleTimeSeconds);
        Assert.True(options.EnableMessageLogging);
    }

    [Fact]
    public void AddGodotMcp_WithAction_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddGodotMcp(options =>
        {
            options.ExecutablePath = "godot-mcp";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify all required services can be resolved
        Assert.NotNull(serviceProvider.GetService<IMcpProtocolClientFactory>());
        Assert.NotNull(serviceProvider.GetService<IParameterConverter>());
        Assert.NotNull(serviceProvider.GetService<IFunctionMapper>());
        Assert.NotNull(serviceProvider.GetService<IMcpClient>());
        Assert.NotNull(serviceProvider.GetService<GodotPlugin>());
    }

    [Fact]
    public void AddGodotMcp_WithAction_ThrowsWhenActionIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddGodotMcp((Action<GodotMcpOptions>)null!));
    }

    [Fact]
    public void AddGodotMcp_WithAction_ThrowsWhenServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IServiceCollection)null!).AddGodotMcp(options => { }));
    }

    #endregion

    #region Service Lifetime Tests

    [Fact]
    public void AddGodotMcp_RegistersServicesAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);

        // Assert - Check service descriptors for singleton lifetime
        var protocolFactoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMcpProtocolClientFactory));
        var parameterConverterDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IParameterConverter));
        var functionMapperDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IFunctionMapper));
        var mcpClientDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMcpClient));
        var pluginDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(GodotPlugin));

        Assert.NotNull(protocolFactoryDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, protocolFactoryDescriptor.Lifetime);
        
        Assert.NotNull(parameterConverterDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, parameterConverterDescriptor.Lifetime);
        
        Assert.NotNull(functionMapperDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, functionMapperDescriptor.Lifetime);
        
        Assert.NotNull(mcpClientDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, mcpClientDescriptor.Lifetime);
        
        Assert.NotNull(pluginDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, pluginDescriptor.Lifetime);
    }

    [Fact]
    public void AddGodotMcp_ResolvesSameInstanceForMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act - Resolve services multiple times
        var factory1 = serviceProvider.GetService<IMcpProtocolClientFactory>();
        var factory2 = serviceProvider.GetService<IMcpProtocolClientFactory>();
        
        var plugin1 = serviceProvider.GetService<GodotPlugin>();
        var plugin2 = serviceProvider.GetService<GodotPlugin>();

        // Assert - Same instances should be returned (singleton behavior)
        Assert.Same(factory1, factory2);
        Assert.Same(plugin1, plugin2);
    }

    #endregion

    #region Service Resolution Tests

    [Fact]
    public void AddGodotMcp_ResolvesMcpProtocolClientFactoryWithCorrectImplementation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IMcpProtocolClientFactory>();

        Assert.IsType<McpSdkProtocolClientFactory>(factory);
    }

    [Fact]
    public void AddGodotMcp_ResolvesParameterConverterWithCorrectImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var parameterConverter = serviceProvider.GetRequiredService<IParameterConverter>();

        // Assert
        Assert.IsType<ParameterConverter>(parameterConverter);
    }

    [Fact]
    public void AddGodotMcp_ResolvesFunctionMapperWithCorrectImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var functionMapper = serviceProvider.GetRequiredService<IFunctionMapper>();

        // Assert
        Assert.IsType<FunctionMapper>(functionMapper);
    }

    [Fact]
    public void AddGodotMcp_ResolvesMcpClientWithCorrectImplementation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var mcpClient = serviceProvider.GetRequiredService<IMcpClient>();

        // Assert
        Assert.IsType<StdioMcpClient>(mcpClient);
    }

    [Fact]
    public void AddGodotMcp_ResolvesGodotPlugin()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var plugin = serviceProvider.GetRequiredService<GodotPlugin>();

        // Assert
        Assert.NotNull(plugin);
        Assert.IsType<GodotPlugin>(plugin);
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void AddGodotMcp_RegistersConfigurationValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration);

        // Assert - Check that validator is registered
        var validatorDescriptor = services.FirstOrDefault(s => 
            s.ServiceType == typeof(IValidateOptions<GodotMcpOptions>));
        
        Assert.NotNull(validatorDescriptor);
        Assert.Equal(typeof(GodotMcpOptionsValidator), validatorDescriptor.ImplementationType);
    }

    [Fact]
    public void AddGodotMcp_ValidationRunsAtStartup_WithInvalidExecutablePath()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "", // Invalid: empty string
                ["GodotMcp:ConnectionTimeoutSeconds"] = "30"
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Validation should fail when trying to access options
        Assert.Throws<OptionsValidationException>(() => 
            serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value);
    }

    [Fact]
    public void AddGodotMcp_ValidationRunsAtStartup_WithInvalidConnectionTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp",
                ["GodotMcp:ConnectionTimeoutSeconds"] = "0" // Invalid: must be positive
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Validation should fail
        Assert.Throws<OptionsValidationException>(() => 
            serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value);
    }

    [Fact]
    public void AddGodotMcp_ValidationRunsAtStartup_WithInvalidRequestTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp",
                ["GodotMcp:RequestTimeoutSeconds"] = "-5" // Invalid: must be positive
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Validation should fail
        Assert.Throws<OptionsValidationException>(() => 
            serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value);
    }

    [Fact]
    public void AddGodotMcp_ValidationRunsAtStartup_WithInvalidMaxRetryAttempts()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp",
                ["GodotMcp:MaxRetryAttempts"] = "-1" // Invalid: cannot be negative
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Validation should fail
        Assert.Throws<OptionsValidationException>(() => 
            serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value);
    }

    [Fact]
    public void AddGodotMcp_ValidationPasses_WithValidConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp",
                ["GodotMcp:ConnectionTimeoutSeconds"] = "30",
                ["GodotMcp:RequestTimeoutSeconds"] = "60",
                ["GodotMcp:MaxRetryAttempts"] = "3"
            })
            .Build();

        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act - Should not throw
        var options = serviceProvider.GetRequiredService<IOptions<GodotMcpOptions>>().Value;

        // Assert
        Assert.NotNull(options);
        Assert.Equal("godot-mcp", options.ExecutablePath);
    }

    #endregion

    #region Keyed Services Tests

    [Fact]
    public void AddGodotMcp_WithServiceKey_RegistersKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act
        services.AddGodotMcp(configuration, serviceKey: "editor-1");
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify keyed services can be resolved
        var protocolFactory = serviceProvider.GetKeyedService<IMcpProtocolClientFactory>("editor-1");
        var parameterConverter = serviceProvider.GetKeyedService<IParameterConverter>("editor-1");
        var functionMapper = serviceProvider.GetKeyedService<IFunctionMapper>("editor-1");
        var mcpClient = serviceProvider.GetKeyedService<IMcpClient>("editor-1");
        var plugin = serviceProvider.GetKeyedService<GodotPlugin>("editor-1");

        Assert.NotNull(protocolFactory);
        Assert.NotNull(parameterConverter);
        Assert.NotNull(functionMapper);
        Assert.NotNull(mcpClient);
        Assert.NotNull(plugin);
    }

    [Fact]
    public void AddGodotMcp_WithMultipleServiceKeys_RegistersMultipleInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act - Register two instances with different keys
        services.AddGodotMcp(configuration, serviceKey: "editor-1");
        services.AddGodotMcp(configuration, serviceKey: "editor-2");
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify both instances can be resolved and are different
        var plugin1 = serviceProvider.GetKeyedService<GodotPlugin>("editor-1");
        var plugin2 = serviceProvider.GetKeyedService<GodotPlugin>("editor-2");

        Assert.NotNull(plugin1);
        Assert.NotNull(plugin2);
        Assert.NotSame(plugin1, plugin2); // Different instances
    }

    [Fact]
    public void AddGodotMcp_WithServiceKey_DoesNotRegisterNonKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act - Register only with service key
        services.AddGodotMcp(configuration, serviceKey: "editor-1");
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Non-keyed services should not be available
        var plugin = serviceProvider.GetService<GodotPlugin>();
        Assert.Null(plugin);
    }

    [Fact]
    public void AddGodotMcp_WithoutServiceKey_RegistersNonKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act - Register without service key
        services.AddGodotMcp(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Non-keyed services should be available
        var plugin = serviceProvider.GetService<GodotPlugin>();
        Assert.NotNull(plugin);
    }

    [Fact]
    public void AddGodotMcp_WithAction_SupportsKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddGodotMcp(options =>
        {
            options.ExecutablePath = "godot-mcp";
        }, serviceKey: "editor-1");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify keyed services can be resolved
        var plugin = serviceProvider.GetKeyedService<GodotPlugin>("editor-1");
        Assert.NotNull(plugin);
    }

    #endregion

    #region Method Chaining Tests

    [Fact]
    public void AddGodotMcp_WithConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddGodotMcp(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddGodotMcp_WithAction_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGodotMcp(options => { });

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddGodotMcp_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GodotMcp:ExecutablePath"] = "godot-mcp"
            })
            .Build();

        // Act - Chain multiple service registrations
        var result = services
            .AddGodotMcp(configuration)
            .AddSingleton<string>("test");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<GodotPlugin>());
        Assert.Equal("test", serviceProvider.GetService<string>());
    }

    #endregion
}
