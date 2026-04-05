using GodotMcp.Core.Models;
using ModelContextProtocol.Protocol;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Abstraction over a connected MCP protocol session (initialize + tools/call + tools/list + ping).
/// Test doubles replace this to keep <see cref="StdioMcpClient"/> unit-testable without spawning a server process.
/// </summary>
public interface IMcpProtocolSession : IAsyncDisposable
{
    Task<CallToolResult> CallToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<McpToolDefinition>> ListToolsAsync(CancellationToken cancellationToken);

    Task PingAsync(CancellationToken cancellationToken);
}
