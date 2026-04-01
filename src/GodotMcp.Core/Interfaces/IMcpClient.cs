namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Defines the contract for MCP client communication
/// </summary>
public interface IMcpClient : IAsyncDisposable
{
    /// <summary>
    /// Connects to the godot-mcp server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous connection operation</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a tool invocation request
    /// </summary>
    /// <param name="toolName">The name of the tool to invoke</param>
    /// <param name="parameters">The parameters to pass to the tool</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation with the MCP response</returns>
    Task<McpResponse> InvokeToolAsync(
        string toolName, 
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Discovers available tools from the server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation with the list of available tools</returns>
    Task<IReadOnlyList<McpToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the connection is healthy
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation with true if connection is healthy, false otherwise</returns>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current connection state
    /// </summary>
    ConnectionState State { get; }
}
