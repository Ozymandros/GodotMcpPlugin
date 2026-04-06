namespace GodotMcp.Core.Exceptions;

/// <summary>
/// Thrown when plugin configuration is invalid or missing required values.
/// </summary>
/// <remarks>
/// This exception is raised during plugin initialization when configuration validation fails,
/// such as when required settings are missing, values are out of valid ranges, or configuration
/// format is incorrect. The <see cref="ParameterName"/> property identifies the specific
/// configuration parameter that caused the error.
/// </remarks>
public class ConfigurationException : GodotMcpException
{
    /// <summary>
    /// Gets the name of the configuration parameter that is invalid or missing.
    /// </summary>
    /// <value>
    /// A string identifying the configuration parameter (e.g., "ExecutablePath", "ConnectionTimeoutSeconds"),
    /// or null if not applicable.
    /// </value>
    public string? ParameterName { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message
    /// and optional parameter name.
    /// </summary>
    /// <param name="message">The message that describes the configuration error.</param>
    /// <param name="parameterName">The name of the invalid or missing parameter, or null if not applicable.</param>
    public ConfigurationException(string message, string? parameterName = null)
        : base(message)
    {
        ParameterName = parameterName;
    }
}
