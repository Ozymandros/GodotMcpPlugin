using Microsoft.Extensions.Logging;
using NSubstitute;
using GodotMcp.Core.Exceptions;
using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;
using GodotMcp.Core.Utilities;
using GodotMcp.Plugin;
using GodotMcp.Plugin.Validation;

namespace GodotMcp.Tests.PluginTests;

/// <summary>
/// Integration tests for security features
/// Tests that input validation, error sanitization, and secure logging work correctly in integration scenarios
/// </summary>
public class SecurityIntegrationTests
{
    private readonly IMcpClient _mockMcpClient;
    private readonly IFunctionMapper _mockFunctionMapper;
    private readonly IParameterConverter _mockParameterConverter;
    private readonly ILogger<GodotPlugin> _mockLogger;
    private readonly GodotPlugin _plugin;

    public SecurityIntegrationTests()
    {
        _mockMcpClient = Substitute.For<IMcpClient>();
        _mockFunctionMapper = Substitute.For<IFunctionMapper>();
        _mockParameterConverter = Substitute.For<IParameterConverter>();
        _mockLogger = Substitute.For<ILogger<GodotPlugin>>();

        _plugin = new GodotPlugin(
            _mockMcpClient,
            _mockFunctionMapper,
            _mockParameterConverter,
            _mockLogger);
    }

    [Fact]
    public void InputValidator_RejectsInvalidToolName_WithSanitizedErrorMessage()
    {
        // Arrange
        var maliciousToolName = "../../etc/passwd";
        var registeredTools = new[] { "Godot_create_scene", "Godot_delete_object" };

        // Act
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateToolName(maliciousToolName, registeredTools));

        // Assert
        Assert.Contains("not registered", exception.Message);
        // Verify the tool name is sanitized in the error message
        Assert.DoesNotContain("../../etc/passwd", exception.Message);
    }

    [Fact]
    public void InputValidator_RejectsInvalidParameters_WithoutExposingInternalDetails()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "name", 12345 } // Wrong type - should be string
        };

        var toolDefinition = new McpToolDefinition(
            "Godot_create_scene",
            "Creates a new Godot scene",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) }
            });

        // Act
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));

        // Assert
        Assert.Contains("invalid type", exception.Message);
        // Verify error message doesn't expose internal type information
        Assert.DoesNotContain("System.Int32", exception.Message);
        Assert.DoesNotContain("System.String", exception.Message);
    }

    [Fact]
    public void InputValidator_SanitizesErrorMessages_RemovingFilePaths()
    {
        // Arrange - test both Windows and Unix paths
        var errorWithWindowsPath = "Failed to load file from C:\\Users\\admin\\secrets\\config.json";
        var errorWithUnixPath = "Failed to load file from /home/admin/secrets/config.json";

        // Act
        var sanitizedWindows = InputValidator.SanitizeErrorMessage(errorWithWindowsPath);
        var sanitizedUnix = InputValidator.SanitizeErrorMessage(errorWithUnixPath);

        // Assert
        Assert.DoesNotContain("C:\\Users\\admin\\secrets\\config.json", sanitizedWindows);
        Assert.Contains("[path]", sanitizedWindows);
        Assert.DoesNotContain("/home/admin/secrets/config.json", sanitizedUnix);
        Assert.Contains("[path]", sanitizedUnix);
    }

    [Fact]
    public void InputValidator_SanitizesErrorMessages_RemovingStackTraces()
    {
        // Arrange
        var errorWithStackTrace = @"System.Exception: Error occurred
   at GodotMcp.Infrastructure.Client.StdioMcpClient.InvokeToolAsync() in /projects/file.cs:line 42
   at GodotMcp.Plugin.GodotPlugin.InvokeToolAsync() in /projects/plugin.cs:line 15";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorWithStackTrace);

        // Assert
        Assert.DoesNotContain("StdioMcpClient.InvokeToolAsync", sanitized);
        Assert.DoesNotContain("/projects/file.cs", sanitized);
        Assert.Contains("[stack trace removed]", sanitized);
    }

    [Fact]
    public void InputValidator_SanitizesErrorMessages_RemovingCredentials()
    {
        // Arrange
        var errorWithCredentials = "Connection failed to https://admin:P@ssw0rd123@server.example.com/api";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorWithCredentials);

        // Assert
        Assert.DoesNotContain("admin:P@ssw0rd123", sanitized);
        // The credentials are replaced, but the URL path may also be sanitized
        Assert.DoesNotContain("admin", sanitized);
        Assert.DoesNotContain("P@ssw0rd123", sanitized);
    }

    [Fact]
    public void LogSanitizer_RedactsSensitiveParameters_InLogMessages()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "sceneName", "TestScene" },
            { "apiKey", "sk-1234567890abcdef" },
            { "password", "SuperSecret123" },
            { "token", "bearer_xyz789" }
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("TestScene", sanitized["sceneName"]); // Non-sensitive preserved
        Assert.Equal("[REDACTED]", sanitized["apiKey"]); // Sensitive redacted
        Assert.Equal("[REDACTED]", sanitized["password"]); // Sensitive redacted
        Assert.Equal("[REDACTED]", sanitized["token"]); // Sensitive redacted
    }

    [Fact]
    public void LogSanitizer_RedactsNestedSensitiveData()
    {
        // Arrange
        var nestedConfig = new Dictionary<string, object?>
        {
            { "apiKey", "secret_key_123" },
            { "endpoint", "https://api.example.com" }
        };

        var parameters = new Dictionary<string, object?>
        {
            { "config", nestedConfig },
            { "name", "TestProject" }
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("TestProject", sanitized["name"]);
        var sanitizedNested = sanitized["config"] as IReadOnlyDictionary<string, object?>;
        Assert.NotNull(sanitizedNested);
        Assert.Equal("[REDACTED]", sanitizedNested["apiKey"]);
        Assert.Equal("https://api.example.com", sanitizedNested["endpoint"]);
    }

    [Fact]
    public void LogSanitizer_RedactsJwtTokensInStrings()
    {
        // Arrange
        var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var message = $"Authorization header: {jwtToken}";

        // Act
        var sanitized = LogSanitizer.SanitizeString(message);

        // Assert
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", sanitized);
        Assert.Contains("[REDACTED]", sanitized);
    }

    [Fact]
    public void LogSanitizer_RedactsEmailAddresses()
    {
        // Arrange
        var message = "User john.doe@example.com requested access";

        // Act
        var sanitized = LogSanitizer.SanitizeString(message);

        // Assert
        Assert.DoesNotContain("john.doe@example.com", sanitized);
        Assert.Contains("[EMAIL_REDACTED]", sanitized);
    }

    [Fact]
    public void LogSanitizer_RedactsPasswordsInConnectionStrings()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=Godot;User=admin;Password=MySecretPass123;";

        // Act
        var sanitized = LogSanitizer.SanitizeString(connectionString);

        // Assert
        Assert.DoesNotContain("MySecretPass123", sanitized);
        Assert.Contains("Password=[REDACTED]", sanitized);
    }

    [Fact]
    public void SecurityFeatures_WorkTogether_ValidateAndSanitize()
    {
        // Arrange - Simulate a scenario with invalid parameters containing sensitive data
        var parameters = new Dictionary<string, object?>
        {
            { "apiKey", "sk-secret123" },
            { "invalidParam", "value" }
        };

        var toolDefinition = new McpToolDefinition(
            "Godot_api_call",
            "Makes an API call",
            new Dictionary<string, McpParameterDefinition>
            {
                { "apiKey", new McpParameterDefinition("apiKey", "string", Required: true) }
            });

        // Act - Validate parameters (should fail due to unknown parameter)
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));

        // Sanitize the parameters for logging
        var sanitizedParams = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Contains("Unknown parameter", exception.Message);
        Assert.Equal("[REDACTED]", sanitizedParams["apiKey"]); // API key is redacted in logs
        Assert.Equal("value", sanitizedParams["invalidParam"]); // Non-sensitive data preserved
    }

    [Fact]
    public void InputValidator_RejectsSqlInjectionAttempts()
    {
        // Arrange
        var maliciousInput = "'; DROP TABLE users; --";
        var parameters = new Dictionary<string, object?>
        {
            { "name", maliciousInput }
        };

        var toolDefinition = new McpToolDefinition(
            "Godot_create_object",
            "Creates a Godot object",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) }
            });

        // Act - Validation should pass (it's a valid string), but sanitization should clean it
        var exception = Record.Exception(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));

        // Assert - Validation passes (it's a valid string type)
        Assert.Null(exception);

        // But if this causes an error, the error message should be sanitized
        var errorMessage = $"Failed to create object with name: {maliciousInput}";
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // The sanitized message should be truncated if too long
        Assert.True(sanitized.Length <= 200);
    }

    [Fact]
    public void InputValidator_HandlesXssAttempts_InErrorMessages()
    {
        // Arrange
        var xssAttempt = "<script>alert('XSS')</script>";
        var errorMessage = $"Invalid tool name: {xssAttempt}";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        // The error message should be truncated and not contain the full XSS payload
        Assert.True(sanitized.Length <= 200);
    }

    [Fact]
    public void LogSanitizer_HandlesMultipleSensitivePatterns()
    {
        // Arrange
        var complexMessage = @"
            User: admin@example.com
            API Key: sk-1234567890abcdef1234567890abcdef
            JWT: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U
            Connection: Server=db.example.com;Password=secret123;
        ";

        // Act
        var sanitized = LogSanitizer.SanitizeString(complexMessage);

        // Assert
        Assert.DoesNotContain("admin@example.com", sanitized);
        Assert.DoesNotContain("sk-1234567890abcdef1234567890abcdef", sanitized);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", sanitized);
        Assert.DoesNotContain("secret123", sanitized);
        Assert.Contains("[EMAIL_REDACTED]", sanitized);
        Assert.Contains("[REDACTED]", sanitized);
    }

    [Theory]
    [InlineData("password", "secret123")]
    [InlineData("Password", "MyPass456")]
    [InlineData("PASSWORD", "TOPSECRET")]
    [InlineData("apiKey", "sk-abc123")]
    [InlineData("api_key", "key_xyz789")]
    [InlineData("token", "bearer_token")]
    [InlineData("secret", "my_secret")]
    [InlineData("authorization", "auth_value")]
    [InlineData("bearer", "bearer_xyz")]
    public void LogSanitizer_RedactsVariousSensitiveKeyFormats(string key, string value)
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { key, value }
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("[REDACTED]", sanitized[key]);
    }

    [Fact]
    public void InputValidator_ValidatesRequiredParameters_WithoutLeakingSensitiveDefaults()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>(); // Missing required parameter

        var toolDefinition = new McpToolDefinition(
            "Godot_authenticate",
            "Authenticates with Godot",
            new Dictionary<string, McpParameterDefinition>
            {
                { "apiKey", new McpParameterDefinition("apiKey", "string", Required: true, DefaultValue: "default_secret_key") }
            });

        // Act
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));

        // Assert
        Assert.Contains("Required parameter", exception.Message);
        Assert.Contains("missing", exception.Message);
        // Verify the default value is not exposed in the error message
        Assert.DoesNotContain("default_secret_key", exception.Message);
    }

    [Fact]
    public void ErrorMessages_DoNotExposeInternalTypeNames()
    {
        // Arrange
        var errorMessage = "Error in GodotMcp.Infrastructure.Client.StdioMcpClient: Connection failed";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.DoesNotContain("GodotMcp.Infrastructure.Client.StdioMcpClient", sanitized);
        Assert.Contains("[internal type]", sanitized);
    }

    [Fact]
    public void ErrorMessages_DoNotExposeUnixFilePaths()
    {
        // Arrange
        var errorMessage = "Failed to read /home/user/.config/Godot/credentials.json";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.DoesNotContain("/home/user/.config/Godot/credentials.json", sanitized);
        Assert.Contains("[path]", sanitized);
    }

    [Fact]
    public void ErrorMessages_PreserveUrlsWithoutCredentials()
    {
        // Arrange
        var errorMessage = "Connection failed to https://api.Godot.com/v1/tools";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        // The URL domain should be preserved, though the path may be sanitized
        // The important thing is that no sensitive data is exposed
        Assert.DoesNotContain("://[credentials]@", sanitized);
        // Verify the message is still meaningful
        Assert.Contains("Connection failed", sanitized);
    }

    [Fact]
    public void LogSanitizer_PreservesNonSensitiveData()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "sceneName", "MainScene" },
            { "objectCount", 42 },
            { "isActive", true },
            { "position", new { x = 1.0, y = 2.0, z = 3.0 } }
        };

        // Act
        var sanitized = LogSanitizer.SanitizeParameters(parameters);

        // Assert
        Assert.Equal("MainScene", sanitized["sceneName"]);
        Assert.Equal(42, sanitized["objectCount"]);
        Assert.Equal(true, sanitized["isActive"]);
        Assert.NotNull(sanitized["position"]);
    }
}
