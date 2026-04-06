namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot resource item returned by Resource MCP commands.
/// </summary>
/// <param name="Path">Resource path in the Godot project.</param>
/// <param name="Type">Godot resource type name.</param>
/// <param name="Name">Friendly resource name.</param>
/// <param name="Exists">Whether the resource exists.</param>
public sealed record ResourceInfo(
    string Path,
    string Type,
    string Name,
    bool Exists = true);
