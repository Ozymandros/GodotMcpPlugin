namespace GodotMcp.Core.Models;

/// <summary>
/// UI command request for listing controls in a scene.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record UiListControlsRequest(string ScenePath);

/// <summary>
/// UI command request for creating a control.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ParentPath">The parent node path for the new control.</param>
/// <param name="ControlName">The control name.</param>
/// <param name="ControlType">The Godot control type.</param>
public sealed record UiCreateControlRequest(
    string ScenePath,
    string ParentPath,
    string ControlName,
    string ControlType);

/// <summary>
/// UI command request for updating a control.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ControlPath">The control node path.</param>
/// <param name="Properties">Properties to update on the control.</param>
public sealed record UiUpdateControlRequest(
    string ScenePath,
    string ControlPath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// UI command request for applying a layout preset.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ControlPath">The control node path.</param>
/// <param name="PresetName">The layout preset name.</param>
public sealed record UiApplyLayoutPresetRequest(
    string ScenePath,
    string ControlPath,
    string PresetName);

/// <summary>
/// Represents the result of applying a UI layout preset.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional result message.</param>
public sealed record UiLayoutPresetResult(
    bool Success,
    string? Message = null);

/// <summary>
/// UI command request for listing themes in a scene.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
public sealed record UiListThemesRequest(string ScenePath);

/// <summary>
/// UI command request for applying a theme to a control.
/// </summary>
/// <param name="ScenePath">The scene resource path.</param>
/// <param name="ControlPath">The control node path.</param>
/// <param name="ThemeName">The theme name to apply.</param>
public sealed record UiApplyThemeRequest(
    string ScenePath,
    string ControlPath,
    string ThemeName);

/// <summary>
/// Represents the result of applying a UI theme.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Optional result message.</param>
/// <param name="AppliedTheme">Optional applied theme identifier.</param>
public sealed record UiThemeResult(
    bool Success,
    string? Message = null,
    string? AppliedTheme = null);
