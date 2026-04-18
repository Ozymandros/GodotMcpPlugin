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

    [KernelFunction("list_controls")]
    [Description("Lists UI controls in a scene.")]
    public Task<IReadOnlyList<ControlInfo>> ListControlsAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiListControlsAsync(new UiListControlsRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    [KernelFunction("create_control")]
    [Description("Creates a UI control in a scene.")]
    public Task<ControlInfo?> CreateControlAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Parent node path.")] string parentNodePath,
        [Description("Control name.")] string controlName,
        [Description("Control type.")] string controlType,
        CancellationToken cancellationToken = default) =>
        _mcp.UiCreateControlAsync(
            new UiCreateControlRequest(new McpProjectFile(projectPath, fileName), parentNodePath, controlName, controlType),
            cancellationToken);

    [KernelFunction("update_control")]
    [Description("Updates a UI control in a scene.")]
    public Task<ControlInfo?> UpdateControlAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Control node path.")] string controlNodePath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.UiUpdateControlAsync(
            new UiUpdateControlRequest(new McpProjectFile(projectPath, fileName), controlNodePath, properties),
            cancellationToken);

    [KernelFunction("apply_layout_preset")]
    [Description("Applies a layout preset to a UI control.")]
    public Task<UiLayoutPresetResult?> ApplyLayoutPresetAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Control node path.")] string controlNodePath,
        [Description("Layout preset (e.g. full_rect, top_left, center).")] string preset,
        CancellationToken cancellationToken = default) =>
        _mcp.UiApplyLayoutPresetAsync(
            new UiApplyLayoutPresetRequest(new McpProjectFile(projectPath, fileName), controlNodePath, preset),
            cancellationToken);

    [KernelFunction("list_themes")]
    [Description("Lists available UI themes in a scene.")]
    public Task<IReadOnlyList<string>> ListThemesAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiListThemesAsync(new UiListThemesRequest(new McpProjectFile(projectPath, fileName)), cancellationToken);

    [KernelFunction("apply_theme")]
    [Description("Applies a UI theme to a control.")]
    public Task<UiThemeResult?> ApplyThemeAsync(
        [Description("Project root path (res:// or absolute path under the project).")] string projectPath,
        [Description("Scene file path relative to project root.")] string fileName,
        [Description("Control node path.")] string controlNodePath,
        [Description("Theme name.")] string themeName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiApplyThemeAsync(
            new UiApplyThemeRequest(new McpProjectFile(projectPath, fileName), controlNodePath, themeName),
            cancellationToken);
}
