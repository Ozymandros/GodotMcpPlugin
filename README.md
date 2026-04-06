## Godot-MCP-SK-Plugin

<!-- CI & Quality -->
[![CI](https://github.com/Ozymandros/GodotMcpPlugin/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/Ozymandros/GodotMcpPlugin/actions/workflows/ci.yml)
[![Release](https://github.com/Ozymandros/GodotMcpPlugin/actions/workflows/release.yml/badge.svg?branch=master)](https://github.com/Ozymandros/GodotMcpPlugin/actions/workflows/release.yml)
[![CodeQL](https://github.com/Ozymandros/GodotMcpPlugin/actions/workflows/codeql.yml/badge.svg?branch=master)](https://github.com/Ozymandros/GodotMcpPlugin/actions/workflows/codeql.yml)

<!-- Package -->
[![NuGet](https://img.shields.io/nuget/v/GodotMcp.SemanticKernel.Plugin.svg)](https://www.nuget.org/packages/GodotMcp.SemanticKernel.Plugin)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GodotMcp.SemanticKernel.Plugin.svg)](https://www.nuget.org/packages/GodotMcp.SemanticKernel.Plugin)

<!-- Project -->
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A .NET 10 Semantic Kernel plugin that connects SK agents/apps to **`GodotMCP.Server` 1.2+** (including newer tool surfaces such as camera settings in current releases) via the .NET global tool `godot-mcp`, using the official **[ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol)** .NET SDK over stdio (`initialize`, `tools/list`, `tools/call`) and dynamically exposing Godot automation tools as Kernel functions.

### Key Features

## Architecture

- `GodotMcp.Core`: interfaces, models, exceptions.
- `GodotMcp.Infrastructure`: stdio client, process manager, serialization, conversion, options.
- `GodotMcp.Plugin`: Semantic Kernel integration, function mapping, DI extensions.
- `GodotMcp.Tests`: unit/integration/property-based tests.

## Quick Start

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddGodotMcp(options =>
{
    options.ExecutablePath = "godot-mcp";
    options.GodotExecutablePath = Environment.GetEnvironmentVariable("GODOT_PATH");
});

using var host = builder.Build();
var plugin = host.Services.GetRequiredService<GodotPlugin>();
await plugin.InitializeAsync();

var result = await plugin.InvokeToolAsync(
    "create_scene",
    new Dictionary<string, object?>
    {
        ["scenePath"] = "res://Scenes/Main.tscn",
        ["rootNodeName"] = "Main",
        ["rootNodeType"] = "Node2D"
    });
```

## SK Registration Modes

- Expanded mode (recommended): `kernel.RegisterGodotTools(host.Services, "godot")`
  - Registers each discovered method as an individual function like `godot_create_scene`.
- Router mode: `kernel.Plugins.AddFromObject(plugin, "godot")`
  - Exposes a single `invoke_godot_tool(toolName, parameters)` function.

## Configuration

`appsettings.json`:

```json
{
  "GodotMcp": {
    "ExecutablePath": "godot-mcp",
    "GodotExecutablePath": null,
    "ConnectionTimeoutSeconds": 30,
    "RequestTimeoutSeconds": 60,
    "MaxRetryAttempts": 3,
    "BackoffStrategy": "Exponential",
    "InitialRetryDelayMs": 1000,
    "EnableProcessPooling": true,
    "MaxIdleTimeSeconds": 300,
    "ToolDefinitionsPath": null,
    "EnableMessageLogging": false
  }
}
```

Environment overrides:
- `GODOT_MCP_PATH`: overrides the MCP server executable path.
- `GODOT_PATH`: Godot binary path used by server CLI-backed tools.

## Current GD Tool Surface

This plugin supports full dynamic discovery and invocation of the local `GD_MCP-Server` tools across:
- Core and diagnostics
- Projects
- Scenes and nodes
- Scripts
- Resources and imports
- Editor/export automation
- Integration discovery and health tooling

Detailed reference: `Docs/tool-contracts.md`.

## Documentation

- `Docs/tool-contracts.md`
- `Docs/godot-cli-setup.md`
- `Docs/docker-mcp-setup.md`
- `Docs/testing-guide.md`
- `Skills/SKILL.md`

## Test

```bash
dotnet test GodotMcp.sln
dotnet test GodotMcp.sln --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `ExecutablePath` | string | `"godot-mcp"` | Path to the godot-mcp executable |
| `ConnectionTimeoutSeconds` | int | `30` | Maximum time to wait for connection establishment |
| `RequestTimeoutSeconds` | int | `60` | Maximum time to wait for request completion |
| `MaxRetryAttempts` | int | `3` | Number of retry attempts for transient failures |
| `BackoffStrategy` | enum | `Exponential` | Retry backoff strategy (`Linear` or `Exponential`) |
| `InitialRetryDelayMs` | int | `1000` | Initial delay before first retry in milliseconds |
| `EnableProcessPooling` | bool | `true` | Reuse godot-mcp process across requests |
| `MaxIdleTimeSeconds` | int | `300` | Maximum idle time before process shutdown |
| `ToolDefinitionsPath` | string? | `null` | Path to predefined tool definitions (fallback) |
| `EnableMessageLogging` | bool | `false` | Enable detailed request/response logging |

### Backoff Strategies

- **Linear**: Delay = InitialRetryDelayMs × attempt number
- **Exponential**: Delay = InitialRetryDelayMs × 2^(attempt number)

## Available Functions

The plugin automatically discovers available Godot functions from the godot-mcp-Server. Common functions include:

### Scene Management
- `Godot_create_scene` - Create a new Godot scene
- `Godot_load_scene` - Load an existing scene
- `Godot_save_scene` - Save the current scene

### GameObject Operations
- `Godot_create_gameobject` - Create a new GameObject
- `Godot_delete_gameobject` - Delete a GameObject
- `Godot_find_gameobject` - Find GameObject by name or tag

### Asset Management
- `Godot_import_asset` - Import an asset into the project
- `Godot_create_material` - Create a new material
- `Godot_create_prefab` - Create a prefab from GameObject

### Project Information
- `Godot_get_project_info` - Get Godot project information
- `Godot_list_scenes` - List all scenes in the project

**Note**: The exact list of available functions depends on the godot-mcp-Server version. Use `ListToolsAsync()` to discover all available functions at runtime.

## Advanced Usage

### Custom Type Converters

Register custom converters for specialized types:

```csharp
var parameterConverter = host.Services.GetRequiredService<IParameterConverter>();

parameterConverter.RegisterConverter(new MyCustomTypeConverter());

public class MyCustomTypeConverter : ITypeConverter<MyCustomType>
{
    public object? ToMcp(MyCustomType value)
    {
        // Convert to MCP format
        return new { /* ... */ };
    }

    public MyCustomType? FromMcp(object? mcpValue)
    {
        // Convert from MCP format
        return new MyCustomType(/* ... */);
    }
}
```

### Error Handling

The plugin provides a comprehensive exception hierarchy:

```csharp
try
{
    await plugin.InvokeToolAsync("Godot_create_scene", parameters);
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Request timed out after {ex.Timeout}");
}
catch (McpServerException ex)
{
    Console.WriteLine($"Godot server error: {ex.Message} (Code: {ex.ErrorCode})");
}
catch (NetworkException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (ProtocolException ex)
{
    Console.WriteLine($"Protocol violation: {ex.Message}");
}
catch (GodotMcpException ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
```

### Health Monitoring

Check connection health:

```csharp
var mcpClient = host.Services.GetRequiredService<IMcpClient>();

bool isHealthy = await mcpClient.PingAsync();
Console.WriteLine($"Connection healthy: {isHealthy}");

var state = mcpClient.State;
Console.WriteLine($"Connection state: {state}");
```

### Graceful Degradation

Use predefined tool definitions as fallback:

```json
{
  "GodotMcp": {
    "ToolDefinitionsPath": "Godot-tools.json"
  }
}
```

Create `Godot-tools.json`:

```json
[
  {
    "name": "Godot_create_scene",
    "description": "Creates a new Godot scene",
    "parameters": {
      "sceneName": {
        "name": "sceneName",
        "type": "string",
        "description": "Name of the scene",
        "required": true
      }
    }
  }
]
```

## Project Structure

```
Godot-MCP-SK-Plugin/
├── src/
│   ├── GodotMcp.Core/              # Domain models, interfaces, and exceptions
│   │   ├── Interfaces/             # Core abstractions (IMcpClient, IParameterConverter, etc.)
│   │   ├── Models/                 # DTOs and domain models (McpRequest, McpResponse, etc.)
│   │   ├── Exceptions/             # Custom exception types
│   │   └── Utilities/              # Helper utilities (LogSanitizer)
│   ├── GodotMcp.Infrastructure/    # MCP client and process management
│   │   ├── Client/                 # StdioMcpClient implementation
│   │   ├── Process/                # ProcessManager for godot-mcp lifecycle
│   │   ├── Serialization/          # JSON-RPC request/response handling
│   │   ├── Conversion/             # Parameter type conversion
│   │   └── Configuration/          # Configuration options and validation
│   └── GodotMcp.Plugin/            # Semantic Kernel integration
│       ├── Mapping/                # FunctionMapper for tool discovery
│       ├── Extensions/             # ServiceCollectionExtensions for DI
│       ├── Validation/             # Input validation
│       └── GodotPlugin.cs          # Main plugin class
└── tests/
    └── GodotMcp.Tests/             # Unit and integration tests
        ├── CoreTests/              # Core layer tests
        ├── InfrastructureTests/    # Infrastructure layer tests
        └── PluginTests/            # Plugin layer tests
```

## Technology Stack

- **.NET 10** (2026 LTS) - Latest long-term support release
- **C# 13** - File-scoped namespaces, collection expressions, required members
- **Microsoft.SemanticKernel** - AI agent framework integration
- **System.Text.Json** - High-performance JSON serialization with source generators
- **xUnit** - Unit testing framework
- **NSubstitute** - Mocking framework for tests
- **FsCheck** - Property-based testing library

## Security

The plugin implements comprehensive security measures:

### Secure Logging

All logging operations automatically sanitize sensitive information:

- **Passwords and Tokens**: Never logged in plain text
- **API Keys**: Automatically redacted from logs
- **Email Addresses**: Replaced with `[EMAIL_REDACTED]`
- **JWT Tokens**: Detected and redacted (eyJ... format)
- **Bearer Tokens**: Removed from authorization headers
- **Connection Strings**: Password fields redacted

The `LogSanitizer` utility provides automatic detection and redaction of sensitive data patterns.

### Input Validation

All parameters are validated before being sent to the godot-mcp server:

- Tool names validated against registered tools
- Parameter types checked for compatibility
- Null values handled appropriately
- Prevents injection attacks and malformed requests

### Error Message Sanitization

Error messages are sanitized to prevent information disclosure about internal system details while maintaining useful diagnostic information.

## Performance

The plugin is optimized for high performance:

- **System.Text.Json Source Generators**: Zero-reflection serialization
- **FrozenDictionary**: Immutable, high-performance lookups for tool definitions
- **PipeReader/PipeWriter**: Efficient stdio communication
- **ValueTask**: Reduced allocations for hot paths
- **LoggerMessage Source Generators**: Zero-allocation structured logging
- **Process Pooling**: Reuse godot-mcp process across requests

## Contributing

Contributions are welcome! Please follow these guidelines:

### Development Setup

1. Clone the repository
2. Install .NET 10 SDK
3. Install godot-mcp-Server: `dotnet tool install -g godot-mcp`
4. Restore dependencies: `dotnet restore`
5. Build the solution: `dotnet build`
6. Run tests: `dotnet test`

### Code Style

- Follow clean architecture principles
- Use file-scoped namespaces
- Enable nullable reference types
- Use collection expressions for initialization
- Add XML documentation comments to all public APIs
- Write unit tests for all new functionality
- Maintain >80% code coverage

### Pull Request Process

1. Create a feature branch from `main`
2. Implement your changes with tests
3. Ensure all tests pass: `dotnet test`
4. Update documentation as needed
5. Submit a pull request with a clear description

### Testing Guidelines

- Write both unit tests and property-based tests
- Use NSubstitute for mocking dependencies
- Test error handling and edge cases
- Verify thread safety for concurrent scenarios
- Include integration tests for end-to-end flows

## Building and Testing

### Build

```bash
# Build entire solution
dotnet build GodotMcp.sln

# Build specific project
dotnet build src/GodotMcp.Plugin/GodotMcp.Plugin.csproj

# Build in Release mode
dotnet build -c Release
```

### Test

```bash
# Run all tests
dotnet test GodotMcp.sln

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/GodotMcp.Tests/GodotMcp.Tests.csproj

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Package

```bash
# Create NuGet package
dotnet pack -c Release

# Package will be created in bin/Release/
```

## Troubleshooting

### Connection Issues

**Problem**: `NetworkException: Failed to connect to godot-mcp server`

**Solutions**:
- Verify godot-mcp is installed: `dotnet tool list -g`
- Check Godot Editor is running
- Increase `ConnectionTimeoutSeconds` in configuration
- Check godot-mcp logs for errors

### Timeout Errors

**Problem**: `TimeoutException: Request timed out`

**Solutions**:
- Increase `RequestTimeoutSeconds` for long-running operations
- Check Godot Editor is responsive
- Verify network connectivity
- Enable `EnableMessageLogging` to debug request/response flow

### Tool Not Found

**Problem**: `GodotMcpException: Tool not found: Godot_xxx`

**Solutions**:
- Verify godot-mcp-Server version supports the tool
- Call `ListToolsAsync()` to see available tools
- Check tool name spelling
- Provide `ToolDefinitionsPath` as fallback

### Process Management Issues

**Problem**: Multiple godot-mcp processes running

**Solutions**:
- Enable `EnableProcessPooling` to reuse processes
- Properly dispose of `GodotPlugin` and services
- Check `MaxIdleTimeSeconds` configuration
- Manually kill orphaned processes

## License

MIT License - See LICENSE file for details

## Acknowledgments

- Built on [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- Integrates with [godot-mcp-Server](https://github.com/Ozymandros/godot-mcp-Server)
- Follows [Model Context Protocol](https://modelcontextprotocol.io/) specification

## Support

- **Issues**: Report bugs and feature requests on GitHub Issues
- **Discussions**: Join commGodot discussions on GitHub Discussions
- **Documentation**: Full API documentation available in XML comments

## Version History

### 1.0.0 (Initial Release)
- Stdio-based MCP communication
- Automatic tool discovery
- Type-safe parameter conversion
- Retry logic with configurable backoff
- Health monitoring
- Secure logging
- Comprehensive test coverage (>80%)
- Clean architecture implementation
