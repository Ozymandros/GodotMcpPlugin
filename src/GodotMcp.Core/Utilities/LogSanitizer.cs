using System.Text.RegularExpressions;

namespace GodotMcp.Core.Utilities;

/// <summary>
/// Provides utilities for sanitizing log messages to remove sensitive data
/// </summary>
public static partial class LogSanitizer
{
    private static readonly string[] SensitiveKeys = 
    [
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "auth",
        "credential",
        "key",
        "private",
        "passphrase",
        "bearer"
    ];

    private const string RedactedValue = "[REDACTED]";

    /// <summary>
    /// Sanitizes a dictionary of parameters by redacting sensitive values
    /// </summary>
    /// <param name="parameters">The parameters to sanitize</param>
    /// <returns>A new dictionary with sensitive values redacted</returns>
    public static IReadOnlyDictionary<string, object?> SanitizeParameters(
        IReadOnlyDictionary<string, object?>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return parameters ?? new Dictionary<string, object?>();
        }

        var sanitized = new Dictionary<string, object?>();

        foreach (var (key, value) in parameters)
        {
            if (IsSensitiveKey(key))
            {
                sanitized[key] = RedactedValue;
            }
            else if (value is string stringValue)
            {
                sanitized[key] = SanitizeString(stringValue);
            }
            else if (value is IReadOnlyDictionary<string, object?> nestedDict)
            {
                sanitized[key] = SanitizeParameters(nestedDict);
            }
            else
            {
                sanitized[key] = value;
            }
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes a string by redacting patterns that look like sensitive data
    /// </summary>
    /// <param name="value">The string to sanitize</param>
    /// <returns>The sanitized string</returns>
    public static string SanitizeString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Redact JWT tokens (format: xxx.yyy.zzz)
        value = JwtTokenRegex().Replace(value, RedactedValue);

        // Redact Bearer tokens
        value = BearerTokenRegex().Replace(value, "Authorization: Bearer [REDACTED]");

        // Redact API keys (common patterns)
        value = ApiKeyRegex().Replace(value, RedactedValue);

        // Redact email addresses (potential PII)
        value = EmailRegex().Replace(value, "[EMAIL_REDACTED]");

        // Redact potential passwords in connection strings
        value = PasswordInConnectionStringRegex().Replace(value, "Password=[REDACTED]");

        return value;
    }

    /// <summary>
    /// Sanitizes configuration values by redacting sensitive keys
    /// </summary>
    /// <param name="configKey">The configuration key</param>
    /// <param name="configValue">The configuration value</param>
    /// <returns>The sanitized value</returns>
    public static string SanitizeConfigValue(string configKey, string configValue)
    {
        if (string.IsNullOrEmpty(configValue))
        {
            return configValue;
        }

        if (IsSensitiveKey(configKey))
        {
            return RedactedValue;
        }

        return SanitizeString(configValue);
    }

    /// <summary>
    /// Checks if a key name indicates sensitive data
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key is sensitive, false otherwise</returns>
    private static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        var lowerKey = key.ToLowerInvariant();
        
        return SensitiveKeys.Any(sensitiveKey => lowerKey.Contains(sensitiveKey));
    }

    // Regex patterns using source generators for performance
    [GeneratedRegex(@"eyJ[A-Za-z0-9_-]+\.eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+", RegexOptions.Compiled)]
    private static partial Regex JwtTokenRegex();

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"[A-Za-z0-9]{32,}", RegexOptions.Compiled)]
    private static partial Regex ApiKeyRegex();

    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"Password\s*=\s*[^;]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PasswordInConnectionStringRegex();
}
