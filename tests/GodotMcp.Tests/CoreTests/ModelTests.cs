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

    [Fact]
    public void ResourceInfo_ShouldCreateWithExpectedValues()
    {
        var resource = new ResourceInfo(
            Path: "res://materials/mat.tres",
            Type: "StandardMaterial3D",
            Name: "mat",
            Exists: true);

        Assert.Equal("res://materials/mat.tres", resource.Path);
        Assert.Equal("StandardMaterial3D", resource.Type);
        Assert.Equal("mat", resource.Name);
        Assert.True(resource.Exists);
    }

    [Fact]
    public void ResourceData_ShouldCreateWithProperties()
    {
        var properties = new Dictionary<string, object?>
        {
            ["albedoColor"] = "#ffffff",
            ["metallic"] = 0.5
        };

        var resource = new ResourceData(
            Path: "res://materials/mat.tres",
            Type: "StandardMaterial3D",
            Properties: properties);

        Assert.Equal("res://materials/mat.tres", resource.Path);
        Assert.Equal("StandardMaterial3D", resource.Type);
        Assert.Equal(2, resource.Properties.Count);
        Assert.Equal("#ffffff", resource.Properties["albedoColor"]);
    }

    [Fact]
    public void ResourceRequestRecords_ShouldCreateWithExpectedValues()
    {
        var list = new ResourceListRequest("res://materials", "StandardMaterial3D");
        var read = new ResourceReadRequest(new McpProjectFile("res://", "materials/mat.tres"));
        var update = new ResourceUpdateRequest(
            new McpProjectFile("res://", "materials/mat.tres"),
            new Dictionary<string, object?> { ["metallic"] = 0.8 });
        var create = new ResourceCreateRequest(
            new McpProjectFile("res://", "materials/new_mat.tres"),
            "StandardMaterial3D",
            new Dictionary<string, object?> { ["roughness"] = 0.2 });

        Assert.Equal("res://materials", list.Directory);
        Assert.Equal("StandardMaterial3D", list.ResourceType);
        Assert.Equal("res://", read.Resource.ProjectPath);
        Assert.Equal("materials/mat.tres", read.Resource.FileName);
        Assert.Equal("res://", update.Resource.ProjectPath);
        Assert.Equal("materials/mat.tres", update.Resource.FileName);
        Assert.Single(update.Properties);
        Assert.Equal("res://", create.Resource.ProjectPath);
        Assert.Equal("materials/new_mat.tres", create.Resource.FileName);
        Assert.Equal("StandardMaterial3D", create.ResourceType);
        Assert.Single(create.Properties);
    }

    [Fact]
    public void ControlInfo_AndUiRequests_ShouldCreateWithExpectedValues()
    {
        var uiScene = new McpProjectFile("res://", "scenes/ui.tscn");
        var control = new ControlInfo("RootPanel", ".", "Panel", null);
        var list = new UiListControlsRequest(uiScene);
        var create = new UiCreateControlRequest(uiScene, ".", "PlayButton", "Button");
        var update = new UiUpdateControlRequest(
            uiScene,
            "./PlayButton",
            new Dictionary<string, object?> { ["text"] = "Play" });
        var preset = new UiApplyLayoutPresetRequest(uiScene, "./RootPanel", "full_rect");
        var presetResult = new UiLayoutPresetResult(true, "Applied");
        var listThemes = new UiListThemesRequest(uiScene);
        var applyTheme = new UiApplyThemeRequest(uiScene, "./RootPanel", "dark_flat");
        var themeResult = new UiThemeResult(true, "Theme applied", "dark_flat");

        Assert.Equal("Panel", control.ControlType);
        Assert.Equal("res://", list.Scene.ProjectPath);
        Assert.Equal("scenes/ui.tscn", list.Scene.FileName);
        Assert.Equal("PlayButton", create.ControlName);
        Assert.Equal("./PlayButton", update.ControlNodePath);
        Assert.Single(update.Properties);
        Assert.Equal("full_rect", preset.Preset);
        Assert.True(presetResult.Success);
        Assert.Equal("res://", listThemes.Scene.ProjectPath);
        Assert.Equal("scenes/ui.tscn", listThemes.Scene.FileName);
        Assert.Equal("dark_flat", applyTheme.ThemeName);
        Assert.Equal("dark_flat", themeResult.AppliedTheme);
    }

    [Fact]
    public void LightInfo_AndLightingRequests_ShouldCreateWithExpectedValues()
    {
        var mainScene = new McpProjectFile("res://", "scenes/main.tscn");
        var light = new LightInfo("Sun", "./Sun", "DirectionalLight3D", true);
        var list = new LightListRequest("res://");
        var create = new LightCreateRequest(mainScene, ".", "Fill", "OmniLight3D");
        var update = new LightUpdateRequest(
            mainScene,
            "./Fill",
            new Dictionary<string, object?> { ["energy"] = 2.0 });
        var validate = new LightValidateRequest("res://");
        var validateResult = new LightValidationResult(true, "Lighting valid");
        var tune = new LightTuneRequest(
            mainScene,
            "./Sun",
            new Dictionary<string, object?> { ["energy"] = 2.5 });

        Assert.Equal("DirectionalLight3D", light.LightType);
        Assert.Equal("res://", list.ProjectPath);
        Assert.Equal("Fill", create.NodeName);
        Assert.Equal("./Fill", update.NodePath);
        Assert.Single(update.Properties);
        Assert.Equal("res://", validate.ProjectPath);
        Assert.True(validateResult.Success);
        Assert.Equal("./Sun", tune.NodePath);
        Assert.Single(tune.Properties);
    }

    [Fact]
    public void PhysicsModels_AndRequests_ShouldCreateWithExpectedValues()
    {
        var mainScene = new McpProjectFile("res://", "scenes/main.tscn");
        var body = new PhysicsBodyInfo("PlayerBody", "./Player", "CharacterBody3D", true);
        var shape = new PhysicsShapeInfo(
            "Capsule",
            "./Player/Capsule",
            "CapsuleShape3D",
            new Dictionary<string, object?> { ["radius"] = 0.4 });
        var create = new PhysicsCreateShapeRequest(
            mainScene,
            "./Player",
            "Capsule",
            "CapsuleShape3D",
            new Dictionary<string, object?> { ["height"] = 1.8 });
        var validate = new PhysicsValidationResult(true, "Physics valid");
        var setLayers = new PhysicsSetLayersRequest(mainScene, "./Player", 2, 5);
        var runChecks = new PhysicsRunChecksRequest(mainScene, "./Player");
        var layerResult = new PhysicsLayerResult(true, "Updated", 2, 5);
        var checkResult = new PhysicsCheckResult(true, "No issues", Array.Empty<string>());

        Assert.Equal("CharacterBody3D", body.BodyType);
        Assert.Equal("CapsuleShape3D", shape.ShapeType);
        Assert.Single(shape.Properties);
        Assert.Equal("./Player", create.BodyPath);
        Assert.True(validate.Success);
        Assert.Equal(2, setLayers.CollisionLayer);
        Assert.Equal("./Player", runChecks.BodyPath);
        Assert.Equal(5, layerResult.CollisionMask);
        Assert.Empty(checkResult.Issues!);
    }

    [Fact]
    public void NavigationLintAndPresetModels_ShouldCreateWithExpectedValues()
    {
        var mainScene = new McpProjectFile("res://", "scenes/main.tscn");
        var nav = new NavigationRegionInfo("RegionA", "./RegionA", true);
        var navCreate = new NavigationCreateRegionRequest(mainScene, ".", "RegionA");
        var navResult = new NavigationResult(true, "Baked");

        var issue = new LintIssue("L001", "warning", "Unused node", "./Temp");
        var lintResult = new LintResult(new[] { issue }, true);
        var lintProject = new LintProjectRequest("res://");

        var presetReq = new PresetApplyRequest(mainScene, "./Player", "player_default");
        var presetResult = new PresetResult(true, "Applied", "./Player");

        Assert.Equal("RegionA", nav.Name);
        Assert.Equal(".", navCreate.ParentPath);
        Assert.True(navResult.Success);
        Assert.Single(lintResult.Issues);
        Assert.Equal("L001", lintResult.Issues[0].Code);
        Assert.Equal("res://", lintProject.ProjectPath);
        Assert.Equal("player_default", presetReq.PresetName);
        Assert.Equal("./Player", presetResult.AppliedToPath);
    }

    [Fact]
    public void CameraModels_ShouldCreateWithExpectedValues()
    {
        var camera = new CameraInfo(
            "res://scenes/main.tscn",
            "./MainCamera",
            "Camera3D",
            70,
            null,
            0.1,
            4000,
            "Perspective",
            true);

        var mainScene = new McpProjectFile("res://", "scenes/main.tscn");
        var list = new CameraListRequest("res://");
        var create = new CameraCreateRequest(mainScene, "./MainCamera", "3d", "cinematic");
        var update = new CameraUpdateRequest(
            mainScene,
            "./MainCamera",
            new Dictionary<string, object?> { ["fov"] = 75.0 });
        var validate = new CameraValidateRequest("res://");
        var issue = new CameraValidationIssue(
            "res://scenes/main.tscn",
            "warning",
            "FOV out of range",
            "Use value between 1 and 179",
            "camera_fov_range",
            "res://scenes/main.tscn",
            "./MainCamera");

        Assert.Equal("Camera3D", camera.Type);
        Assert.True(camera.Current);
        Assert.Equal("res://", list.ProjectPath);
        Assert.Equal("cinematic", create.Preset);
        Assert.Equal("./MainCamera", update.NodePath);
        Assert.Equal("res://", validate.ProjectPath);
        Assert.Equal("camera_fov_range", issue.Rule);
    }

    [Fact]
    public void ScriptAndImportModels_ShouldCreateWithExpectedValues()
    {
        var mainScene = new McpProjectFile("res://", "scenes/main.tscn");
        var playerScript = new McpProjectFile("res://", "scripts/player.cs");
        var scriptCreate = new ScriptCreateRequest(playerScript, "CSharp", "Node3D", "PlayerController");
        var scriptAttach = new ScriptAttachRequest(mainScene, "Player", playerScript);
        var scriptValidate = new ScriptValidateRequest(playerScript, true);
        var scriptInfo = new ScriptInfo("res://scripts/player.cs", "CSharp", "Node3D", "PlayerController");
        var scriptValidation = new ScriptValidationResult(true, "OK", Array.Empty<string>(), new[] { "warning" });

        var heroPng = new McpProjectFile("res://", "assets/hero.png");
        var importGenerate = new GenerateImportFileRequest(
            heroPng,
            "texture",
            "CompressedTexture2D",
            new Dictionary<string, object?> { ["compress/mode"] = 2 });
        var importTexture = new CreateTextureRequest(heroPng);
        var importAudio = new CreateAudioRequest(new McpProjectFile("res://", "assets/music.ogg"));
        var reimport = new ReimportAssetRequest(heroPng);
        var importResult = new ImportOperationResult(true, "Generated", "res://assets/hero.png", "res://assets/hero.png.import");

        Assert.Equal("PlayerController", scriptCreate.ClassName);
        Assert.Equal("Player", scriptAttach.NodeName);
        Assert.True(scriptValidate.IsCSharp);
        Assert.Equal("Node3D", scriptInfo.BaseType);
        Assert.Single(scriptValidation.Warnings!);

        Assert.Equal("texture", importGenerate.Importer);
        Assert.Equal("res://", importTexture.Texture.ProjectPath);
        Assert.Equal("assets/hero.png", importTexture.Texture.FileName);
        Assert.Equal("res://", importAudio.Audio.ProjectPath);
        Assert.Equal("assets/music.ogg", importAudio.Audio.FileName);
        Assert.Equal("res://", reimport.Asset.ProjectPath);
        Assert.Equal("assets/hero.png", reimport.Asset.FileName);
        Assert.Equal("res://assets/hero.png.import", importResult.ImportPath);
    }

    [Fact]
    public void ProjectModels_ShouldCreateWithExpectedValues()
    {
        var create = new CreateGodotProjectRequest("MyGame");
        var config = new ConfigureAutoloadRequest("Game", "res://scripts/game.gd", true);
        var addPlugin = new AddPluginRequest("my_plugin");
        var result = new ProjectOperationResult(true, "Configured");

        Assert.Equal("MyGame", create.ProjectName);
        Assert.Equal("Game", config.Key);
        Assert.True(config.Enabled);
        Assert.Equal("my_plugin", addPlugin.PluginName);
        Assert.True(result.Success);
    }
}
