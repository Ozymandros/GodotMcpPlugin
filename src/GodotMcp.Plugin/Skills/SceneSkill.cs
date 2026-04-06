using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Scene Graph MCP commands.
/// </summary>
public sealed class SceneSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists nodes in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of scene nodes.</returns>
    [KernelFunction("list_nodes")]
    [Description("Lists nodes in a scene.")]
    public Task<IReadOnlyList<NodeInfo>> ListNodesAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneListNodesAsync(new SceneListNodesRequest(scenePath), cancellationToken);

    /// <summary>
    /// Adds a node to a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="parentPath">Parent node path.</param>
    /// <param name="nodeName">Node name.</param>
    /// <param name="nodeType">Node type.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created node, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("add_node")]
    [Description("Adds a node to a scene.")]
    public Task<NodeInfo?> AddNodeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Parent node path.")] string parentPath,
        [Description("Node name.")] string nodeName,
        [Description("Node type.")] string nodeType,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneAddNodeAsync(new SceneAddNodeRequest(scenePath, parentPath, nodeName, nodeType), cancellationToken);

    /// <summary>
    /// Removes a node from a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="nodePath">Node path to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The scene command result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("remove_node")]
    [Description("Removes a node from a scene.")]
    public Task<SceneCommandResult?> RemoveNodeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Node path to remove.")] string nodePath,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneRemoveNodeAsync(new SceneRemoveNodeRequest(scenePath, nodePath), cancellationToken);

    /// <summary>
    /// Moves a node in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="nodePath">Node path to move.</param>
    /// <param name="newParentPath">New parent node path.</param>
    /// <param name="index">Optional sibling index under the new parent.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The scene command result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("move_node")]
    [Description("Moves a node in a scene.")]
    public Task<SceneCommandResult?> MoveNodeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Node path to move.")] string nodePath,
        [Description("New parent node path.")] string newParentPath,
        [Description("Optional sibling index.")] int? index = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneMoveNodeAsync(new SceneMoveNodeRequest(scenePath, nodePath, newParentPath, index), cancellationToken);

    /// <summary>
    /// Renames a node in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="nodePath">Node path.</param>
    /// <param name="newName">New node name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The renamed node, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("rename_node")]
    [Description("Renames a node in a scene.")]
    public Task<NodeInfo?> RenameNodeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Node path.")] string nodePath,
        [Description("New node name.")] string newName,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneRenameNodeAsync(new SceneRenameNodeRequest(scenePath, nodePath, newName), cancellationToken);

    /// <summary>
    /// Gets scene node properties.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="nodePath">Node path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of node properties.</returns>
    [KernelFunction("get_node_properties")]
    [Description("Gets scene node properties.")]
    public Task<IReadOnlyList<NodePropertyInfo>> GetNodePropertiesAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Node path.")] string nodePath,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneGetNodePropertiesAsync(new SceneGetNodePropertiesRequest(scenePath, nodePath), cancellationToken);

    /// <summary>
    /// Sets scene node properties.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="nodePath">Node path.</param>
    /// <param name="properties">Properties to set.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of resulting node properties.</returns>
    [KernelFunction("set_node_properties")]
    [Description("Sets scene node properties.")]
    public Task<IReadOnlyList<NodePropertyInfo>> SetNodePropertiesAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Node path.")] string nodePath,
        [Description("Properties to set.")] IReadOnlyList<NodePropertyInfo> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneSetNodePropertiesAsync(new SceneSetNodePropertiesRequest(scenePath, nodePath, properties), cancellationToken);
}
