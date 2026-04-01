namespace GodotMcp.Core.Models;

/// <summary>
/// Represents Godot project information including metadata and assets
/// </summary>
/// <param name="ProjectPath">The file system path to the Godot project</param>
/// <param name="ProjectName">The name of the Godot project</param>
/// <param name="GodotVersion">The Godot Editor version (e.g., "2023.2.1f1")</param>
/// <param name="Scenes">Collection of scene paths in the project</param>
/// <param name="Packages">Collection of installed package names</param>
public sealed record ProjectInfo(
    string ProjectPath,
    string ProjectName,
    string GodotVersion,
    IReadOnlyList<string> Scenes,
    IReadOnlyList<string> Packages);
