namespace GodotMcp.Core.Interfaces;

/// <summary>
/// Defines the contract for MCP client communication.
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
    /// Gets the current connection state.
    /// </summary>
    /// <value>The current state of the MCP connection lifecycle.</value>
    ConnectionState State { get; }

    /// <summary>
    /// Updates the active Godot project root used as the server working directory and reconnects
    /// the <c>godot-mcp</c> process when the path has changed.
    /// </summary>
    /// <remarks>
    /// GodotMCP.Server 1.5+ validates every <c>projectPath</c> tool argument against the server's
    /// working directory (CWD). Calling this method before a turn ensures the server process is
    /// scoped to the correct project, so absolute <c>projectPath</c> values pass the boundary check.
    /// If <paramref name="projectRoot"/> is the same as the currently configured path, this is a no-op.
    /// </remarks>
    /// <param name="projectRoot">Absolute path to the Godot project root, or <see langword="null"/> to keep the current configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApplyProjectRootAsync(string? projectRoot, CancellationToken cancellationToken = default);
}
