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
}
