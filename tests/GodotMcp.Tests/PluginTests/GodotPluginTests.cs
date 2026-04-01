using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using GodotMcp.Core.Exceptions;
using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;
using GodotMcp.Plugin;

namespace GodotMcp.Tests.PluginTests;

public class GodotPluginTests
{
    private readonly IMcpClient _mockMcpClient;
    private readonly IFunctionMapper _mockFunctionMapper;
    private readonly IParameterConverter _mockParameterConverter;
    private readonly GodotPlugin _plugin;

    public GodotPluginTests()
    {
        _mockMcpClient = Substitute.For<IMcpClient>();
        _mockFunctionMapper = Substitute.For<IFunctionMapper>();
        _mockParameterConverter = Substitute.For<IParameterConverter>();
        
        _plugin = new GodotPlugin(
            _mockMcpClient,
            _mockFunctionMapper,
            _mockParameterConverter,
            NullLogger<GodotPlugin>.Instance);
    }

    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_ConnectsToMcpClient()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("test_tool", "Test tool", new Dictionary<string, McpParameterDefinition>())
        };
        
        _mockMcpClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockMcpClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        _mockFunctionMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _plugin.InitializeAsync();

        // Assert
        await _mockMcpClient.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_DiscoverToolsFromMcpClient()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool1", "First tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool2", "Second tool", new Dictionary<string, McpParameterDefinition>())
        };
        
        _mockMcpClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockMcpClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        _mockFunctionMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _plugin.InitializeAsync();

        // Assert
        await _mockMcpClient.Received(1).ListToolsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_RegistersDiscoveredTools()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool1", "First tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool2", "Second tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool3", "Third tool", new Dictionary<string, McpParameterDefinition>())
        };
        
        _mockMcpClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockMcpClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        _mockFunctionMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _plugin.InitializeAsync();

        // Assert
        await _mockFunctionMapper.Received(1).RegisterToolsAsync(
            Arg.Is<IReadOnlyList<McpToolDefinition>>(t => t.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_WithCancellationToken_PassesTokenToAllCalls()
    {
        // Arrange
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("test_tool", "Test tool", new Dictionary<string, McpParameterDefinition>())
        };
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        
        _mockMcpClient.ConnectAsync(token).Returns(Task.CompletedTask);
        _mockMcpClient.ListToolsAsync(token).Returns(tools);
        _mockFunctionMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), token)
            .Returns(Task.CompletedTask);

        // Act
        await _plugin.InitializeAsync(token);

        // Assert
        await _mockMcpClient.Received(1).ConnectAsync(token);
        await _mockMcpClient.Received(1).ListToolsAsync(token);
        await _mockFunctionMapper.Received(1).RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), token);
    }

    [Fact]
    public async Task InitializeAsync_WhenConnectionFails_ThrowsException()
    {
        // Arrange
        _mockMcpClient.ConnectAsync(Arg.Any<CancellationToken>())
            .Throws(new NetworkException("Connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<NetworkException>(() => _plugin.InitializeAsync());
    }

    [Fact]
    public async Task InitializeAsync_WhenToolDiscoveryFails_ThrowsException()
    {
        // Arrange
        _mockMcpClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _mockMcpClient.ListToolsAsync(Arg.Any<CancellationToken>())
            .Throws(new McpServerException("Tool discovery failed", 500));

        // Act & Assert
        await Assert.ThrowsAsync<McpServerException>(() => _plugin.InitializeAsync());
    }

    #endregion

    #region InvokeToolAsync Tests

    [Fact]
    public async Task InvokeToolAsync_InvokesCorrectMcpTool()
    {
        // Arrange
        var toolName = "Godot_create_scene";
        var parameters = new Dictionary<string, object?> { ["name"] = "TestScene" };
        var toolDefinition = new McpToolDefinition(
            toolName,
            "Creates a scene",
            new Dictionary<string, McpParameterDefinition>
            {
                ["name"] = new McpParameterDefinition("name", "string", "Scene name", true)
            });
        var mcpParameters = new Dictionary<string, object?> { ["name"] = "TestScene" };
        var response = new McpResponse("1", true, "Scene created");

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _plugin.InvokeToolAsync(toolName, parameters);

        // Assert
        await _mockMcpClient.Received(1).InvokeToolAsync(
            toolName,
            Arg.Is<IReadOnlyDictionary<string, object?>>(p => p["name"]!.Equals("TestScene")),
            Arg.Any<CancellationToken>());
        Assert.Equal("Scene created", result);
    }

    [Fact]
    public async Task InvokeToolAsync_ConvertsParametersCorrectly()
    {
        // Arrange
        var toolName = "Godot_set_position";
        var parameters = new Dictionary<string, object?> 
        { 
            ["objectName"] = "Cube",
            ["x"] = 1.0,
            ["y"] = 2.0,
            ["z"] = 3.0
        };
        var toolDefinition = new McpToolDefinition(
            toolName,
            "Sets position",
            new Dictionary<string, McpParameterDefinition>
            {
                ["objectName"] = new McpParameterDefinition("objectName", "string", "Object name", true),
                ["x"] = new McpParameterDefinition("x", "number", "X coordinate", true),
                ["y"] = new McpParameterDefinition("y", "number", "Y coordinate", true),
                ["z"] = new McpParameterDefinition("z", "number", "Z coordinate", true)
            });
        var mcpParameters = new Dictionary<string, object?> 
        { 
            ["objectName"] = "Cube",
            ["x"] = 1.0,
            ["y"] = 2.0,
            ["z"] = 3.0
        };
        var response = new McpResponse("1", true, true);

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        await _plugin.InvokeToolAsync(toolName, parameters);

        // Assert
        _mockParameterConverter.Received(1).ConvertToMcp(
            Arg.Is<IReadOnlyDictionary<string, object?>>(p => 
                p["objectName"]!.Equals("Cube") &&
                p["x"]!.Equals(1.0) &&
                p["y"]!.Equals(2.0) &&
                p["z"]!.Equals(3.0)),
            toolDefinition);
    }

    [Fact]
    public async Task InvokeToolAsync_ThrowsExceptionForUnknownTool()
    {
        // Arrange
        var toolName = "unknown_tool";
        var parameters = new Dictionary<string, object?>();

        _mockFunctionMapper.GetRegisteredToolNames().Returns(Array.Empty<string>());
        _mockFunctionMapper.GetToolDefinition(toolName).Returns((McpToolDefinition?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<GodotMcpException>(
            () => _plugin.InvokeToolAsync(toolName, parameters));
        
        Assert.Contains("not registered", exception.Message);
        Assert.Contains(toolName, exception.Message);
    }

    [Fact]
    public async Task InvokeToolAsync_WithComplexParameters_ConvertsCorrectly()
    {
        // Arrange
        var toolName = "Godot_create_object";
        var parameters = new Dictionary<string, object?> 
        { 
            ["name"] = "GameObject",
            ["position"] = new { x = 1.0, y = 2.0, z = 3.0 },
            ["components"] = new[] { "Rigidbody", "BoxCollider" }
        };
        var toolDefinition = new McpToolDefinition(
            toolName,
            "Creates object",
            new Dictionary<string, McpParameterDefinition>
            {
                ["name"] = new McpParameterDefinition("name", "string", "Object name", true),
                ["position"] = new McpParameterDefinition("position", "object", "Position", false),
                ["components"] = new McpParameterDefinition("components", "array", "Components", false)
            });
        var mcpParameters = new Dictionary<string, object?> 
        { 
            ["name"] = "GameObject",
            ["position"] = new { x = 1.0, y = 2.0, z = 3.0 },
            ["components"] = new[] { "Rigidbody", "BoxCollider" }
        };
        var response = new McpResponse("1", true, "Object created");

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _plugin.InvokeToolAsync(toolName, parameters);

        // Assert
        _mockParameterConverter.Received(1).ConvertToMcp(parameters, toolDefinition);
        Assert.Equal("Object created", result);
    }

    [Fact]
    public async Task InvokeToolAsync_WithEmptyParameters_InvokesSuccessfully()
    {
        // Arrange
        var toolName = "Godot_get_scene_name";
        var parameters = new Dictionary<string, object?>();
        var toolDefinition = new McpToolDefinition(
            toolName,
            "Gets scene name",
            new Dictionary<string, McpParameterDefinition>());
        var mcpParameters = new Dictionary<string, object?>();
        var response = new McpResponse("1", true, "MainScene");

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _plugin.InvokeToolAsync(toolName, parameters);

        // Assert
        await _mockMcpClient.Received(1).InvokeToolAsync(toolName, Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
        Assert.Equal("MainScene", result);
    }

    [Fact]
    public async Task InvokeToolAsync_WithNullResult_ReturnsNull()
    {
        // Arrange
        var toolName = "Godot_delete_object";
        var parameters = new Dictionary<string, object?> { ["name"] = "Cube" };
        var toolDefinition = new McpToolDefinition(
            toolName,
            "Deletes object",
            new Dictionary<string, McpParameterDefinition>
            {
                ["name"] = new McpParameterDefinition("name", "string", "Object name", true)
            });
        var mcpParameters = new Dictionary<string, object?> { ["name"] = "Cube" };
        var response = new McpResponse("1", true, null);

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _plugin.InvokeToolAsync(toolName, parameters);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InvokeToolAsync_WithCancellationToken_PassesTokenToMcpClient()
    {
        // Arrange
        var toolName = "Godot_test";
        var parameters = new Dictionary<string, object?>();
        var toolDefinition = new McpToolDefinition(toolName, "Test", new Dictionary<string, McpParameterDefinition>());
        var mcpParameters = new Dictionary<string, object?>();
        var response = new McpResponse("1", true, "Success");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, token).Returns(response);

        // Act
        await _plugin.InvokeToolAsync(toolName, parameters, token);

        // Assert
        await _mockMcpClient.Received(1).InvokeToolAsync(toolName, Arg.Any<IReadOnlyDictionary<string, object?>>(), token);
    }

    [Fact]
    public async Task InvokeToolAsync_WhenMcpClientThrowsNetworkException_PropagatesException()
    {
        // Arrange
        var toolName = "Godot_test";
        var parameters = new Dictionary<string, object?>();
        var toolDefinition = new McpToolDefinition(toolName, "Test", new Dictionary<string, McpParameterDefinition>());
        var mcpParameters = new Dictionary<string, object?>();

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Throws(new NetworkException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<NetworkException>(() => _plugin.InvokeToolAsync(toolName, parameters));
    }

    [Fact]
    public async Task InvokeToolAsync_WhenMcpClientThrowsTimeoutException_PropagatesException()
    {
        // Arrange
        var toolName = "Godot_test";
        var parameters = new Dictionary<string, object?>();
        var toolDefinition = new McpToolDefinition(toolName, "Test", new Dictionary<string, McpParameterDefinition>());
        var mcpParameters = new Dictionary<string, object?>();

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition).Returns(mcpParameters);
        _mockMcpClient.InvokeToolAsync(toolName, mcpParameters, Arg.Any<CancellationToken>())
            .Throws(new GodotMcp.Core.Exceptions.TimeoutException("Request timeout", TimeSpan.FromSeconds(30)));

        // Act & Assert
        await Assert.ThrowsAsync<GodotMcp.Core.Exceptions.TimeoutException>(() => _plugin.InvokeToolAsync(toolName, parameters));
    }

    [Fact]
    public async Task InvokeToolAsync_WhenParameterConversionFails_PropagatesException()
    {
        // Arrange
        var toolName = "Godot_test";
        var parameters = new Dictionary<string, object?> { ["param1"] = "valid string" };
        var toolDefinition = new McpToolDefinition(
            toolName, 
            "Test", 
            new Dictionary<string, McpParameterDefinition>
            {
                ["param1"] = new McpParameterDefinition("param1", "string", "Test parameter", true)
            });

        _mockFunctionMapper.GetRegisteredToolNames().Returns(new[] { toolName });
        _mockFunctionMapper.GetToolDefinition(toolName).Returns(toolDefinition);
        _mockParameterConverter.ConvertToMcp(parameters, toolDefinition)
            .Throws(new TypeConversionException("Conversion failed"));

        // Act & Assert
        await Assert.ThrowsAsync<TypeConversionException>(() => _plugin.InvokeToolAsync(toolName, parameters));
    }

    #endregion

    #region CreateKernelWithGodotAsync Tests

    [Fact]
    public async Task CreateKernelWithGodotAsync_RegistersAllTools()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("tool1", "First tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool2", "Second tool", new Dictionary<string, McpParameterDefinition>()),
            new McpToolDefinition("tool3", "Third tool", new Dictionary<string, McpParameterDefinition>())
        };

        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var mockConverter = Substitute.For<IParameterConverter>();

        mockClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        mockClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        mockMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockMapper.GetRegisteredTools().Returns(tools);

        foreach (var tool in tools)
        {
            var metadata = new KernelFunctionMetadata(tool.Name) { Description = tool.Description };
            mockMapper.MapToKernelFunction(tool).Returns(metadata);
            mockMapper.GetToolDefinition(tool.Name).Returns(tool);
        }

        var plugin = new GodotPlugin(mockClient, mockMapper, mockConverter, NullLogger<GodotPlugin>.Instance);
        services.AddSingleton(plugin);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var kernel = await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider);

        // Assert
        Assert.NotNull(kernel);
        var pluginCollection = Assert.Single(kernel.Plugins);
        Assert.Equal("godot", pluginCollection.Name);

        var functionNames = pluginCollection.Select(f => f.Name).ToList();
        Assert.Contains("godot_tool1", functionNames);
        Assert.Contains("godot_tool2", functionNames);
        Assert.Contains("godot_tool3", functionNames);
    }

    [Fact]
    public async Task CreateKernelWithGodotAsync_InitializesPlugin()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("test_tool", "Test tool", new Dictionary<string, McpParameterDefinition>())
        };

        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var mockConverter = Substitute.For<IParameterConverter>();

        mockClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        mockClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        mockMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockMapper.GetRegisteredTools().Returns(tools);

        var metadata = new KernelFunctionMetadata("test_tool") { Description = "Test tool" };
        mockMapper.MapToKernelFunction(tools[0]).Returns(metadata);
        mockMapper.GetToolDefinition("test_tool").Returns(tools[0]);

        var plugin = new GodotPlugin(mockClient, mockMapper, mockConverter, NullLogger<GodotPlugin>.Instance);
        services.AddSingleton(plugin);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider);

        // Assert
        await mockClient.Received(1).ConnectAsync(Arg.Any<CancellationToken>());
        await mockClient.Received(1).ListToolsAsync(Arg.Any<CancellationToken>());
        await mockMapper.Received(1).RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateKernelWithGodotAsync_WithCancellationToken_PassesTokenToInitialize()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("test_tool", "Test tool", new Dictionary<string, McpParameterDefinition>())
        };

        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var mockConverter = Substitute.For<IParameterConverter>();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        mockClient.ConnectAsync(token).Returns(Task.CompletedTask);
        mockClient.ListToolsAsync(token).Returns(tools);
        mockMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), token)
            .Returns(Task.CompletedTask);
        mockMapper.GetRegisteredTools().Returns(tools);

        var metadata = new KernelFunctionMetadata("test_tool") { Description = "Test tool" };
        mockMapper.MapToKernelFunction(tools[0]).Returns(metadata);
        mockMapper.GetToolDefinition("test_tool").Returns(tools[0]);

        var plugin = new GodotPlugin(mockClient, mockMapper, mockConverter, NullLogger<GodotPlugin>.Instance);
        services.AddSingleton(plugin);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider, token);

        // Assert
        await mockClient.Received(1).ConnectAsync(token);
    }

    [Fact]
    public async Task CreateKernelWithGodotAsync_WhenInitializationFails_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var mockConverter = Substitute.For<IParameterConverter>();

        mockClient.ConnectAsync(Arg.Any<CancellationToken>())
            .Throws(new NetworkException("Connection failed"));

        var plugin = new GodotPlugin(mockClient, mockMapper, mockConverter, NullLogger<GodotPlugin>.Instance);
        services.AddSingleton(plugin);

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        await Assert.ThrowsAsync<NetworkException>(
            () => GodotPlugin.CreateKernelWithGodotAsync(serviceProvider));
    }

    [Fact]
    public async Task CreateKernelWithGodotAsync_CreatesKernelWithLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        
        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition("test_tool", "Test tool", new Dictionary<string, McpParameterDefinition>())
        };

        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var mockConverter = Substitute.For<IParameterConverter>();

        mockClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        mockClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        mockMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockMapper.GetRegisteredTools().Returns(tools);

        var metadata = new KernelFunctionMetadata("test_tool") { Description = "Test tool" };
        mockMapper.MapToKernelFunction(tools[0]).Returns(metadata);
        mockMapper.GetToolDefinition("test_tool").Returns(tools[0]);

        var plugin = new GodotPlugin(mockClient, mockMapper, mockConverter, NullLogger<GodotPlugin>.Instance);
        services.AddSingleton(plugin);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var kernel = await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider);

        // Assert
        Assert.NotNull(kernel);
    }

    [Fact]
    public async Task CreateKernelWithGodotAsync_ExposesPerToolParameterMetadata()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var tools = new List<McpToolDefinition>
        {
            new McpToolDefinition(
                "Godot_create_scene",
                "Creates a new Godot scene",
                new Dictionary<string, McpParameterDefinition>
                {
                    ["sceneName"] = new McpParameterDefinition("sceneName", "string", "Scene name", true),
                    ["addToHierarchy"] = new McpParameterDefinition("addToHierarchy", "boolean", "Add to hierarchy", false, true)
                })
        };

        var mockClient = Substitute.For<IMcpClient>();
        var mockMapper = Substitute.For<IFunctionMapper>();
        var mockConverter = Substitute.For<IParameterConverter>();

        mockClient.ConnectAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        mockClient.ListToolsAsync(Arg.Any<CancellationToken>()).Returns(tools);
        mockMapper.RegisterToolsAsync(Arg.Any<IReadOnlyList<McpToolDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockMapper.GetRegisteredTools().Returns(tools);

        var metadata = new KernelFunctionMetadata("Godot_create_scene")
        {
            Description = "Creates a new Godot scene",
            Parameters =
            [
                new KernelParameterMetadata("sceneName") { Description = "Scene name", IsRequired = true, ParameterType = typeof(string) },
                new KernelParameterMetadata("addToHierarchy") { Description = "Add to hierarchy", IsRequired = false, DefaultValue = true, ParameterType = typeof(bool) }
            ]
        };
        mockMapper.MapToKernelFunction(tools[0]).Returns(metadata);

        var plugin = new GodotPlugin(mockClient, mockMapper, mockConverter, NullLogger<GodotPlugin>.Instance);
        services.AddSingleton(plugin);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var kernel = await GodotPlugin.CreateKernelWithGodotAsync(serviceProvider);

        // Assert — the registered function exposes individual MCP parameters, not a single Dictionary
        var pluginCollection = kernel.Plugins["godot"];
        var func = pluginCollection["godot_create_scene"];
        Assert.Equal("Creates a new Godot scene", func.Description);
        Assert.Equal(2, func.Metadata.Parameters.Count);

        var sceneNameParam = func.Metadata.Parameters.First(p => p.Name == "sceneName");
        Assert.True(sceneNameParam.IsRequired);
        Assert.Equal(typeof(string), sceneNameParam.ParameterType);

        var addToHierarchyParam = func.Metadata.Parameters.First(p => p.Name == "addToHierarchy");
        Assert.False(addToHierarchyParam.IsRequired);
        Assert.Equal(typeof(bool), addToHierarchyParam.ParameterType);
    }

    #endregion
}
