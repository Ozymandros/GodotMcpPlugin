namespace GodotMcp.Core.Models;

/// <summary>
/// Scene Graph request to list nodes.
/// </summary>
/// <param name="ScenePath">The scene resource path (for example: <c>res://scenes/main.tscn</c>).</param>
public sealed record SceneListNodesRequest(string ScenePath);

/// <summary>
/// Scene Graph request to add a node.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ParentPath">The parent node path where the new node will be added.</param>
/// <param name="NodeName">The name to assign to the new node.</param>
/// <param name="NodeType">The Godot node type to instantiate.</param>
public sealed record SceneAddNodeRequest(
    string ScenePath,
    string ParentPath,
    string NodeName,
    string NodeType);

/// <summary>
/// Scene Graph request to remove a node.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="NodePath">The node path to remove.</param>
public sealed record SceneRemoveNodeRequest(
    string ScenePath,
    string NodePath);

/// <summary>
/// Scene Graph request to move a node.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="NodePath">The node path to move.</param>
/// <param name="NewParentPath">The destination parent node path.</param>
/// <param name="Index">Optional sibling index under the destination parent.</param>
public sealed record SceneMoveNodeRequest(
    string ScenePath,
    string NodePath,
    string NewParentPath,
    int? Index = null);

/// <summary>
/// Scene Graph request to rename a node.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="NodePath">The node path to rename.</param>
/// <param name="NewName">The new node name.</param>
public sealed record SceneRenameNodeRequest(
    string ScenePath,
    string NodePath,
    string NewName);

/// <summary>
/// Scene Graph request to get node properties.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="NodePath">The node path whose properties will be read.</param>
public sealed record SceneGetNodePropertiesRequest(
    string ScenePath,
    string NodePath);

/// <summary>
/// Scene Graph request to set node properties.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="NodePath">The node path whose properties will be updated.</param>
/// <param name="Properties">The properties to apply to the target node.</param>
public sealed record SceneSetNodePropertiesRequest(
    string ScenePath,
    string NodePath,
    IReadOnlyList<NodePropertyInfo> Properties);
