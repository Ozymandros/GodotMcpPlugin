using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing Scene Graph MCP commands (GodotMCP.Server: <c>projectPath</c> + <c>fileName</c>).
/// </summary>
public sealed class SceneSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists nodes in a scene.
    /// </summary>
    [KernelFunction("list_nodes")]
    [Description("Lists nodes in a scene.")]
    public Task<IReadOnlyList<NodeInfo>> ListNodesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn or nested/player/main.tscn). Must end with .tscn.")] string fileName,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneListNodesAsync(new SceneListNodesRequest(McpProjectFile.ForScene(projectPath, fileName), rootType), cancellationToken);

    /// <summary>
    /// Adds a node to a scene.
    /// </summary>
    [KernelFunction("add_node")]
    [Description("Adds a node to a scene.")]
    public Task<NodeInfo?> AddNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Parent node path.")] string parentNodePath,
        [Description("Node name.")] string nodeName,
        [Description("Node type.")] string nodeType,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneAddNodeAsync(
            new SceneAddNodeRequest(McpProjectFile.ForScene(projectPath, fileName), parentNodePath, nodeName, nodeType, rootType),
            cancellationToken);

    /// <summary>
    /// Removes a node from a scene.
    /// </summary>
    [KernelFunction("remove_node")]
    [Description("Removes a node from a scene.")]
    public Task<SceneCommandResult?> RemoveNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Node path to remove.")] string nodePath,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneRemoveNodeAsync(new SceneRemoveNodeRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, rootType), cancellationToken);

    /// <summary>
    /// Moves a node in a scene.
    /// </summary>
    [KernelFunction("move_node")]
    [Description("Moves a node in a scene.")]
    public Task<SceneCommandResult?> MoveNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Node path to move.")] string nodePath,
        [Description("New parent node path.")] string newParentPath,
        [Description("Optional sibling index under the new parent.")] int? index = null,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneMoveNodeAsync(
            new SceneMoveNodeRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, newParentPath, index, rootType),
            cancellationToken);

    /// <summary>
    /// Renames a node in a scene.
    /// </summary>
    [KernelFunction("rename_node")]
    [Description("Renames a node in a scene.")]
    public Task<NodeInfo?> RenameNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Node path.")] string nodePath,
        [Description("New node name.")] string newName,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneRenameNodeAsync(new SceneRenameNodeRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, newName, rootType), cancellationToken);

    /// <summary>
    /// Gets scene node properties.
    /// </summary>
    [KernelFunction("get_node_properties")]
    [Description("Gets scene node properties.")]
    public Task<IReadOnlyList<NodePropertyInfo>> GetNodePropertiesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Node path.")] string nodePath,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneGetNodePropertiesAsync(new SceneGetNodePropertiesRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, rootType), cancellationToken);

    /// <summary>
    /// Sets scene node properties.
    /// </summary>
    [KernelFunction("set_node_properties")]
    [Description("Sets scene node properties.")]
    public Task<IReadOnlyList<NodePropertyInfo>> SetNodePropertiesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Node path.")] string nodePath,
        [Description("Properties to set.")] IReadOnlyList<NodePropertyInfo> properties,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneSetNodePropertiesAsync(
            new SceneSetNodePropertiesRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, properties, rootType),
            cancellationToken);

    /// <summary>
    /// Adds a signal connection between nodes in a scene.
    /// </summary>
    [KernelFunction("add_connection")]
    [Description("Adds a signal connection between nodes.")]
    public Task<ProjectOperationResult?> AddConnectionAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Source node path.")] string nodePath,
        [Description("Signal name (e.g. 'pressed').")] string signal,
        [Description("Target node path.")] string targetNodePath,
        [Description("Method name to call.")] string method,
        [Description("Whether to connect (true) or disconnect (false).")] bool connected = true,
        [Description("Optional connection flags.")] int? flags = null,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneConnectionAddAsync(
            new SceneConnectionAddRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, signal, targetNodePath, method, connected, flags, rootType),
            cancellationToken);

    /// <summary>
    /// Removes a signal connection between nodes in a scene.
    /// </summary>
    [KernelFunction("remove_connection")]
    [Description("Removes a signal connection between nodes.")]
    public Task<ProjectOperationResult?> RemoveConnectionAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Source node path.")] string nodePath,
        [Description("Signal name.")] string signal,
        [Description("Target node path.")] string targetNodePath,
        [Description("Method name.")] string method,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneConnectionRemoveAsync(
            new SceneConnectionRemoveRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, signal, targetNodePath, method, rootType),
            cancellationToken);

    /// <summary>
    /// Queries signal connections in a scene.
    /// </summary>
    [KernelFunction("list_connections")]
    [Description("Lists signal connections in a scene.")]
    public Task<IReadOnlyList<SceneConnectionInfo>> ListConnectionsAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file name under scenes/ (e.g. main.tscn). Must end with .tscn.")] string fileName,
        [Description("Optional node path filter.")] string? nodePath = null,
        [Description("Optional root node type used by server bootstrap when the scene file is missing.")] string? rootType = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneConnectionInfoAsync(
            new SceneConnectionInfoRequest(McpProjectFile.ForScene(projectPath, fileName), nodePath, rootType),
            cancellationToken);
}
