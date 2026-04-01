namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a Godot callable reference.
/// </summary>
/// <param name="Method">Callable method name.</param>
/// <param name="Target">Optional callable target object identifier.</param>
public readonly record struct CallableReference(string Method, string? Target = null);
