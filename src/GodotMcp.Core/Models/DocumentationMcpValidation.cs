namespace GodotMcp.Core.Models;

/// <summary>
/// Validation and normalization for Godot MCP documentation tool parameters (client-side).
/// </summary>
public static class DocumentationMcpValidation
{
    public const string SourceManifest = "manifest";
    public const string SourceMarkdown = "markdown";
    public const string SourceBoth = "both";

    public const int EngineMaxResultsMin = 1;
    public const int EngineMaxResultsMax = 40;
    public const int EngineVersionMaxLength = 64;

    /// <summary>
    /// Normalizes <paramref name="source"/> to one of manifest, markdown, or both (case-insensitive).
    /// </summary>
    /// <exception cref="ArgumentException">When the value is not a supported source mode.</exception>
    public static string NormalizeSource(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return SourceBoth;
        }

        var s = source.Trim().ToLowerInvariant();
        return s switch
        {
            SourceManifest or SourceMarkdown or SourceBoth => s,
            _ => throw new ArgumentException(
                $"source must be '{SourceManifest}', '{SourceMarkdown}', or '{SourceBoth}'.",
                nameof(source))
        };
    }

    /// <summary>
    /// Ensures the engine documentation query is non-empty after trim.
    /// </summary>
    /// <exception cref="ArgumentException">When query is null, empty, or whitespace.</exception>
    public static string ValidateEngineQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("query is required for engine documentation search.", nameof(query));
        }

        return query.Trim();
    }

    /// <summary>
    /// Normalizes RTD version label: default stable, max length, no URL schemes.
    /// </summary>
    /// <exception cref="ArgumentException">When the version string is invalid.</exception>
    public static string NormalizeEngineVersion(string? version)
    {
        var v = string.IsNullOrWhiteSpace(version) ? "stable" : version.Trim();
        if (v.Length > EngineVersionMaxLength)
        {
            throw new ArgumentException($"version must be at most {EngineVersionMaxLength} characters.", nameof(version));
        }

        if (v.Contains("://", StringComparison.Ordinal))
        {
            throw new ArgumentException("version must not contain a URL scheme (e.g. no '://').", nameof(version));
        }

        return v;
    }

    /// <summary>
    /// Clamps max_results to the server-supported range (1–40).
    /// </summary>
    public static int ClampEngineMaxResults(int maxResults)
    {
        if (maxResults < EngineMaxResultsMin)
        {
            return EngineMaxResultsMin;
        }

        return maxResults > EngineMaxResultsMax ? EngineMaxResultsMax : maxResults;
    }
}
