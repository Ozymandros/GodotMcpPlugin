using System.Text.Json;

namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a single node property entry returned by Scene Graph MCP commands.
/// </summary>
/// <param name="Name">Property name.</param>
/// <param name="Type">Property type name.</param>
/// <param name="Value">Property value.</param>
/// <param name="ReadOnly">Whether the property is read-only.</param>
public sealed record NodePropertyInfo(
    string Name,
    string Type,
    JsonElement Value,
    bool ReadOnly = false);
