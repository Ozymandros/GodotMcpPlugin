namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot node path such as "Level/Player".
/// </summary>
/// <param name="Value">The node path value.</param>
public readonly record struct NodePath(string Value)
{
    /// <summary>
    /// Returns the node path as a string.
    /// </summary>
    public override string ToString() => Value;
}
