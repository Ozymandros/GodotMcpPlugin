using GodotMcp.Core.Utilities;

namespace GodotMcp.Tests.CoreTests;

/// <summary>
/// Unit tests for LogSanitizer
/// </summary>
public class LogSanitizerTests
{
    [Fact]
    public void SanitizeParameters_RedactsSensitiveKeys()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["username"] = "testuser",
            ["password"] = "secret123",
            ["apiKey"] = "abc123xyz",
            ["token"] = "bearer_token_here"
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("testuser", sanitized["username"]);
        Assert.Equal("[REDACTED]", sanitized["password"]);
        Assert.Equal("[REDACTED]", sanitized["apiKey"]);
        Assert.Equal("[REDACTED]", sanitized["token"]);
    }

    [Fact]
    public void SanitizeParameters_HandlesNestedDictionaries()
    {
        // Arrange
        var nested = new Dictionary<string, object?>
        {
            ["secret"] = "sensitive_value"
        };

        var parameters = new Dictionary<string, object?>
        {
            ["config"] = nested,
            ["name"] = "test"
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("test", sanitized["name"]);
        var sanitizedNested = sanitized["config"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(sanitizedNested);
        Assert.Equal("[REDACTED]", sanitizedNested["secret"]);
    }

    [Fact]
    public void SanitizeString_RedactsJwtTokens()
    {
        // Arrange
        var input = "Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

        // Act
        var sanitized = LogSanitizer.SanitizeString(input);

        // Assert
        Assert.Contains("[REDACTED]", sanitized);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", sanitized);
    }

    [Fact]
    public void SanitizeString_RedactsBearerTokens()
    {
        // Arrange
        var input = "Authorization: Bearer abc123xyz456";

        // Act
        var sanitized = LogSanitizer.SanitizeString(input);

        // Assert
        Assert.Contains("[REDACTED]", sanitized);
        Assert.DoesNotContain("abc123xyz456", sanitized);
    }

    [Fact]
    public void SanitizeString_RedactsEmailAddresses()
    {
        // Arrange
        var input = "User email is test@example.com for notifications";

        // Act
        var sanitized = LogSanitizer.SanitizeString(input);

        // Assert
        Assert.Contains("[EMAIL_REDACTED]", sanitized);
        Assert.DoesNotContain("test@example.com", sanitized);
    }

    [Fact]
    public void SanitizeString_RedactsPasswordsInConnectionStrings()
    {
        // Arrange
        var input = "Server=localhost;Database=test;User=admin;Password=secret123;";

        // Act
        var sanitized = LogSanitizer.SanitizeString(input);

        // Assert
        Assert.Contains("Password=[REDACTED]", sanitized);
        Assert.DoesNotContain("secret123", sanitized);
    }

    [Fact]
    public void SanitizeString_HandlesNullOrEmpty()
    {
        // Act & Assert
        Assert.Null(LogSanitizer.SanitizeString(null!));
        Assert.Equal(string.Empty, LogSanitizer.SanitizeString(string.Empty));
    }

    [Fact]
    public void SanitizeConfigValue_RedactsSensitiveConfigKeys()
    {
        // Arrange
        var key = "ApiKey";
        var value = "secret_api_key_value";

        // Act
        var sanitized = LogSanitizer.SanitizeConfigValue(key, value);

        // Assert
        Assert.Equal("[REDACTED]", sanitized);
    }

    [Fact]
    public void SanitizeConfigValue_PreservesNonSensitiveValues()
    {
        // Arrange
        var key = "ServerUrl";
        var value = "https://example.com";

        // Act
        var sanitized = LogSanitizer.SanitizeConfigValue(key, value);

        // Assert
        Assert.Equal("https://example.com", sanitized);
    }

    [Fact]
    public void SanitizeParameters_PreservesNonSensitiveData()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "John Doe",
            ["age"] = 30,
            ["active"] = true
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("John Doe", sanitized["name"]);
        Assert.Equal(30, sanitized["age"]);
        Assert.Equal(true, sanitized["active"]);
    }

    [Fact]
    public void SanitizeParameters_HandlesEmptyDictionary()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>();

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Empty(sanitized);
    }

    [Fact]
    public void SanitizeString_RedactsApiKeys()
    {
        // Arrange - API keys are typically long alphanumeric strings (32+ chars)
        var input = "API Key: abcdef1234567890abcdef1234567890";

        // Act
        var sanitized = LogSanitizer.SanitizeString(input);

        // Assert
        Assert.Contains("[REDACTED]", sanitized);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("PASSWORD")]
    [InlineData("api_key")]
    [InlineData("apiKey")]
    [InlineData("token")]
    [InlineData("secret")]
    [InlineData("authorization")]
    [InlineData("bearer")]
    public void SanitizeParameters_RedactsVariousSensitiveKeyFormats(string sensitiveKey)
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            [sensitiveKey] = "sensitive_value"
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("[REDACTED]", sanitized[sensitiveKey]);
    }
}
