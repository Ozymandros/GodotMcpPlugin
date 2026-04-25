using System.ComponentModel;
using GodotMcp.Core.Models;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing UI MCP commands.
/// </summary>
public sealed class UiSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists UI controls present in the specified scene.
    /// </summary>
    [KernelFunction("list_controls")]
    [Description("Lists UI controls in a scene.")]
    public Task<IReadOnlyList<ControlInfo>> ListControlsAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiListControlsAsync(new UiListControlsRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Creates a UI control in the specified scene under the given parent node.
    /// </summary>
    [KernelFunction("create_control")]
    [Description("Creates a UI control in a scene.")]
    public Task<ControlInfo?> CreateControlAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Parent node path.")] string parentNodePath,
        [Description("Control name.")] string controlName,
        [Description("Control type.")] string controlType,
        CancellationToken cancellationToken = default) =>
        _mcp.UiCreateControlAsync(
            new UiCreateControlRequest(new McpProjectFile(projectPath, fileName), parentNodePath, controlName, controlType),
            cancellationToken);

    /// <summary>
    /// Updates properties of a UI control node in a scene.
    /// </summary>
    [KernelFunction("update_control")]
    [Description("Updates a UI control in a scene.")]
    public Task<ControlInfo?> UpdateControlAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Control node path.")] string controlNodePath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.UiUpdateControlAsync(
            new UiUpdateControlRequest(new McpProjectFile(projectPath, fileName), controlNodePath, properties),
            cancellationToken);

    /// <summary>
    /// Applies a predefined layout preset to a UI control node.
    /// </summary>
    [KernelFunction("apply_layout_preset")]
    [Description("Applies a layout preset to a UI control.")]
    public Task<UiLayoutPresetResult?> ApplyLayoutPresetAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Control node path.")] string controlNodePath,
        [Description("Layout preset (e.g. full_rect, top_left, center).")] string preset,
        CancellationToken cancellationToken = default) =>
        _mcp.UiApplyLayoutPresetAsync(
            new UiApplyLayoutPresetRequest(new McpProjectFile(projectPath, fileName), controlNodePath, preset),
            cancellationToken);

    /// <summary>
    /// Lists available UI themes referenced by the specified scene.
    /// </summary>
    [KernelFunction("list_themes")]
    [Description("Lists available UI themes in a scene.")]
    public Task<IReadOnlyList<string>> ListThemesAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiListThemesAsync(new UiListThemesRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    /// <summary>
    /// Applies a UI theme to a control node in the specified scene.
    /// </summary>
    [KernelFunction("apply_theme")]
    [Description("Applies a UI theme to a control.")]
    public Task<UiThemeResult?> ApplyThemeAsync(
        [Description("Absolute filesystem path to the Godot project root (folder containing project.godot).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Control node path.")] string controlNodePath,
        [Description("Theme name.")] string themeName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiApplyThemeAsync(
            new UiApplyThemeRequest(new McpProjectFile(projectPath, fileName), controlNodePath, themeName),
            cancellationToken);
}
