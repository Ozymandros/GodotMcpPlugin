using GodotMcp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Creates MCP protocol sessions for a configured Godot MCP server process (stdio transport).
/// </summary>
public interface IMcpProtocolClientFactory
{
    Task<IMcpProtocolSession> ConnectAsync(
        GodotMcpOptions options,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken);
}
