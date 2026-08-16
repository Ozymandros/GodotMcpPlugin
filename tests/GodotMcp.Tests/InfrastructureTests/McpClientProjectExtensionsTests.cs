using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Project extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientProjectExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task CreateGodotProjectAsync_MapsPayloadAndReturnsProjectInfo()
    {
        _client
            .InvokeToolAsync("create_godot_project", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new
                {
                    projectPath = "C:/Projects/MyGame",
                    projectName = "MyGame",
                    godotVersion = "4.5",
                    scenes = Array.Empty<string>(),
                    packages = Array.Empty<string>()
                }));

        var result = await _client.CreateGodotProjectAsync(new CreateGodotProjectRequest("MyGame"));

        Assert.NotNull(result);
        Assert.Equal("MyGame", result!.ProjectName);

        await _client.Received(1).InvokeToolAsync(
            "create_godot_project",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectName"], "MyGame") &&
                Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectInfoAsync_UsesExpectedToolAndReturnsProjectInfo()
    {
        _client
            .InvokeToolAsync("get_project_info", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new
                {
                    projectPath = "C:/Projects/MyGame",
                    projectName = "MyGame",
                    godotVersion = "4.5",
                    scenes = new[] { Combine("scenes", "main.tscn") },
                    packages = Array.Empty<string>()
                }));

        var result = await _client.GetProjectInfoAsync();

        Assert.NotNull(result);
        Assert.Equal("C:/Projects/MyGame", result!.ProjectPath);

        await _client.Received(1).InvokeToolAsync(
            "get_project_info",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfigureAutoloadAsync_MapsPayloadAndReturnsOperationResult()
    {
        _client
            .InvokeToolAsync("configure_autoload", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-3", true, new { success = true, message = "Configured" }));

        var gameGd = Combine("scripts", "game.gd");
        var result = await _client.ConfigureAutoloadAsync(
            new ConfigureAutoloadRequest("Game", gameGd, true));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "configure_autoload",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath) &&
                Equals(d["key"], "Game") &&
                Equals(d["value"], gameGd) &&
                Equals(d["enabled"], true)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddPluginAsync_MapsPayloadAndReturnsOperationResult()
    {
        _client
            .InvokeToolAsync("add_plugin", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-4", true, new { success = true, message = "Added" }));

        var result = await _client.AddPluginAsync(new AddPluginRequest("my_plugin"));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "add_plugin",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath) &&
                Equals(d["pluginName"], "my_plugin")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeProjectAsync_MapsPayloadFromProjectPath()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("initialize_project", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-init", true, new { success = true, message = "Initialized" }));

        var result = await _client.InitializeProjectAsync(
            new InitializeProjectRequest(requestPath, "Runner", "cs", "3d", true));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "initialize_project",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                !Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath) &&
                Equals(d["projectName"], "Runner") &&
                Equals(d["language"], "cs") &&
                Equals(d["gameType"], "3d") &&
                Equals(d["includeUi"], true)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateActorAsync_MapsPayloadFromProjectPath()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("create_actor", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-actor", true, new { success = true, message = "Actor created" }));

        var result = await _client.CreateActorAsync(
            new CreateActorRequest(requestPath, "Player", "player", "gd", "2d", true, true));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "create_actor",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                !Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath) &&
                Equals(d["actorName"], "Player") &&
                Equals(d["role"], "player") &&
                Equals(d["language"], "gd") &&
                Equals(d["gameType"], "2d") &&
                Equals(d["createScript"], true) &&
                Equals(d["addToMain"], true)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateActorAsync_OmitsEmptyLanguageAndGameType()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("create_actor", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-actor-omit", true, new { success = true, message = "Actor created" }));

        await _client.CreateActorAsync(new CreateActorRequest(requestPath, "Enemy"));

        await _client.Received(1).InvokeToolAsync(
            "create_actor",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                Equals(d["actorName"], "Enemy") &&
                Equals(d["role"], "enemy") &&
                Equals(d["createScript"], true) &&
                Equals(d["addToMain"], true) &&
                !d.ContainsKey("language") &&
                !d.ContainsKey("gameType")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSpawnableAsync_MapsPayloadFromProjectPath()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("create_spawnable", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-spawn", true, new { success = true, message = "Spawnable created" }));

        var result = await _client.CreateSpawnableAsync(
            new CreateSpawnableRequest(requestPath, "Rock", "cs", "3d", true, true));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "create_spawnable",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                !Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath) &&
                Equals(d["spawnableName"], "Rock") &&
                Equals(d["language"], "cs") &&
                Equals(d["gameType"], "3d") &&
                Equals(d["createScript"], true) &&
                Equals(d["wireToMain"], true)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSpawnableAsync_OmitsEmptyLanguageAndGameType()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("create_spawnable", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-spawn-omit", true, new { success = true, message = "Spawnable created" }));

        await _client.CreateSpawnableAsync(new CreateSpawnableRequest(requestPath, "Crate"));

        await _client.Received(1).InvokeToolAsync(
            "create_spawnable",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                Equals(d["spawnableName"], "Crate") &&
                Equals(d["createScript"], true) &&
                Equals(d["wireToMain"], true) &&
                !d.ContainsKey("language") &&
                !d.ContainsKey("gameType")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetupUiAsync_MapsPayloadFromProjectPath()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("setup_ui", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-ui", true, new { success = true, message = "UI set up" }));

        var result = await _client.SetupUiAsync(new SetupUiRequest(requestPath, "gd", "2d"));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "setup_ui",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                !Equals(d["projectPath"], GodotMcpPathDefaults.DefaultProjectRootPath) &&
                Equals(d["language"], "gd") &&
                Equals(d["gameType"], "2d")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetupUiAsync_OmitsEmptyLanguageAndGameType()
    {
        var requestPath = Path.Combine(Path.GetTempPath(), "GodotMcp", "RunnerBlueprint");
        var expectedPath = GodotMcpPathNormalization.NormalizeProjectDirectory(requestPath);

        _client
            .InvokeToolAsync("setup_ui", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-ui-omit", true, new { success = true, message = "UI set up" }));

        await _client.SetupUiAsync(new SetupUiRequest(requestPath));

        await _client.Received(1).InvokeToolAsync(
            "setup_ui",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], expectedPath) &&
                !d.ContainsKey("language") &&
                !d.ContainsKey("gameType")),
            Arg.Any<CancellationToken>());
    }
}
