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
    public static async Task<IReadOnlyList<NodeInfo>> SceneListNodesAsync(
        this IMcpClient client,
        SceneListNodesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NodeInfo>>(
            "scene.list_nodes",
            BuildSceneMutationPayload(request.Scene, request.RootType, static _ => { }),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NodeInfo>();
    }

    /// <summary>
    /// Adds a node under a parent in a scene.
    /// </summary>
    public static Task<NodeInfo?> SceneAddNodeAsync(
        this IMcpClient client,
        SceneAddNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<NodeInfo>(
            "scene.add_node",
            BuildSceneMutationPayload(request.Scene, request.RootType, d =>
            {
                d["parentNodePath"] = request.ParentNodePath;
                d["nodeName"] = request.NodeName;
                d["nodeType"] = request.NodeType;
            }),
            cancellationToken);

    /// <summary>
    /// Removes a node from a scene.
    /// </summary>
    public static Task<SceneCommandResult?> SceneRemoveNodeAsync(
        this IMcpClient client,
        SceneRemoveNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<SceneCommandResult>(
            "scene.remove_node",
            BuildSceneMutationPayload(request.Scene, request.RootType, d => d["nodePath"] = request.NodePath),
            cancellationToken);

    /// <summary>
    /// Moves a node to a different parent in a scene.
    /// </summary>
    public static Task<SceneCommandResult?> SceneMoveNodeAsync(
        this IMcpClient client,
        SceneMoveNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<SceneCommandResult>(
            "scene.move_node",
            BuildSceneMutationPayload(request.Scene, request.RootType, d =>
            {
                d["nodePath"] = request.NodePath;
                d["newParentPath"] = request.NewParentPath;
            }),
            cancellationToken);

    /// <summary>
    /// Renames a node in a scene.
    /// </summary>
    public static Task<NodeInfo?> SceneRenameNodeAsync(
        this IMcpClient client,
        SceneRenameNodeRequest request,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<NodeInfo>(
            "scene.rename_node",
            BuildSceneMutationPayload(request.Scene, request.RootType, d =>
            {
                d["nodePath"] = request.NodePath;
                d["newName"] = request.NewName;
            }),
            cancellationToken);

    /// <summary>
    /// Gets node properties from a scene node.
    /// </summary>
    public static async Task<IReadOnlyList<NodePropertyInfo>> SceneGetNodePropertiesAsync(
        this IMcpClient client,
        SceneGetNodePropertiesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NodePropertyInfo>>(
            "scene.get_node_properties",
            BuildSceneMutationPayload(request.Scene, request.RootType, d => d["nodePath"] = request.NodePath),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NodePropertyInfo>();
    }

    /// <summary>
    /// Sets node properties on a scene node.
    /// </summary>
    public static async Task<IReadOnlyList<NodePropertyInfo>> SceneSetNodePropertiesAsync(
        this IMcpClient client,
        SceneSetNodePropertiesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<NodePropertyInfo>>(
            "scene.set_node_properties",
            BuildSceneMutationPayload(request.Scene, request.RootType, d =>
            {
                d["nodePath"] = request.NodePath;
                d["properties"] = request.Properties;
            }),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<NodePropertyInfo>();
    }

    private static Dictionary<string, object?> BuildSceneMutationPayload(
        McpProjectFile scene,
        string? rootType,
        Action<Dictionary<string, object?>> addFields)
    {
        var d = McpProjectFilePayload.ToDictionary(scene);
        if (!string.IsNullOrWhiteSpace(rootType))
        {
            d["root_type"] = rootType;
        }
        addFields(d);
        return d;
    }
}
