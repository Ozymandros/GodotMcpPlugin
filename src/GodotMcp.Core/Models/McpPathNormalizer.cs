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
        var nativeInput = fileName.Trim()
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        // Resolve to a full path while supporting partial paths that may start with a separator.
        var fullCandidate = ResolveFilePathCandidate(normalizedRoot, nativeInput);

        var relative = Path.GetRelativePath(normalizedRoot, fullCandidate);
        var relativeParts = relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        if (relativeParts.Length == 0 || relativeParts.Any(static p => p == ".."))
        {
            throw new ArgumentException(
                $"File path '{fileName}' resolves outside project root '{normalizedRoot}'.",
                nameof(fileName));
        }

        return string.Join('/', relativeParts);
    }

    private static string ResolveFilePathCandidate(string normalizedRoot, string nativeInput)
    {
        if (!Path.IsPathRooted(nativeInput))
        {
            return Path.GetFullPath(Path.Combine(normalizedRoot, nativeInput));
        }

        var rootedCandidate = Path.GetFullPath(nativeInput);
        if (IsWithinRoot(normalizedRoot, rootedCandidate))
        {
            return rootedCandidate;
        }

        // If the path starts with a single separator and points outside the project root,
        // treat it as a partial project-relative input and strip that leading separator.
        var directorySeparator = Path.DirectorySeparatorChar.ToString();
        var isSingleLeadingSeparator =
            nativeInput.StartsWith(directorySeparator, StringComparison.Ordinal)
            && !nativeInput.StartsWith(new string(Path.DirectorySeparatorChar, 2), StringComparison.Ordinal)
            && !(nativeInput.Length >= 2 && nativeInput[1] == ':');

        return isSingleLeadingSeparator
            ? Path.GetFullPath(Path.Combine(normalizedRoot, nativeInput.TrimStart(Path.DirectorySeparatorChar)))
            : rootedCandidate;
    }

    private static bool IsWithinRoot(string normalizedRoot, string fullCandidate)
    {
        var relative = Path.GetRelativePath(normalizedRoot, fullCandidate);
        var parts = relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 && parts.All(static p => p != "..");
    }
}
