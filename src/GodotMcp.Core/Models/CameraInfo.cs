namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a camera node returned by Camera MCP commands.
/// </summary>
/// <param name="ScenePath">Scene path containing the camera.</param>
/// <param name="NodePath">Node path of the camera in scene hierarchy.</param>
/// <param name="Type">Camera type name.</param>
/// <param name="Fov">Field-of-view value when available.</param>
/// <param name="Size">Orthographic size when available.</param>
/// <param name="Near">Near clipping plane value when available.</param>
/// <param name="Far">Far clipping plane value when available.</param>
/// <param name="Projection">Projection mode string.</param>
/// <param name="Current">Whether this camera is current/active.</param>
public sealed record CameraInfo(
    string ScenePath,
    string NodePath,
    string Type,
    double? Fov,
    double? Size,
    double? Near,
    double? Far,
    string Projection,
    bool Current);
