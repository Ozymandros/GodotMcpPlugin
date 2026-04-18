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
        [Description("Scene file path relative to project root (POSIX-style, e.g. scenes/main.tscn).")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneListNodesAsync(new SceneListNodesRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Adds a node to a scene.
    /// </summary>
    [KernelFunction("add_node")]
    [Description("Adds a node to a scene.")]
    public Task<NodeInfo?> AddNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Parent node path.")] string parentNodePath,
        [Description("Node name.")] string nodeName,
        [Description("Node type.")] string nodeType,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneAddNodeAsync(
            new SceneAddNodeRequest(new McpProjectFile(projectPath, fileName), parentNodePath, nodeName, nodeType),
            cancellationToken);

    /// <summary>
    /// Removes a node from a scene.
    /// </summary>
    [KernelFunction("remove_node")]
    [Description("Removes a node from a scene.")]
    public Task<SceneCommandResult?> RemoveNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Node path to remove.")] string nodePath,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneRemoveNodeAsync(new SceneRemoveNodeRequest(new McpProjectFile(projectPath, fileName), nodePath), cancellationToken);

    /// <summary>
    /// Moves a node in a scene.
    /// </summary>
    [KernelFunction("move_node")]
    [Description("Moves a node in a scene.")]
    public Task<SceneCommandResult?> MoveNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Node path to move.")] string nodePath,
        [Description("New parent node path.")] string newParentPath,
        [Description("Optional sibling index under the new parent.")] int? index = null,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneMoveNodeAsync(
            new SceneMoveNodeRequest(new McpProjectFile(projectPath, fileName), nodePath, newParentPath, index),
            cancellationToken);

    /// <summary>
    /// Renames a node in a scene.
    /// </summary>
    [KernelFunction("rename_node")]
    [Description("Renames a node in a scene.")]
    public Task<NodeInfo?> RenameNodeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Node path.")] string nodePath,
        [Description("New node name.")] string newName,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneRenameNodeAsync(new SceneRenameNodeRequest(new McpProjectFile(projectPath, fileName), nodePath, newName), cancellationToken);

    /// <summary>
    /// Gets scene node properties.
    /// </summary>
    [KernelFunction("get_node_properties")]
    [Description("Gets scene node properties.")]
    public Task<IReadOnlyList<NodePropertyInfo>> GetNodePropertiesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Node path.")] string nodePath,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneGetNodePropertiesAsync(new SceneGetNodePropertiesRequest(new McpProjectFile(projectPath, fileName), nodePath), cancellationToken);

    /// <summary>
    /// Sets scene node properties.
    /// </summary>
    [KernelFunction("set_node_properties")]
    [Description("Sets scene node properties.")]
    public Task<IReadOnlyList<NodePropertyInfo>> SetNodePropertiesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Node path.")] string nodePath,
        [Description("Properties to set.")] IReadOnlyList<NodePropertyInfo> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.SceneSetNodePropertiesAsync(
            new SceneSetNodePropertiesRequest(new McpProjectFile(projectPath, fileName), nodePath, properties),
            cancellationToken);
}
