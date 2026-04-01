namespace GodotMcp.Tests.CoreTests;

/// <summary>
/// Unit tests for Core model types
/// </summary>
public class ModelTests
{
    [Fact]
    public void McpRequest_ShouldCreateWithValidParameters()
    {
        // Arrange
        var id = "test-id";
        var method = "test-method";
        var parameters = new Dictionary<string, object?> { ["key"] = "value" };

        // Act
        var request = new McpRequest(id, method, parameters);

        // Assert
        Assert.Equal(id, request.Id);
        Assert.Equal(method, request.Method);
        Assert.Equal(parameters, request.Parameters);
    }

    [Fact]
    public void McpResponse_ShouldCreateSuccessResponse()
    {
        // Arrange
        var id = "test-id";
        var result = new { data = "test" };

        // Act
        var response = new McpResponse(id, true, result);

        // Assert
        Assert.Equal(id, response.Id);
        Assert.True(response.Success);
        Assert.Equal(result, response.Result);
        Assert.Null(response.Error);
    }

    [Fact]
    public void McpResponse_ShouldCreateErrorResponse()
    {
        // Arrange
        var id = "test-id";
        var error = new McpError(500, "Test error");

        // Act
        var response = new McpResponse(id, false, null, error);

        // Assert
        Assert.Equal(id, response.Id);
        Assert.False(response.Success);
        Assert.Null(response.Result);
        Assert.NotNull(response.Error);
        Assert.Equal(500, response.Error.Code);
        Assert.Equal("Test error", response.Error.Message);
    }

    [Fact]
    public void McpError_ShouldCreateWithCodeAndMessage()
    {
        var error = new McpError(-32600, "Invalid Request", "extra data");
        Assert.Equal(-32600, error.Code);
        Assert.Equal("Invalid Request", error.Message);
        Assert.Equal("extra data", error.Data);
    }

    [Fact]
    public void McpError_DefaultData_IsNull()
    {
        var error = new McpError(0, "msg");
        Assert.Null(error.Data);
    }

    [Fact]
    public void McpToolDefinition_ShouldCreateWithRequiredFields()
    {
        var parameters = new Dictionary<string, McpParameterDefinition>
        {
            ["name"] = new McpParameterDefinition("name", "string", Required: true)
        };
        var tool = new McpToolDefinition("Godot_create_scene", "Creates a scene", parameters);

        Assert.Equal("Godot_create_scene", tool.Name);
        Assert.Equal("Creates a scene", tool.Description);
        Assert.Single(tool.Parameters);
        Assert.Null(tool.ReturnType);
    }

    [Fact]
    public void McpToolDefinition_WithReturnType_ShouldStoreReturnType()
    {
        var returnType = new McpReturnType("string", "Scene name");
        var tool = new McpToolDefinition("get_scene", "Gets scene", new Dictionary<string, McpParameterDefinition>(), returnType);

        Assert.NotNull(tool.ReturnType);
        Assert.Equal("string", tool.ReturnType.Type);
        Assert.Equal("Scene name", tool.ReturnType.Description);
    }

    [Fact]
    public void McpParameterDefinition_ShouldCreateWithDefaults()
    {
        var param = new McpParameterDefinition("count", "integer");

        Assert.Equal("count", param.Name);
        Assert.Equal("integer", param.Type);
        Assert.Null(param.Description);
        Assert.False(param.Required);
        Assert.Null(param.DefaultValue);
    }

    [Fact]
    public void McpParameterDefinition_ShouldCreateWithAllFields()
    {
        var param = new McpParameterDefinition("count", "integer", "Item count", Required: true, DefaultValue: 1);

        Assert.Equal("count", param.Name);
        Assert.Equal("integer", param.Type);
        Assert.Equal("Item count", param.Description);
        Assert.True(param.Required);
        Assert.Equal(1, param.DefaultValue);
    }

    [Fact]
    public void McpReturnType_ShouldCreateWithTypeAndDescription()
    {
        var rt = new McpReturnType("object", "Result object");
        Assert.Equal("object", rt.Type);
        Assert.Equal("Result object", rt.Description);
    }

    [Fact]
    public void McpReturnType_DefaultDescription_IsNull()
    {
        var rt = new McpReturnType("string");
        Assert.Null(rt.Description);
    }

    [Fact]
    public void ProcessInfo_ShouldStoreAllFields()
    {
        var startTime = DateTime.UtcNow;
        var info = new ProcessInfo(1234, "godot-mcp", startTime);

        Assert.Equal(1234, info.ProcessId);
        Assert.Equal("godot-mcp", info.ExecutablePath);
        Assert.Equal(startTime, info.StartTime);
    }

    [Fact]
    public void ProcessState_ShouldHaveExpectedValues()
    {
        Assert.Equal(0, (int)ProcessState.NotStarted);
        Assert.True(Enum.IsDefined(typeof(ProcessState), ProcessState.Starting));
        Assert.True(Enum.IsDefined(typeof(ProcessState), ProcessState.Running));
        Assert.True(Enum.IsDefined(typeof(ProcessState), ProcessState.Stopping));
        Assert.True(Enum.IsDefined(typeof(ProcessState), ProcessState.Stopped));
        Assert.True(Enum.IsDefined(typeof(ProcessState), ProcessState.Faulted));
    }

    [Fact]
    public void ConnectionState_ShouldHaveExpectedValues()
    {
        Assert.True(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Disconnected));
        Assert.True(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Connecting));
        Assert.True(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Connected));
        Assert.True(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Reconnecting));
        Assert.True(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Faulted));
    }

    [Fact]
    public void Vector2_ShouldStoreComponents()
    {
        var v = new Vector2(1.5f, 2.5f);
        Assert.Equal(1.5f, v.X);
        Assert.Equal(2.5f, v.Y);
    }

    [Fact]
    public void Vector2_Equality_WorksCorrectly()
    {
        var v1 = new Vector2(1f, 2f);
        var v2 = new Vector2(1f, 2f);
        var v3 = new Vector2(3f, 4f);

        Assert.Equal(v1, v2);
        Assert.NotEqual(v1, v3);
    }

    [Fact]
    public void Vector3_ShouldStoreComponents()
    {
        var v = new Vector3(1f, 2f, 3f);
        Assert.Equal(1f, v.X);
        Assert.Equal(2f, v.Y);
        Assert.Equal(3f, v.Z);
    }

    [Fact]
    public void Vector3_Equality_WorksCorrectly()
    {
        var v1 = new Vector3(1f, 2f, 3f);
        var v2 = new Vector3(1f, 2f, 3f);
        var v3 = new Vector3(0f, 0f, 0f);

        Assert.Equal(v1, v2);
        Assert.NotEqual(v1, v3);
    }

    [Fact]
    public void Color_ShouldStoreRgbaComponents()
    {
        var c = new Color(0.1f, 0.2f, 0.3f, 0.4f);
        Assert.Equal(0.1f, c.R);
        Assert.Equal(0.2f, c.G);
        Assert.Equal(0.3f, c.B);
        Assert.Equal(0.4f, c.A);
    }

    [Fact]
    public void Color_DefaultAlpha_IsOne()
    {
        var c = new Color(1f, 0f, 0f);
        Assert.Equal(1.0f, c.A);
    }

    [Fact]
    public void ComponentDefinition_ShouldStoreTypeAndProperties()
    {
        var props = new Dictionary<string, object?> { ["mass"] = 1.0 };
        var comp = new ComponentDefinition("Rigidbody", props);

        Assert.Equal("Rigidbody", comp.Type);
        Assert.NotNull(comp.Properties);
        Assert.Equal(1.0, comp.Properties["mass"]);
    }

    [Fact]
    public void ComponentDefinition_DefaultProperties_IsNull()
    {
        var comp = new ComponentDefinition("BoxCollider");
        Assert.Null(comp.Properties);
    }

    [Fact]
    public void GameObjectDefinition_ShouldStoreAllFields()
    {
        var pos = new Vector3(1f, 2f, 3f);
        var rot = new Vector3(0f, 90f, 0f);
        var scale = new Vector3(1f, 1f, 1f);
        var components = new List<ComponentDefinition> { new("Rigidbody") };

        var go = new GameObjectDefinition("Player", pos, rot, scale, components);

        Assert.Equal("Player", go.Name);
        Assert.Equal(pos, go.Position);
        Assert.Equal(rot, go.Rotation);
        Assert.Equal(scale, go.Scale);
        Assert.Single(go.Components!);
    }

    [Fact]
    public void GameObjectDefinition_DefaultOptionalFields_AreNull()
    {
        var go = new GameObjectDefinition("Empty");
        Assert.Null(go.Position);
        Assert.Null(go.Rotation);
        Assert.Null(go.Scale);
        Assert.Null(go.Components);
    }

    [Fact]
    public void MaterialDefinition_ShouldStoreAllFields()
    {
        var color = new Color(1f, 0f, 0f);
        var emission = new Color(0.5f, 0f, 0f);
        var mat = new MaterialDefinition("RedMetal", color, 0.8f, 0.9f, emission);

        Assert.Equal("RedMetal", mat.Name);
        Assert.Equal(color, mat.Color);
        Assert.Equal(0.8f, mat.Metallic);
        Assert.Equal(0.9f, mat.Smoothness);
        Assert.Equal(emission, mat.EmissionColor);
    }

    [Fact]
    public void MaterialDefinition_DefaultValues_AreCorrect()
    {
        var mat = new MaterialDefinition("Default");
        Assert.Null(mat.Color);
        Assert.Equal(0.0f, mat.Metallic);
        Assert.Equal(0.5f, mat.Smoothness);
        Assert.Null(mat.EmissionColor);
    }

    [Fact]
    public void ProjectInfo_ShouldStoreAllFields()
    {
        var scenes = new List<string> { "Assets/Scenes/Main.Godot" };
        var packages = new List<string> { "com.Godot.render-pipelines.universal" };
        var info = new ProjectInfo("/projects/MyGame", "MyGame", "2023.2.1f1", scenes, packages);

        Assert.Equal("/projects/MyGame", info.ProjectPath);
        Assert.Equal("MyGame", info.ProjectName);
        Assert.Equal("2023.2.1f1", info.GodotVersion);
        Assert.Single(info.Scenes);
        Assert.Single(info.Packages);
    }
}
