using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Tests.InfrastructureTests;

/// <summary>
/// Unit tests for Camera extension wrappers on <see cref="IMcpClient"/>.
/// </summary>
public class McpClientCameraExtensionsTests
{
    private readonly IMcpClient _client = Substitute.For<IMcpClient>();

    [Fact]
    public async Task CameraListAsync_MapsPayloadAndReturnsTypedList()
    {
        _client
            .InvokeToolAsync("camera.list", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-1",
                true,
                new[]
                {
                    new
                    {
                        scenePath = "res://scenes/main.tscn",
                        nodePath = "./MainCamera",
                        type = "Camera3D",
                        fov = 70.0,
                        size = (double?)null,
                        near = 0.1,
                        far = 4000.0,
                        projection = "Perspective",
                        current = true
                    }
                }));

        var result = await _client.CameraListAsync(new CameraListRequest("res://"));

        Assert.Single(result);
        Assert.Equal("Camera3D", result[0].Type);
        Assert.Equal("./MainCamera", result[0].NodePath);
    }

    [Fact]
    public async Task CameraCreateAsync_MapsPayloadAndReturnsTypedCamera()
    {
        _client
            .InvokeToolAsync("camera.create", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-2",
                true,
                new
                {
                    scenePath = "res://scenes/main.tscn",
                    nodePath = "./MainCamera",
                    type = "Camera3D",
                    fov = 70.0,
                    size = (double?)null,
                    near = 0.1,
                    far = 4000.0,
                    projection = "Perspective",
                    current = true
                }));

        var result = await _client.CameraCreateAsync(
            new CameraCreateRequest(new McpProjectFile("res://", "scenes/main.tscn"), "./MainCamera", "3d", "cinematic"));

        Assert.NotNull(result);
        Assert.Equal("Camera3D", result!.Type);
    }

    [Fact]
    public async Task CameraValidateAsync_MapsPayloadAndReturnsIssues()
    {
        _client
            .InvokeToolAsync("camera.validate", Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(new McpResponse(
                "req-3",
                true,
                new[]
                {
                    new
                    {
                        path = "res://scenes/main.tscn",
                        severity = "warning",
                        message = "FOV out of range",
                        suggestedFix = "Set fov between 1 and 179",
                        rule = "camera_fov_range",
                        scenePath = "res://scenes/main.tscn",
                        nodePath = "./MainCamera"
                    }
                }));

        var result = await _client.CameraValidateAsync(new CameraValidateRequest("res://"));

        Assert.Single(result);
        Assert.Equal("camera_fov_range", result[0].Rule);
    }
}
