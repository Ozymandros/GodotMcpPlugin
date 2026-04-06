using GodotMcp.Core.Exceptions;
using GodotMcp.Core.Models;
using GodotMcp.Plugin.Validation;
using System.Text.Json;

namespace GodotMcp.Tests.PluginTests;

/// <summary>
/// Unit tests for InputValidator
/// </summary>
public class InputValidatorTests
{
    [Fact]
    public void ValidateToolName_WithValidToolName_DoesNotThrow()
    {
        // Arrange
        var toolName = "Godot_create_scene";
        var registeredTools = new[] { "Godot_create_scene", "Godot_delete_object" };

        // Act & Assert
        var exception = Record.Exception(() => InputValidator.ValidateToolName(toolName, registeredTools));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateToolName_WithNullToolName_ThrowsException()
    {
        // Arrange
        string? toolName = null;
        var registeredTools = new[] { "Godot_create_scene" };

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateToolName(toolName!, registeredTools));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public void ValidateToolName_WithEmptyToolName_ThrowsException()
    {
        // Arrange
        var toolName = "";
        var registeredTools = new[] { "Godot_create_scene" };

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateToolName(toolName, registeredTools));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public void ValidateToolName_WithUnregisteredTool_ThrowsException()
    {
        // Arrange
        var toolName = "unknown_tool";
        var registeredTools = new[] { "Godot_create_scene" };

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateToolName(toolName, registeredTools));
        Assert.Contains("not registered", exception.Message);
    }

    [Fact]
    public void ValidateParameters_WithValidParameters_DoesNotThrow()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "name", "TestScene" },
            { "count", 5 }
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) },
                { "count", new McpParameterDefinition("count", "integer", Required: false) }
            });

        // Act & Assert
        var exception = Record.Exception(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParameters_WithMissingRequiredParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "count", 5 }
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) },
                { "count", new McpParameterDefinition("count", "integer", Required: false) }
            });

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Contains("Required parameter", exception.Message);
        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public void ValidateParameters_WithNullRequiredParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "name", null }
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) }
            });

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Contains("Required parameter", exception.Message);
        Assert.Contains("cannot be null", exception.Message);
    }

    [Fact]
    public void ValidateParameters_WithUnknownParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "name", "TestScene" },
            { "unknown", "value" }
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) }
            });

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Contains("Unknown parameter", exception.Message);
    }

    [Fact]
    public void ValidateParameters_WithInvalidStringType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "name", 123 } // Should be string
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "name", new McpParameterDefinition("name", "string", Required: true) }
            });

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Contains("invalid type", exception.Message);
    }

    [Fact]
    public void ValidateParameters_WithInvalidIntegerType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "count", "not a number" } // Should be integer
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "count", new McpParameterDefinition("count", "integer", Required: true) }
            });

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Contains("invalid type", exception.Message);
    }

    [Fact]
    public void ValidateParameters_WithValidNumberTypes_DoesNotThrow()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "value", 3.14 }
        };

        var toolDefinition = new McpToolDefinition(
            "test_tool",
            "Test tool",
            new Dictionary<string, McpParameterDefinition>
            {
                { "value", new McpParameterDefinition("value", "number", Required: true) }
            });

        // Act & Assert
        var exception = Record.Exception(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParameters_WithJsonElementNumber_ForNumberType_DoesNotThrow()
    {
        // Arrange
        using var doc = JsonDocument.Parse("3.14");
        var parameters = new Dictionary<string, object?>
        {
            { "fov", doc.RootElement.Clone() }
        };

        var toolDefinition = new McpToolDefinition(
            "set_camera",
            "Sets camera properties",
            new Dictionary<string, McpParameterDefinition>
            {
                { "fov", new McpParameterDefinition("fov", "number", Required: true) }
            });

        // Act & Assert
        var exception = Record.Exception(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParameters_WithJsonElementArray_ForArrayType_DoesNotThrow()
    {
        // Arrange
        using var doc = JsonDocument.Parse("[0, 1, 2]");
        var parameters = new Dictionary<string, object?>
        {
            { "layers", doc.RootElement.Clone() }
        };

        var toolDefinition = new McpToolDefinition(
            "set_camera",
            "Sets camera properties",
            new Dictionary<string, McpParameterDefinition>
            {
                { "layers", new McpParameterDefinition("layers", "array", Required: true) }
            });

        // Act & Assert
        var exception = Record.Exception(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParameters_WithUnionType_AcceptsEitherCandidate()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "clip", "0.3" }
        };

        var toolDefinition = new McpToolDefinition(
            "set_camera",
            "Sets camera clipping plane",
            new Dictionary<string, McpParameterDefinition>
            {
                { "clip", new McpParameterDefinition("clip", "number|string", Required: true) }
            });

        // Act & Assert
        var exception = Record.Exception(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateParameters_WithUnionType_RejectsInvalidCandidate()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            { "clip", new { value = "oops" } }
        };

        var toolDefinition = new McpToolDefinition(
            "set_camera",
            "Sets camera clipping plane",
            new Dictionary<string, McpParameterDefinition>
            {
                { "clip", new McpParameterDefinition("clip", "number|boolean", Required: true) }
            });

        // Act & Assert
        var exception = Assert.Throws<GodotMcpException>(() =>
            InputValidator.ValidateParameters(parameters, toolDefinition));
        Assert.Contains("invalid type", exception.Message);
    }

    [Fact]
    public void SanitizeErrorMessage_RemovesFilePaths()
    {
        // Arrange
        var errorMessage = "Error in C:\\Users\\test\\file.cs at line 10";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.DoesNotContain("C:\\Users\\test\\file.cs", sanitized);
        Assert.Contains("[path]", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_RemovesUnixPaths()
    {
        // Arrange
        var errorMessage = "Error in /home/user/project/file.cs at line 10";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.DoesNotContain("/home/user/project/file.cs", sanitized);
        Assert.Contains("[path]", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_RemovesStackTraces()
    {
        // Arrange
        var errorMessage = "Error at GodotMcp.Plugin.Method() in C:\\file.cs:line 10";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.Contains("[stack trace removed]", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_RemovesInternalTypeNames()
    {
        // Arrange
        var errorMessage = "Error in GodotMcp.Core.Models.McpRequest";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.DoesNotContain("GodotMcp.Core.Models.McpRequest", sanitized);
        Assert.Contains("[internal type]", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_RemovesCredentialsFromUrls()
    {
        // Arrange
        var errorMessage = "Connection failed to https://user:password@server.com";

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.DoesNotContain("user:password", sanitized);
        Assert.Contains("[credentials]", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_TruncatesLongMessages()
    {
        // Arrange
        var errorMessage = new string('a', 300);

        // Act
        var sanitized = InputValidator.SanitizeErrorMessage(errorMessage);

        // Assert
        Assert.True(sanitized.Length <= 200);
        Assert.EndsWith("...", sanitized);
    }

    [Fact]
    public void SanitizeErrorMessage_WithNullOrEmpty_ReturnsGenericMessage()
    {
        // Arrange & Act
        var sanitizedNull = InputValidator.SanitizeErrorMessage(null!);
        var sanitizedEmpty = InputValidator.SanitizeErrorMessage("");

        // Assert
        Assert.Equal("An error occurred", sanitizedNull);
        Assert.Equal("An error occurred", sanitizedEmpty);
    }
}
