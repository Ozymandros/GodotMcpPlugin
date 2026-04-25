using System.Runtime.InteropServices;

namespace GodotMcp.Core.Models;

/// <summary>
/// Provides robust, cross-platform normalization for project roots and project-relative file names.
/// </summary>
internal static class McpPathNormalizer
{
    public static string NormalizeProjectRoot(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            throw new ArgumentException("Project path cannot be null or whitespace.", nameof(projectPath));
        }

        var trimmed = projectPath.Trim();
        var fullPath = Path.GetFullPath(trimmed);
        return Path.TrimEndingDirectorySeparator(fullPath);
    }

    public static string NormalizeProjectRelativeFileName(string projectRoot, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));
        }

        var normalizedRoot = NormalizeProjectRoot(projectRoot);

        // Work on a copy of the incoming path
        var input = fileName.Trim();

        // Normalize separators to the OS-native separator for consistent processing
        input = input.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        // On Windows: if the input starts with a directory separator but does not contain a drive letter,
        // treat it as relative to the project root by trimming leading separators.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            && input.Length > 0
            && (input[0] == Path.DirectorySeparatorChar || input[0] == Path.AltDirectorySeparatorChar)
            && !(input.Length > 1 && input[1] == Path.VolumeSeparatorChar))
        {
            input = input.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // Resolve absolute vs relative
        string fullPath;
        if (Path.IsPathRooted(input) && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                         ? (input.Length > 1 && input[1] == Path.VolumeSeparatorChar)
                                         : input.StartsWith(Path.DirectorySeparatorChar)))
        {
            // An absolute path with drive letter (Windows) or a rooted absolute path (non-Windows)
            fullPath = Path.GetFullPath(input);
        }
        else
        {
            // Relative — combine with project root then normalize
            fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, input));
        }

        // Ensure file is inside project root
        var relative = Path.GetRelativePath(normalizedRoot, fullPath);
        if (relative.StartsWith("..", StringComparison.Ordinal) || (Path.IsPathRooted(relative) && !string.Equals(Path.GetFullPath(Path.Combine(normalizedRoot, relative)), fullPath, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"File path '{fileName}' resolves outside project root '{normalizedRoot}'.", nameof(fileName));
        }

        // Normalize separator style to forward slashes for FileName
        return relative.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }
 

    private static bool IsWithinRoot(string normalizedRoot, string fullCandidate)
    {
        var relative = Path.GetRelativePath(normalizedRoot, fullCandidate);
        var parts = relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 && parts.All(static p => p != "..");
    }
}
