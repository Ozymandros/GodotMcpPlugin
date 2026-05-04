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

        // Remember whether the original input used a leading backslash (Windows-style Godot path).
        // We need this before normalization, because on Linux '\' becomes '/' after the replace below,
        // making it indistinguishable from a true Unix absolute path.
        var hadLeadingBackslash = input.Length > 0 && input[0] == '\\';

        // Normalize separators to the OS-native separator for consistent cross-platform processing.
        // Explicitly handle '\' in addition to Path.AltDirectorySeparatorChar so that Windows-style
        // paths work correctly on Linux (where AltDirectorySeparatorChar == DirectorySeparatorChar == '/').
        input = input.Replace('\\', Path.DirectorySeparatorChar)
                     .Replace('/', Path.DirectorySeparatorChar);

        // Strip a leading directory separator when the path is not truly absolute:
        //   • On Windows a path like \scenes has no drive letter and is not fully qualified.
        //   • Cross-platform: a path whose original input started with '\' is a Windows-style
        //     project-relative path; after normalization it may start with '/' on Linux, but it
        //     should still be treated as relative to the project root.
        if (input.Length > 0
            && input[0] == Path.DirectorySeparatorChar
            && (hadLeadingBackslash || !Path.IsPathFullyQualified(input)))
        {
            input = input.TrimStart(Path.DirectorySeparatorChar);
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

    public static string NormalizeSceneFileName(string projectRoot, string fileName)
    {
        var normalized = NormalizeProjectRelativeFileName(projectRoot, fileName);
        var relativeToScenes = normalized.StartsWith("scenes/", StringComparison.OrdinalIgnoreCase)
            ? normalized["scenes/".Length..]
            : normalized;

        if (string.IsNullOrWhiteSpace(relativeToScenes))
        {
            throw new ArgumentException("Scene file name cannot be empty.", nameof(fileName));
        }

        if (!relativeToScenes.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Scene file name must end with .tscn.", nameof(fileName));
        }

        return $"scenes/{relativeToScenes}";
    }


    private static bool IsWithinRoot(string normalizedRoot, string fullCandidate)
    {
        var relative = Path.GetRelativePath(normalizedRoot, fullCandidate);
        var parts = relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 && parts.All(static p => p != "..");
    }
}
