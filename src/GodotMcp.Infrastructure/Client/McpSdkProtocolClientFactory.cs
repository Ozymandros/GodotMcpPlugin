using GodotMcp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Connects to <c>godot-mcp</c> via stdio using the official MCP C# client (ModelContextProtocol package).
/// </summary>
public sealed class McpSdkProtocolClientFactory : IMcpProtocolClientFactory
{
    public async Task<IMcpProtocolSession> ConnectAsync(
        GodotMcpOptions options,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var transport = CreateTransport(options);
        var clientOptions = new McpClientOptions
        {
            InitializationTimeout = TimeSpan.FromSeconds(Math.Max(1, options.ConnectionTimeoutSeconds))
        };

        var client = await McpClient.CreateAsync(transport, clientOptions, loggerFactory, cancellationToken)
            .ConfigureAwait(false);
        return new McpSdkProtocolSession(client);
    }

    private static StdioClientTransport CreateTransport(GodotMcpOptions options)
    {
        var command = options.ExecutablePath;
        var pathOverride = Environment.GetEnvironmentVariable("GODOT_MCP_PATH");
        if (!string.IsNullOrWhiteSpace(pathOverride))
        {
            command = pathOverride;
        }

        Dictionary<string, string?>? envVars = null;
        if (!string.IsNullOrWhiteSpace(options.GodotExecutablePath)
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GODOT_PATH")))
        {
            envVars = new Dictionary<string, string?> { ["GODOT_PATH"] = options.GodotExecutablePath };
        }

        // GodotMCP.Server 1.5+ validates every projectPath parameter against its working directory
        // via IPathResolver. Setting WorkingDirectory ensures the server's path sandbox is rooted at
        // the configured Godot project, so absolute projectPath values injected by callers resolve correctly.
        var workingDirectory = string.IsNullOrWhiteSpace(options.ProjectPath)
            ? null
            : options.ProjectPath;

        return new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Godot MCP",
            Command = command,
            WorkingDirectory = workingDirectory,
            EnvironmentVariables = envVars
        });
    }
}
