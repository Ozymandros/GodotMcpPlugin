using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Import extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientImportExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task GenerateImportFileAsync_MapsPayloadAndReturnsOperationResult()
    {
        var parameters = new Dictionary<string, object?> { ["compress/mode"] = 2 };

        _client
            .InvokeToolAsync("generate_import_file", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new
                {
                    success = true,
                    message = "Generated",
                    assetPath = "res://assets/hero.png",
                    importPath = "res://assets/hero.png.import"
                }));

        var result = await _client.GenerateImportFileAsync(
            new GenerateImportFileRequest(new McpProjectFile("res://", "assets/hero.png"), "texture", "CompressedTexture2D", parameters));

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal("res://assets/hero.png.import", result.ImportPath);

        await _client.Received(1).InvokeToolAsync(
            "generate_import_file",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://") &&
                Equals(d["fileName"], "assets/hero.png") &&
                Equals(d["importer"], "texture") &&
                Equals(d["type"], "CompressedTexture2D") &&
                ReferenceEquals(d["parameters"], parameters)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTextureAsync_MapsPayloadAndReturnsResourceInfo()
    {
        _client
            .InvokeToolAsync("create_texture", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new
                {
                    path = "res://assets/hero.png",
                    type = "Texture2D",
                    name = "hero",
                    exists = true
                }));

        var result = await _client.CreateTextureAsync(new CreateTextureRequest(new McpProjectFile("res://", "assets/hero.png")));

        Assert.NotNull(result);
        Assert.Equal("hero", result!.Name);

        await _client.Received(1).InvokeToolAsync(
            "create_texture",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://") &&
                Equals(d["fileName"], "assets/hero.png")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAudioAsync_MapsPayloadAndReturnsResourceInfo()
    {
        _client
            .InvokeToolAsync("create_audio", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-3",
                true,
                new
                {
                    path = "res://assets/music.ogg",
                    type = "AudioStream",
                    name = "music",
                    exists = true
                }));

        var result = await _client.CreateAudioAsync(new CreateAudioRequest(new McpProjectFile("res://", "assets/music.ogg")));

        Assert.NotNull(result);
        Assert.Equal("AudioStream", result!.Type);

        await _client.Received(1).InvokeToolAsync(
            "create_audio",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://") &&
                Equals(d["fileName"], "assets/music.ogg")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReimportAssetAsync_MapsPayloadAndReturnsOperationResult()
    {
        _client
            .InvokeToolAsync("reimport_asset", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-4",
                true,
                new
                {
                    success = true,
                    message = "Reimported",
                    assetPath = "res://assets/hero.png"
                }));

        var result = await _client.ReimportAssetAsync(new ReimportAssetRequest(new McpProjectFile("res://", "assets/hero.png")));

        Assert.NotNull(result);
        Assert.True(result!.Success);

        await _client.Received(1).InvokeToolAsync(
            "reimport_asset",
            Arg.Is<IReadOnlyDictionary<string, object?>>(d =>
                Equals(d["projectPath"], "res://") &&
                Equals(d["fileName"], "assets/hero.png")),
            Arg.Any<CancellationToken>());
    }
}
