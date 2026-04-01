namespace GodotMcp.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the Godot MCP plugin
/// </summary>
public sealed class GodotMcpOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "GodotMcp";
    
    /// <summary>
    /// Path to the godot-mcp executable (default: "godot-mcp")
    /// </summary>
    public required string ExecutablePath { get; set; } = "godot-mcp";

    /// <summary>
    /// Optional override path for the Godot executable that the MCP server uses for headless/editor commands.
    /// When set, this value is forwarded as the GODOT_PATH environment variable to the godot-mcp process.
    /// </summary>
    public string? GodotExecutablePath { get; set; }
    
    /// <summary>
    /// Connection timeout in seconds (default: 30)
    /// </summary>
    public required int ConnectionTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Request timeout in seconds (default: 60)
    /// </summary>
    public required int RequestTimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Maximum retry attempts (default: 3)
    /// </summary>
    public required int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Retry backoff strategy (default: Exponential)
    /// </summary>
    public required BackoffStrategy BackoffStrategy { get; set; } = BackoffStrategy.Exponential;
    
    /// <summary>
    /// Initial retry delay in milliseconds (default: 1000)
    /// </summary>
    public required int InitialRetryDelayMs { get; set; } = 1000;
    
    /// <summary>
    /// Enable process pooling for reuse (default: true)
    /// </summary>
    public required bool EnableProcessPooling { get; set; } = true;
    
    /// <summary>
    /// Maximum idle time before process shutdown in seconds (default: 300)
    /// </summary>
    public required int MaxIdleTimeSeconds { get; set; } = 300;
    
    /// <summary>
    /// Path to predefined tool definitions file (optional)
    /// </summary>
    public string? ToolDefinitionsPath { get; set; }
    
    /// <summary>
    /// Enable request/response logging (default: false)
    /// </summary>
    public required bool EnableMessageLogging { get; set; } = false;
}

/// <summary>
/// Retry backoff strategy enumeration
/// </summary>
public enum BackoffStrategy
{
    /// <summary>
    /// Linear backoff: delay increases linearly with each retry
    /// </summary>
    Linear,
    
    /// <summary>
    /// Exponential backoff: delay doubles with each retry
    /// </summary>
    Exponential
}
