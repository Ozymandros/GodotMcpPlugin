namespace GodotMcp.Core.Models;

/// <summary>
/// Scene Graph request to list nodes.
/// </summary>
public sealed record SceneListNodesRequest(McpProjectFile Scene);

/// <summary>
/// Scene Graph request to add a node.
/// </summary>
public sealed record SceneAddNodeRequest(
    McpProjectFile Scene,
    string ParentNodePath,
    string NodeName,
    string NodeType);

/// <summary>
/// Scene Graph request to remove a node.
/// </summary>
public sealed record SceneRemoveNodeRequest(McpProjectFile Scene, string NodePath);

/// <summary>
/// Scene Graph request to move a node.
/// </summary>
public sealed record SceneMoveNodeRequest(
    McpProjectFile Scene,
    string NodePath,
    string NewParentPath,
    int? Index = null);

/// <summary>
/// Scene Graph request to rename a node.
/// </summary>
public sealed record SceneRenameNodeRequest(McpProjectFile Scene, string NodePath, string NewName);

/// <summary>
/// Scene Graph request to get node properties.
/// </summary>
public sealed record SceneGetNodePropertiesRequest(McpProjectFile Scene, string NodePath);

/// <summary>
/// Scene Graph request to set node properties.
/// </summary>
public sealed record SceneSetNodePropertiesRequest(
    McpProjectFile Scene,
    string NodePath,
    IReadOnlyList<NodePropertyInfo> Properties);
