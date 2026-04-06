namespace GodotMcp.Core.Models;

/// <summary>
/// Represents a navigation region returned by Navigation MCP commands.
/// </summary>
/// <param name="Name">Navigation region name.</param>
/// <param name="Path">Navigation region path.</param>
/// <param name="Enabled">Whether the region is enabled.</param>
public sealed record NavigationRegionInfo(
    string Name,
    string Path,
    bool Enabled = true);
