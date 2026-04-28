using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GodotMcp.Core.Exceptions;
using GodotMcp.Plugin;
using GodotMcp.Plugin.Extensions;

// Create host application builder for modern .NET hosting pattern
var builder = Host.CreateApplicationBuilder(args);

// Configure services
// Add Godot MCP plugin with configuration from appsettings.json
builder.Services.AddGodotMcp(builder.Configuration);

// Build the host
using var host = builder.Build();

// Get required services from dependency injection container
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var plugin = host.Services.GetRequiredService<GodotPlugin>();

logger.LogInformation("Godot MCP Sample Application Starting...");

try
{
    // Step 1: Initialize the Godot plugin
    // This connects to the godot-mcp process and discovers available tools
    logger.LogInformation("Initializing Godot plugin...");
    await plugin.InitializeAsync();
    logger.LogInformation("Godot plugin initialized successfully");

    // Step 2: Validate project folder and query server version.
    var isGodotProject = await plugin.ValidateGodotProjectAsync(Environment.CurrentDirectory, CancellationToken.None);
    logger.LogInformation("Project validation for '{Path}': {IsValid}", Environment.CurrentDirectory, isGodotProject);

    var versionInfo = await plugin.GetGodotVersionAsync(Environment.CurrentDirectory, CancellationToken.None);
    logger.LogInformation("Version info: {VersionInfo}", versionInfo);

    // Step 3: Create a scene with the current GD_MCP contract.
    const string sampleSceneFileName = "SampleScene.tscn";
    logger.LogInformation("Creating a new Godot scene...");
    var createSceneResult = await plugin.InvokeToolAsync(
        "create_scene",
        new Dictionary<string, object?>
        {
            ["projectPath"] = Environment.CurrentDirectory,
            ["fileName"] = sampleSceneFileName,
            ["rootNodeName"] = "SampleRoot",
            ["root_type"] = "Node3D"
        },
        CancellationToken.None);
    logger.LogInformation("Scene created successfully: {Result}", createSceneResult);

    // Step 4: Add a node to the scene.
    logger.LogInformation("Adding a child node...");
    var addNodeResult = await plugin.InvokeToolAsync(
        "add_node",
        new Dictionary<string, object?>
        {
            ["projectPath"] = Environment.CurrentDirectory,
            ["fileName"] = sampleSceneFileName,
            ["parentPath"] = ".",
            ["nodeName"] = "DemoCamera",
            ["nodeType"] = "Camera3D",
            ["root_type"] = "Node3D"
        },
        CancellationToken.None);
    logger.LogInformation("Node added successfully: {Result}", addNodeResult);

    // Step 5: Query project info.
    var projectInfoResult = await plugin.InvokeToolAsync("get_project_info", new Dictionary<string, object?>(), CancellationToken.None);
    logger.LogInformation("Project information retrieved: {Result}", projectInfoResult);

    logger.LogInformation("Sample application completed successfully!");
}
catch (ConfigurationException ex)
{
    // Handle configuration errors
    // These occur when the plugin is misconfigured
    logger.LogError(ex, "Configuration error: {Message}. Parameter: {Parameter}",
        ex.Message, ex.ParameterName);
    logger.LogError("Please check your appsettings.json configuration");
    return 1;
}
catch (ProcessException ex)
{
    // Handle process management errors
    // These occur when the godot-mcp process cannot be started or managed
    logger.LogError(ex, "Process error: {Message}. Process ID: {ProcessId}",
        ex.Message, ex.ProcessId);
    logger.LogError("Ensure godot-mcp is installed and accessible in your PATH");
    logger.LogError("Install with: dotnet tool install -g godot-mcp");
    return 2;
}
catch (NetworkException ex)
{
    // Handle network/communication errors
    // These occur when communication with godot-mcp fails
    logger.LogError(ex, "Network error: {Message}. Endpoint: {Endpoint}",
        ex.Message, ex.Endpoint);
    logger.LogError("Check if godot-mcp process is running and responsive");
    return 3;
}
catch (GodotMcp.Core.Exceptions.TimeoutException ex)
{
    // Handle timeout errors
    // These occur when operations take too long
    logger.LogError(ex, "Timeout error: {Message}. Operation: {Operation}, Timeout: {Timeout}",
        ex.Message, ex.Operation, ex.Timeout);
    logger.LogError("Consider increasing timeout values in configuration");
    return 4;
}
catch (McpServerException ex)
{
    // Handle godot-mcp server errors
    // These occur when the Godot server returns an error
    logger.LogError(ex, "Godot MCP server error: {Message}. Error code: {ErrorCode}",
        ex.Message, ex.ErrorCode);
    if (ex.ErrorData != null)
    {
        logger.LogError("Error data: {ErrorData}", ex.ErrorData);
    }
    return 5;
}
catch (ProtocolException ex)
{
    // Handle protocol errors
    // These occur when MCP messages are malformed
    logger.LogError(ex, "Protocol error: {Message}", ex.Message);
    if (ex.MalformedData != null)
    {
        logger.LogError("Malformed data: {Data}", ex.MalformedData);
    }
    return 6;
}
catch (GodotMcpException ex)
{
    // Handle any other Godot MCP errors
    logger.LogError(ex, "Godot MCP error: {Message}", ex.Message);
    return 7;
}
catch (Exception ex)
{
    // Handle unexpected errors
    logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
    return 99;
}

return 0;
