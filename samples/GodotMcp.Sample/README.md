# Godot MCP Sample Application

This sample demonstrates both validation and tool invocation against the current GD MCP contracts.

## Prerequisites

Before running this sample, you need `godot-mcp` available:

```bash
dotnet tool install --global GodotMcp.Server
```

Verify the installation:

```bash
godot-mcp --version
```

## Configuration

The sample uses `appsettings.json` for configuration. The default settings are:

```json
{
  "GodotMcp": {
    "ExecutablePath": "godot-mcp",
    "ConnectionTimeoutSeconds": 30,
    "RequestTimeoutSeconds": 60,
    "MaxRetryAttempts": 3,
    "BackoffStrategy": "Exponential",
    "InitialRetryDelayMs": 1000,
    "EnableProcessPooling": true,
    "MaxIdleTimeSeconds": 300,
    "EnableMessageLogging": false
  }
}
```

### Configuration Options

- **ExecutablePath**: Path to the godot-mcp executable (default: "godot-mcp")
- **ConnectionTimeoutSeconds**: Maximum time to wait for connection (default: 30)
- **RequestTimeoutSeconds**: Maximum time to wait for a request response (default: 60)
- **MaxRetryAttempts**: Number of retry attempts for transient failures (default: 3)
- **BackoffStrategy**: Retry backoff strategy - "Linear" or "Exponential" (default: Exponential)
- **InitialRetryDelayMs**: Initial delay before first retry in milliseconds (default: 1000)
- **EnableProcessPooling**: Whether to reuse the godot-mcp process (default: true)
- **MaxIdleTimeSeconds**: Maximum idle time before process shutdown (default: 300)
- **EnableMessageLogging**: Enable detailed request/response logging (default: false)

## Running the Sample

1. Ensure your working directory points to a Godot project (contains `project.godot`).
2. Run the sample application:

```bash
dotnet run
```

Or build and run:

```bash
dotnet build
dotnet run --no-build
```

## What the Sample Does

The sample demonstrates:
- Plugin initialization + tool discovery.
- `ValidateGodotProjectAsync`.
- `GetGodotVersionAsync`.
- `create_scene`, `add_node`, and `get_project_info` tool calls.

## Error Handling

The sample demonstrates comprehensive error handling for different types of failures:

- **ConfigurationException**: Configuration errors (check appsettings.json)
- **ProcessException**: Process management errors (ensure godot-mcp is installed)
- **NetworkException**: Communication errors (check if godot-mcp is responsive)
- **TimeoutException**: Operation timeout errors (consider increasing timeout values)
- **McpServerException**: Godot server errors (check Godot Editor logs)
- **ProtocolException**: MCP protocol errors (malformed messages)
- **GodotMcpException**: General Godot MCP errors

Each error type returns a specific exit code for easy diagnosis.

## Code Structure

The sample uses modern .NET hosting patterns:

```csharp
// Create host with dependency injection
var builder = Host.CreateApplicationBuilder(args);

// Register Godot MCP services
builder.Services.AddGodotMcp(builder.Configuration);

// Build and use services
using var host = builder.Build();
var plugin = host.Services.GetRequiredService<GodotPlugin>();

// Initialize and use the plugin
await plugin.InitializeAsync();
var result = await plugin.InvokeToolAsync("Godot_create_scene", parameters);
```

## Customizing the Sample

You can modify the sample to invoke different Godot tools. After initialization, the plugin discovers all available tools from the godot-mcp server. Check the godot-mcp-Server documentation for available tools and their parameters.

Example of invoking a custom tool:

```csharp
var parameters = new Dictionary<string, object?>
{
    ["parameterName"] = "value",
    ["anotherParameter"] = 42
};

var result = await plugin.InvokeToolAsync(
    "your_tool_name",
    parameters,
    CancellationToken.None);
```

## Troubleshooting

### "godot-mcp not found"
- Ensure godot-mcp is installed: `dotnet tool install -g godot-mcp`
- Check if it's in your PATH: `godot-mcp --version`

### "Connection timeout"
- Ensure Godot Editor is running
- Increase `ConnectionTimeoutSeconds` in appsettings.json
- Check Godot Editor console for errors

### "Request timeout"
- The operation is taking too long
- Increase `RequestTimeoutSeconds` in appsettings.json
- Check if Godot Editor is responsive

### "Process error"
- The godot-mcp process failed to start
- Check if the executable path is correct
- Verify godot-mcp installation

## Next Steps

- Explore other Godot tools available through the MCP server
- Integrate the plugin into your Semantic Kernel applications
- Customize error handling for your specific use cases
- Add logging configuration for detailed diagnostics

## Related Documentation

- [Godot-MCP-SK-Plugin README](../../README.md)
- [Godot-MCP-Server Repository](https://github.com/Ozymandros/Godot-MCP-Server)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
