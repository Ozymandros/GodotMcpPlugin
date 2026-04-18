using GodotMcp.Core.Models;

namespace GodotMcp.Tests;

/// <summary>
/// Sample absolute paths aligned with <see cref="GodotMcpPathDefaults.DefaultProjectRootPath"/> for tests.
/// </summary>
internal static class TestPaths
{
    public static string Root => GodotMcpPathDefaults.DefaultProjectRootPath;

    /// <summary>Absolute path under the sample project root.</summary>
    public static string Combine(params string[] segments) => Path.Combine([Root, ..segments]);
}
