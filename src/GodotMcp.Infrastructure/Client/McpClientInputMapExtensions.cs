using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

public static class McpClientInputMapExtensions
{
    public static async Task<IReadOnlyList<InputMapActionInfo>> InputMapListActionsAsync(
        this IMcpClient client,
        InputMapListActionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return await client.SendAsync<IReadOnlyList<InputMapActionInfo>>(
            "input.list_actions",
            new Dictionary<string, object?> { ["projectPath"] = projectPath },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<InputMapActionInfo>();
    }

    public static Task<InputMapActionInfo?> InputMapGetActionAsync(
        this IMcpClient client,
        InputMapGetActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return client.SendAsync<InputMapActionInfo>(
            "input.get_action",
            new Dictionary<string, object?>
            {
                ["projectPath"] = projectPath,
                ["actionName"] = request.ActionName
            },
            cancellationToken);
    }

    public static Task<ProjectOperationResult?> InputMapAddActionAsync(
        this IMcpClient client,
        InputMapAddActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        var parameters = new Dictionary<string, object?>
        {
            ["projectPath"] = projectPath,
            ["actionName"] = request.ActionName
        };
        if (request.Deadzone.HasValue)
        {
            parameters["deadzone"] = request.Deadzone.Value;
        }
        return client.SendAsync<ProjectOperationResult>("input.add_action", parameters, cancellationToken);
    }

    public static Task<ProjectOperationResult?> InputMapRemoveActionAsync(
        this IMcpClient client,
        InputMapRemoveActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return client.SendAsync<ProjectOperationResult>(
            "input.remove_action",
            new Dictionary<string, object?>
            {
                ["projectPath"] = projectPath,
                ["actionName"] = request.ActionName
            },
            cancellationToken);
    }

    public static Task<ProjectOperationResult?> InputMapAddInputEventAsync(
        this IMcpClient client,
        InputMapAddInputEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        var parameters = new Dictionary<string, object?>
        {
            ["projectPath"] = projectPath,
            ["actionName"] = request.ActionName,
            ["device"] = request.Device
        };
        if (request.ButtonMask.HasValue) parameters["buttonMask"] = request.ButtonMask.Value;
        if (request.Keycode.HasValue) parameters["keycode"] = request.Keycode.Value;
        if (request.PhysicalKeycode.HasValue) parameters["physicalKeycode"] = request.PhysicalKeycode.Value;
        if (request.Gravity.HasValue) parameters["gravity"] = request.Gravity.Value;
        if (request.PositionX.HasValue) parameters["positionX"] = request.PositionX.Value;
        if (request.PositionY.HasValue) parameters["positionY"] = request.PositionY.Value;
        if (request.Axis.HasValue) parameters["axis"] = request.Axis.Value;
        return client.SendAsync<ProjectOperationResult>("input.add_input_event", parameters, cancellationToken);
    }

    public static Task<ProjectOperationResult?> InputMapRemoveInputEventAsync(
        this IMcpClient client,
        InputMapRemoveInputEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectPath = GodotMcpPathNormalization.NormalizeProjectDirectory(request.ProjectPath);
        return client.SendAsync<ProjectOperationResult>(
            "input.remove_input_event",
            new Dictionary<string, object?>
            {
                ["projectPath"] = projectPath,
                ["actionName"] = request.ActionName,
                ["eventIndex"] = request.EventIndex
            },
            cancellationToken);
    }
}
