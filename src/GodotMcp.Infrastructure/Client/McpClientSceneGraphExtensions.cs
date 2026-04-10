using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed Scene Graph wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientSceneGraphExtensions
{
    private static readonly JsonSerializerOptions TypedResultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
            McpJsonSerializerContext.Default,
            new DefaultJsonTypeInfoResolver())
    };

    /// <summary>
    /// Sends a typed MCP command and deserializes its result to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="command">The MCP command name to invoke.</param>
    /// <param name="parameters">Command parameters serialized as key/value pairs.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The typed command result, or <c>null</c> when the response has no result payload.</returns>
    public static async Task<T?> SendAsync<T>(
        this IMcpClient client,
        string command,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var response = await client.InvokeToolAsync(command, parameters, cancellationToken).ConfigureAwait(false);

        if (response.Result is null)
        {
            return default;
        }

        if (response.Result is T typed)
        {
            return typed;
        }

        if (response.Result is JsonElement element)
        {
            return element.Deserialize<T>(TypedResultJsonOptions);
        }

        var json = JsonSerializer.Serialize(response.Result, TypedResultJsonOptions);
        return JsonSerializer.Deserialize<T>(json, TypedResultJsonOptions);
    }

    /// <summary>
    /// Lists nodes for a scene file.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph list-nodes request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of scene nodes. Returns an empty list when the server payload is empty.</returns>
    public static async Task<IReadOnlyList<NodeInfo>> SceneListNodesAsync(
        this IMcpClient client,
        SceneListNodesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NodeInfo>>(
            "scene.list_nodes",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NodeInfo>();
    }

    /// <summary>
    /// Adds a node under a parent in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph add-node request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created node information, or <c>null</c> when the server returns no node payload.</returns>
    public static Task<NodeInfo?> SceneAddNodeAsync(
        this IMcpClient client,
        SceneAddNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<NodeInfo>(
            "scene.add_node",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["parentPath"] = request.ParentPath,
                ["nodeName"] = request.NodeName,
                ["nodeType"] = request.NodeType
            },
            cancellationToken);

    /// <summary>
    /// Removes a node from a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph remove-node request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A command result indicating whether the remove operation succeeded.</returns>
    public static Task<SceneCommandResult?> SceneRemoveNodeAsync(
        this IMcpClient client,
        SceneRemoveNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<SceneCommandResult>(
            "scene.remove_node",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath
            },
            cancellationToken);

    /// <summary>
    /// Moves a node to a different parent in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph move-node request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A command result indicating whether the move operation succeeded.</returns>
    public static Task<SceneCommandResult?> SceneMoveNodeAsync(
        this IMcpClient client,
        SceneMoveNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<SceneCommandResult>(
            "scene.move_node",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath,
                ["newParentPath"] = request.NewParentPath,
                ["index"] = request.Index
            },
            cancellationToken);

    /// <summary>
    /// Renames a node in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph rename-node request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated node information, or <c>null</c> when no payload is returned.</returns>
    public static Task<NodeInfo?> SceneRenameNodeAsync(
        this IMcpClient client,
        SceneRenameNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<NodeInfo>(
            "scene.rename_node",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath,
                ["newName"] = request.NewName
            },
            cancellationToken);

    /// <summary>
    /// Gets node properties from a scene node.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph get-node-properties request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of node properties. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<NodePropertyInfo>> SceneGetNodePropertiesAsync(
        this IMcpClient client,
        SceneGetNodePropertiesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NodePropertyInfo>>(
            "scene.get_node_properties",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NodePropertyInfo>();
    }

    /// <summary>
    /// Sets node properties on a scene node.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The Scene Graph set-node-properties request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of resulting node properties. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<NodePropertyInfo>> SceneSetNodePropertiesAsync(
        this IMcpClient client,
        SceneSetNodePropertiesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NodePropertyInfo>>(
            "scene.set_node_properties",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["nodePath"] = request.NodePath,
                ["properties"] = request.Properties
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NodePropertyInfo>();
    }
}
