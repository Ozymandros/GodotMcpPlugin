using Microsoft.Extensions.Options;

namespace GodotMcp.Infrastructure.Configuration;

/// <summary>
/// Validates Godot MCP configuration options at startup
/// </summary>
public sealed class GodotMcpOptionsValidator : IValidateOptions<GodotMcpOptions>
{
    /// <summary>
    /// Validates the Godot MCP configuration options
    /// </summary>
    /// <param name="name">The name of the options instance being validated</param>
    /// <param name="options">The options instance to validate</param>
    /// <returns>A validation result indicating success or failure with descriptive error messages</returns>
    public ValidateOptionsResult Validate(string? name, GodotMcpOptions options)
    {
        // Validate ExecutablePath is not null or whitespace
        if (string.IsNullOrWhiteSpace(options.ExecutablePath))
        {
            return ValidateOptionsResult.Fail(
                "ExecutablePath is required and cannot be null or whitespace. " +
                "Please provide a valid path to the godot-mcp executable.");
        }

        if (!string.IsNullOrWhiteSpace(options.GodotExecutablePath)
            && Path.GetInvalidPathChars().Any(options.GodotExecutablePath.Contains))
        {
            return ValidateOptionsResult.Fail(
                "GodotExecutablePath contains invalid path characters.");
        }

        // Validate ConnectionTimeoutSeconds is positive
        if (options.ConnectionTimeoutSeconds <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"ConnectionTimeoutSeconds must be positive. Current value: {options.ConnectionTimeoutSeconds}. " +
                "Please provide a timeout value greater than 0.");
        }

        // Validate RequestTimeoutSeconds is positive
        if (options.RequestTimeoutSeconds <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"RequestTimeoutSeconds must be positive. Current value: {options.RequestTimeoutSeconds}. " +
                "Please provide a timeout value greater than 0.");
        }

        // Validate MaxRetryAttempts is non-negative
        if (options.MaxRetryAttempts < 0)
        {
            return ValidateOptionsResult.Fail(
                $"MaxRetryAttempts cannot be negative. Current value: {options.MaxRetryAttempts}. " +
                "Please provide a value of 0 or greater.");
        }

        // All validations passed
        return ValidateOptionsResult.Success;
    }
}
