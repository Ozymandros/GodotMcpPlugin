using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Script extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientScriptExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task ScriptCreateAsync_MapsPayloadAndReturnsTypedScript()
    {
        _client
            .InvokeToolAsync("create_script", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new
                {
                    path = Combine("scripts", "player.cs"),
                    language = "CSharp",
                    baseType = "Node3D",
                    className = "PlayerController"
                }));

        var result = await _client.ScriptCreateAsync(
            new ScriptCreateRequest(new McpProjectFile(Root, "scripts/player.cs"), "CSharp", "Node3D", "PlayerController"));

        Assert.NotNull(result);
        Assert.Equal("PlayerController", result!.ClassName);

        await _client.Received(1).InvokeToolAsync(
            "create_script",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scripts/player.cs") &&
                Equals(d["language"], "CSharp") &&
                Equals(d["baseType"], "Node3D") &&
                Equals(d["className"], "PlayerController")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScriptCreateAsync_IncludesRawContent_WhenProvided()
    {
        var raw = "// hello world\nprint(\"hi\")";
        _client
            .InvokeToolAsync("create_script", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-4",
                true,
                new
                {
                    path = Combine("scripts", "generated.gd"),
                    language = "gd",
                    baseType = "Node",
                }));

        var result = await _client.ScriptCreateAsync(
            new ScriptCreateRequest(new McpProjectFile(Root, "scripts/generated.gd"), "gd", "Node", null, raw));

        Assert.NotNull(result);

        await _client.Received(1).InvokeToolAsync(
            "create_script",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d => Equals(d["rawContent"], raw)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScriptAttachAsync_MapsPayloadAndReturnsCommandResult()
    {
        _client
            .InvokeToolAsync("attach_script", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-2", true, new { success = true, message = "Attached" }));

        var result = await _client.ScriptAttachAsync(
            new ScriptAttachRequest(
                new McpProjectFile(Root, "scenes/main.tscn"),
                "Player",
                new McpProjectFile(Root, "scripts/player.cs")));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "attach_script",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/main.tscn") &&
                Equals(d["nodeName"], "Player") &&
                Equals(d["scriptFileName"], "scripts/player.cs")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScriptValidateAsync_MapsPayloadAndReturnsValidationResult()
    {
        _client
            .InvokeToolAsync("validate_script", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-3",
                true,
                new
                {
                    success = true,
                    message = "OK",
                    errors = Array.Empty<string>(),
                    warnings = new[] { "Unused using" }
                }));

        var result = await _client.ScriptValidateAsync(new ScriptValidateRequest(new McpProjectFile(Root, "scripts/player.cs"), true));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Single(result.Warnings!);

        await _client.Received(1).InvokeToolAsync(
            "validate_script",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scripts/player.cs") &&
                Equals(d["isCSharp"], true)),
            Arg.Any<CancellationToken>());
    }
}
