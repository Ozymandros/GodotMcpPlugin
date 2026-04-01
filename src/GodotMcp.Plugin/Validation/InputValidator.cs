using GodotMcp.Core.Exceptions;
using GodotMcp.Core.Models;

namespace GodotMcp.Plugin.Validation;

/// <summary>
/// Validates input parameters and sanitizes error messages for security
/// </summary>
public static class InputValidator
{
    /// <summary>
    /// Validates tool name against registered tools
    /// </summary>
    /// <param name="toolName">The tool name to validate</param>
    /// <param name="registeredTools">Collection of registered tool names</param>
    /// <exception cref="GodotMcpException">Thrown when tool name is invalid</exception>
    public static void ValidateToolName(string toolName, IEnumerable<string> registeredTools)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            throw new GodotMcpException("Tool name cannot be null or empty");
        }

        if (!registeredTools.Contains(toolName))
        {
            throw new GodotMcpException($"Tool '{SanitizeToolName(toolName)}' is not registered");
        }
    }

    /// <summary>
    /// Validates parameters against tool definition
    /// </summary>
    /// <param name="parameters">The parameters to validate</param>
    /// <param name="toolDefinition">The tool definition containing parameter requirements</param>
    /// <exception cref="GodotMcpException">Thrown when parameters are invalid</exception>
    public static void ValidateParameters(
        IReadOnlyDictionary<string, object?> parameters,
        McpToolDefinition toolDefinition)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(toolDefinition);

        // Check for required parameters
        foreach (var paramDef in toolDefinition.Parameters.Values.Where(p => p.Required))
        {
            if (!parameters.ContainsKey(paramDef.Name))
            {
                throw new GodotMcpException($"Required parameter '{SanitizeParameterName(paramDef.Name)}' is missing");
            }

            var value = parameters[paramDef.Name];
            if (value == null)
            {
                throw new GodotMcpException($"Required parameter '{SanitizeParameterName(paramDef.Name)}' cannot be null");
            }
        }

        // Validate parameter types
        foreach (var (paramName, paramValue) in parameters)
        {
            if (paramValue == null)
            {
                continue; // Null values are allowed for optional parameters
            }

            if (!toolDefinition.Parameters.TryGetValue(paramName, out var paramDef))
            {
                throw new GodotMcpException($"Unknown parameter '{SanitizeParameterName(paramName)}'");
            }

            ValidateParameterType(paramName, paramValue, paramDef.Type);
        }
    }

    /// <summary>
    /// Validates a parameter value against its expected type
    /// </summary>
    /// <param name="paramName">The parameter name</param>
    /// <param name="paramValue">The parameter value</param>
    /// <param name="expectedType">The expected MCP type</param>
    /// <exception cref="GodotMcpException">Thrown when type validation fails</exception>
    private static void ValidateParameterType(string paramName, object paramValue, string expectedType)
    {
        var isValid = expectedType.ToLowerInvariant() switch
        {
            "string" => paramValue is string,
            "number" => paramValue is double or float or decimal or int or long,
            "integer" => paramValue is int or long or short or byte,
            "boolean" => paramValue is bool,
            "object" => true, // Objects are always valid
            "array" => paramValue is System.Collections.IEnumerable and not string,
            _ => true // Unknown types are allowed
        };

        if (!isValid)
        {
            throw new GodotMcpException(
                $"Parameter '{SanitizeParameterName(paramName)}' has invalid type. Expected: {expectedType}");
        }
    }

    /// <summary>
    /// Sanitizes error messages to prevent information disclosure
    /// </summary>
    /// <param name="errorMessage">The original error message</param>
    /// <returns>A sanitized error message safe for external consumption</returns>
    public static string SanitizeErrorMessage(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return "An error occurred";
        }

        // Remove connection strings or URLs with credentials (do this first before path removal)
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            errorMessage,
            @"://[^:]+:[^@]+@",
            "://[credentials]@");

        // Remove file paths (Windows style - must start with drive letter)
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"[A-Za-z]:\\[^\s]+",
            "[path]");

        // Remove file paths (Unix style - must not be preceded by :// to avoid matching URLs)
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"(?<!:)/(?:[^\s:/]+/)+[^\s]*",
            "[path]");

        // Remove stack traces
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"at\s+[\w\.<>]+\([^\)]*\)\s+in\s+[^\s]+",
            "[stack trace removed]");

        // Remove internal type names
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"GodotMcp\.\w+\.\w+\.\w+",
            "[internal type]");

        // Limit message length to prevent information leakage through verbose errors
        if (sanitized.Length > 200)
        {
            sanitized = sanitized[..197] + "...";
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes tool name for safe display in error messages
    /// </summary>
    /// <param name="toolName">The tool name to sanitize</param>
    /// <returns>A sanitized tool name</returns>
    private static string SanitizeToolName(string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return "[empty]";
        }

        // Only allow alphanumeric, underscore, hyphen, and dot
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            toolName,
            @"[^a-zA-Z0-9_\-\.]",
            "");

        // Limit length
        if (sanitized.Length > 50)
        {
            sanitized = sanitized[..50];
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "[invalid]" : sanitized;
    }

    /// <summary>
    /// Sanitizes parameter name for safe display in error messages
    /// </summary>
    /// <param name="paramName">The parameter name to sanitize</param>
    /// <returns>A sanitized parameter name</returns>
    private static string SanitizeParameterName(string paramName)
    {
        if (string.IsNullOrWhiteSpace(paramName))
        {
            return "[empty]";
        }

        // Only allow alphanumeric, underscore, and hyphen
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            paramName,
            @"[^a-zA-Z0-9_\-]",
            "");

        // Limit length
        if (sanitized.Length > 50)
        {
            sanitized = sanitized[..50];
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "[invalid]" : sanitized;
    }
}
