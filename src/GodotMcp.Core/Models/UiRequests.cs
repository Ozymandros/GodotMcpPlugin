namespace GodotMcp.Core.Models;

/// <summary>
/// UI command request for listing controls in a scene.
/// </summary>
public sealed record UiListControlsRequest(McpProjectFile Scene);

/// <summary>
/// UI command request for creating a control.
/// </summary>
public sealed record UiCreateControlRequest(
    McpProjectFile Scene,
    string ParentNodePath,
    string ControlName,
    string ControlType);

/// <summary>
/// UI command request for updating a control.
/// </summary>
public sealed record UiUpdateControlRequest(
    McpProjectFile Scene,
    string ControlNodePath,
    IReadOnlyDictionary<string, object?> Properties);

/// <summary>
/// UI command request for applying a layout preset.
/// </summary>
public sealed record UiApplyLayoutPresetRequest(
    McpProjectFile Scene,
    string ControlNodePath,
    string Preset);

/// <summary>
/// Represents the result of applying a UI layout preset.
/// </summary>
public sealed record UiLayoutPresetResult(
    bool Success,
    string? Message = null);

/// <summary>
/// UI command request for listing themes in a scene.
/// </summary>
public sealed record UiListThemesRequest(McpProjectFile Scene);

/// <summary>
/// UI command request for applying a theme to a control.
/// </summary>
public sealed record UiApplyThemeRequest(
    McpProjectFile Scene,
    string ControlNodePath,
    string ThemeName);

/// <summary>
/// Represents the result of applying a UI theme.
/// </summary>
public sealed record UiThemeResult(
    bool Success,
    string? Message = null,
    string? AppliedTheme = null);
