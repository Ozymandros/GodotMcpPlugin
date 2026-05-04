using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

public sealed class InputMapSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    [KernelFunction("list_actions")]
    [Description("Lists all input actions in the project.")]
    public Task<IReadOnlyList<InputMapActionInfo>> ListActionsAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        CancellationToken cancellationToken = default) =>
        _mcp.InputMapListActionsAsync(new InputMapListActionsRequest(projectPath), cancellationToken);

    [KernelFunction("get_action")]
    [Description("Gets details of a specific input action.")]
    public Task<InputMapActionInfo?> GetActionAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Input action name.")] string actionName,
        CancellationToken cancellationToken = default) =>
        _mcp.InputMapGetActionAsync(new InputMapGetActionRequest(projectPath, actionName), cancellationToken);

    [KernelFunction("add_action")]
    [Description("Adds a new input action.")]
    public Task<ProjectOperationResult?> AddActionAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Input action name.")] string actionName,
        [Description("Optional deadzone value.")] float? deadzone = null,
        CancellationToken cancellationToken = default) =>
        _mcp.InputMapAddActionAsync(new InputMapAddActionRequest(projectPath, actionName, deadzone), cancellationToken);

    [KernelFunction("remove_action")]
    [Description("Removes an input action.")]
    public Task<ProjectOperationResult?> RemoveActionAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Input action name.")] string actionName,
        CancellationToken cancellationToken = default) =>
        _mcp.InputMapRemoveActionAsync(new InputMapRemoveActionRequest(projectPath, actionName), cancellationToken);

    [KernelFunction("add_input_event")]
    [Description("Adds an input event to an action.")]
    public Task<ProjectOperationResult?> AddInputEventAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Input action name.")] string actionName,
        [Description("Device type (Joypad, Keyboard, Mouse).")] string device,
        [Description("Optional button mask.")] int? buttonMask = null,
        [Description("Optional keycode.")] int? keycode = null,
        [Description("Optional physical keycode.")] int? physicalKeycode = null,
        [Description("Optional gravity for joystick.")] float? gravity = null,
        [Description("Optional position X for mouse.")] int? positionX = null,
        [Description("Optional position Y for mouse.")] int? positionY = null,
        [Description("Optional axis value.")] float? axis = null,
        CancellationToken cancellationToken = default) =>
        _mcp.InputMapAddInputEventAsync(
            new InputMapAddInputEventRequest(projectPath, actionName, device, buttonMask, keycode, physicalKeycode, gravity, positionX, positionY, axis),
            cancellationToken);

    [KernelFunction("remove_input_event")]
    [Description("Removes an input event from an action.")]
    public Task<ProjectOperationResult?> RemoveInputEventAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Input action name.")] string actionName,
        [Description("Index of the event to remove.")] int eventIndex,
        CancellationToken cancellationToken = default) =>
        _mcp.InputMapRemoveInputEventAsync(new InputMapRemoveInputEventRequest(projectPath, actionName, eventIndex), cancellationToken);
}
