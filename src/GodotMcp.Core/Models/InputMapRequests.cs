namespace GodotMcp.Core.Models;

public sealed record InputMapListActionsRequest(string ProjectPath);

public sealed record InputMapGetActionRequest(
    string ProjectPath,
    string ActionName);

public sealed record InputMapAddActionRequest(
    string ProjectPath,
    string ActionName,
    float? Deadzone = null);

public sealed record InputMapRemoveActionRequest(
    string ProjectPath,
    string ActionName);

public sealed record InputMapAddInputEventRequest(
    string ProjectPath,
    string ActionName,
    string Device,
    int? ButtonMask = null,
    int? Keycode = null,
    int? PhysicalKeycode = null,
    float? Gravity = null,
    int? PositionX = null,
    int? PositionY = null,
    float? Axis = null);

public sealed record InputMapRemoveInputEventRequest(
    string ProjectPath,
    string ActionName,
    int EventIndex);

public sealed record InputMapActionInfo(
    string Name,
    IReadOnlyList<InputMapEventInfo> Events,
    float Deadzone);

public sealed record InputMapEventInfo(
    string Device,
    int? ButtonMask = null,
    int? Keycode = null,
    int? PhysicalKeycode = null,
    float? Gravity = null,
    int? PositionX = null,
    int? PositionY = null,
    float? Axis = null);
