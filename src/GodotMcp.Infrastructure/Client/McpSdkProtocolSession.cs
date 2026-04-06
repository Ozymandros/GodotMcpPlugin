using GodotMcp.Core.Models;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// MCP protocol session backed by the official <see cref="McpClient"/> (GodotMCP.Server 1.2+ / MCP tools/call).
/// </summary>
internal sealed class McpSdkProtocolSession(McpClient client) : IMcpProtocolSession
{
    private readonly McpClient _client = client;

    public async Task<CallToolResult> CallToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, object?>? arguments = parameters.Count == 0
            ? null
            : parameters;
        return await _client.CallToolAsync(toolName, arguments, null, null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<McpToolDefinition>> ListToolsAsync(CancellationToken cancellationToken)
    {
        var tools = await _client.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return tools.Select(GodotMcpToolDefinitionMapper.FromClientTool).ToList();
    }

    public async Task PingAsync(CancellationToken cancellationToken) =>
        await _client.PingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}
