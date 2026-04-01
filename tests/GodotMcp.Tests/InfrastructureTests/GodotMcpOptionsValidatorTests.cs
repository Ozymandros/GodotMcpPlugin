using Microsoft.Extensions.Options;
using GodotMcp.Infrastructure.Configuration;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for GodotMcpOptionsValidator
/// </summary>
public sealed class GodotMcpOptionsValidatorTests
{
    private readonly GodotMcpOptionsValidator _validator = new();

    [Fact]
    public void Validate_WithValidOptions_ReturnsSuccess()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WithNullExecutablePath_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = null!,
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ExecutablePath", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithEmptyExecutablePath_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ExecutablePath", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceExecutablePath_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "   ",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ExecutablePath", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithZeroConnectionTimeout_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 0,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ConnectionTimeoutSeconds", result.FailureMessage);
        Assert.Contains("positive", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithNegativeConnectionTimeout_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = -10,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ConnectionTimeoutSeconds", result.FailureMessage);
        Assert.Contains("positive", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithZeroRequestTimeout_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 0,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("RequestTimeoutSeconds", result.FailureMessage);
        Assert.Contains("positive", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithNegativeRequestTimeout_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = -5,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("RequestTimeoutSeconds", result.FailureMessage);
        Assert.Contains("positive", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithNegativeMaxRetryAttempts_ReturnsFail()
    {
        // Arrange
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = -1,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("MaxRetryAttempts", result.FailureMessage);
        Assert.Contains("negative", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithZeroMaxRetryAttempts_ReturnsSuccess()
    {
        // Arrange - Zero is valid (no retries)
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WithNullToolDefinitionsPath_ReturnsSuccess()
    {
        // Arrange - ToolDefinitionsPath is optional
        var options = new GodotMcpOptions
        {
            ExecutablePath = "godot-mcp",
            ConnectionTimeoutSeconds = 30,
            RequestTimeoutSeconds = 60,
            MaxRetryAttempts = 3,
            BackoffStrategy = BackoffStrategy.Exponential,
            InitialRetryDelayMs = 1000,
            EnableProcessPooling = true,
            MaxIdleTimeSeconds = 300,
            ToolDefinitionsPath = null,
            EnableMessageLogging = false
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WithAllEdgeCaseValues_ReturnsSuccess()
    {
        // Arrange - Test with minimum valid values
        var options = new GodotMcpOptions
        {
            ExecutablePath = "x",
            ConnectionTimeoutSeconds = 1,
            RequestTimeoutSeconds = 1,
            MaxRetryAttempts = 0,
            BackoffStrategy = BackoffStrategy.Linear,
            InitialRetryDelayMs = 1,
            EnableProcessPooling = false,
            MaxIdleTimeSeconds = 1,
            EnableMessageLogging = true
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }
}
