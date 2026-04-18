using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for UI extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientUiExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task UiListControlsAsync_MapsPayloadAndReturnsTypedList()
    {
        _client
            .InvokeToolAsync("ui.list_controls", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new[] { new { name = "RootPanel", path = ".", controlType = "Panel", parentPath = (string?)null } }));

        var result = await _client.UiListControlsAsync(
            new UiListControlsRequest(new McpProjectFile(Root, "scenes/ui.tscn")));

        Assert.Single(result);
        Assert.Equal("Panel", result[0].ControlType);

        await _client.Received(1).InvokeToolAsync(
            "ui.list_controls",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/ui.tscn")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UiCreateControlAsync_MapsPayloadAndReturnsTypedControl()
    {
        _client
            .InvokeToolAsync("ui.add_control", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new { name = "StartButton", path = "./StartButton", controlType = "Button", parentPath = "." }));

        var result = await _client.UiCreateControlAsync(
            new UiCreateControlRequest(new McpProjectFile(Root, "scenes/ui.tscn"), ".", "StartButton", "Button"));

        Assert.NotNull(result);
        Assert.Equal("StartButton", result!.Name);

        await _client.Received(1).InvokeToolAsync(
            "ui.add_control",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], Root) &&
                Equals(d["fileName"], "scenes/ui.tscn") &&
                Equals(d["parentNodePath"], ".") &&
                Equals(d["controlName"], "StartButton") &&
                Equals(d["controlType"], "Button")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UiApplyLayoutPresetAsync_MapsPayloadAndReturnsResult()
    {
        _client
            .InvokeToolAsync("ui.set_layout_preset", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-3", true, new { success = true, message = "Applied" }));

        var result = await _client.UiApplyLayoutPresetAsync(
            new UiApplyLayoutPresetRequest(new McpProjectFile(Root, "scenes/ui.tscn"), "./RootPanel", "full_rect"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("Applied", result.Message);
    }

    [Fact]
    public async Task UiListThemesAsync_MapsPayloadAndReturnsThemeNames()
    {
        _client
            .InvokeToolAsync("ui.list_themes", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-4", true, new[] { "dark_flat", "light_modern" }));

        var result = await _client.UiListThemesAsync(new UiListThemesRequest(new McpProjectFile(Root, "scenes/ui.tscn")));

        Assert.Equal(2, result.Count);
        Assert.Equal("dark_flat", result[0]);
    }

    [Fact]
    public async Task UiApplyThemeAsync_MapsPayloadAndReturnsResult()
    {
        _client
            .InvokeToolAsync("ui.apply_theme", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse("req-5", true, new { success = true, message = "Theme applied", appliedTheme = "dark_flat" }));

        var result = await _client.UiApplyThemeAsync(
            new UiApplyThemeRequest(new McpProjectFile(Root, "scenes/ui.tscn"), "./RootPanel", "dark_flat"));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("dark_flat", result.AppliedTheme);
    }
}
