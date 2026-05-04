namespace GodotMcp.Core.Models;

/// <summary>
/// Scene Graph request to list nodes.
/// </summary>
public sealed record SceneListNodesRequest(McpProjectFile Scene, string? RootType = null);

/// <summary>
/// Scene Graph request to add a node.
/// </summary>
public sealed record SceneAddNodeRequest(
    McpProjectFile Scene,
    string ParentNodePath,
    string NodeName,
    string NodeType,
    string? RootType = null);

/// <summary>
/// Scene Graph request to remove a node.
/// </summary>
public sealed record SceneRemoveNodeRequest(McpProjectFile Scene, string NodePath, string? RootType = null);

/// <summary>
/// Scene Graph request to move a node.
/// </summary>
public sealed record SceneMoveNodeRequest(
    McpProjectFile Scene,
    string NodePath,
    string NewParentPath,
    int? Index = null,
    string? RootType = null);

/// <summary>
/// Scene Graph request to rename a node.
/// </summary>
public sealed record SceneRenameNodeRequest(McpProjectFile Scene, string NodePath, string NewName, string? RootType = null);

/// <summary>
/// Scene Graph request to get node properties.
/// </summary>
public sealed record SceneGetNodePropertiesRequest(McpProjectFile Scene, string NodePath, string? RootType = null);

/// <summary>
/// Scene Graph request to set node properties.
/// </summary>
public sealed record SceneSetNodePropertiesRequest(
    McpProjectFile Scene,
    string NodePath,
    IReadOnlyList<NodePropertyInfo> Properties,
    string? RootType = null);

/// <summary>
/// Scene Graph request to add a signal connection.
/// </summary>
public sealed record SceneConnectionAddRequest(
    McpProjectFile Scene,
    string NodePath,
    string Signal,
    string TargetNodePath,
    string Method,
    bool Connected = true,
    int? Flags = null,
    string? RootType = null);

/// <summary>
/// Scene Graph request to remove a signal connection.
/// </summary>
public sealed record SceneConnectionRemoveRequest(
    McpProjectFile Scene,
    string NodePath,
    string Signal,
    string TargetNodePath,
    string Method,
    string? RootType = null);

/// <summary>
/// Scene Graph request to query signal connections.
/// </summary>
public sealed record SceneConnectionInfoRequest(
    McpProjectFile Scene,
    string? NodePath = null,
    string? RootType = null);
