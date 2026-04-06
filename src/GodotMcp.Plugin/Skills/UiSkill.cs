using System.ComponentModel;
using GodotMcp.Infrastructure.Client;

namespace GodotMcp.Plugin.Skills;

/// <summary>
/// Semantic Kernel skill exposing UI MCP commands.
/// </summary>
public sealed class UiSkill(IMcpClient mcp)
{
    private readonly IMcpClient _mcp = mcp;

    /// <summary>
    /// Lists UI controls in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of controls.</returns>
    [KernelFunction("list_controls")]
    [Description("Lists UI controls in a scene.")]
    public Task<IReadOnlyList<ControlInfo>> ListControlsAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.UiListControlsAsync(new UiListControlsRequest(scenePath), cancellationToken);

    /// <summary>
    /// Creates a UI control in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="parentPath">Parent node path.</param>
    /// <param name="controlName">Control name.</param>
    /// <param name="controlType">Control type.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created control, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("create_control")]
    [Description("Creates a UI control in a scene.")]
    public Task<ControlInfo?> CreateControlAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Parent node path.")] string parentPath,
        [Description("Control name.")] string controlName,
        [Description("Control type.")] string controlType,
        CancellationToken cancellationToken = default) =>
        _mcp.UiCreateControlAsync(new UiCreateControlRequest(scenePath, parentPath, controlName, controlType), cancellationToken);

    /// <summary>
    /// Updates a UI control in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="controlPath">Control node path.</param>
    /// <param name="properties">Properties to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated control, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("update_control")]
    [Description("Updates a UI control in a scene.")]
    public Task<ControlInfo?> UpdateControlAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Control node path.")] string controlPath,
        [Description("Properties to update.")] IReadOnlyDictionary<string, object?> properties,
        CancellationToken cancellationToken = default) =>
        _mcp.UiUpdateControlAsync(new UiUpdateControlRequest(scenePath, controlPath, properties), cancellationToken);

    /// <summary>
    /// Applies a layout preset to a UI control.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="controlPath">Control node path.</param>
    /// <param name="presetName">Preset name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The preset application result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("apply_layout_preset")]
    [Description("Applies a layout preset to a UI control.")]
    public Task<UiLayoutPresetResult?> ApplyLayoutPresetAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Control node path.")] string controlPath,
        [Description("Preset name.")] string presetName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiApplyLayoutPresetAsync(new UiApplyLayoutPresetRequest(scenePath, controlPath, presetName), cancellationToken);

    /// <summary>
    /// Lists available UI themes in a scene.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of theme names.</returns>
    [KernelFunction("list_themes")]
    [Description("Lists available UI themes in a scene.")]
    public Task<IReadOnlyList<string>> ListThemesAsync(
        [Description("Scene resource path.")] string scenePath,
        CancellationToken cancellationToken = default) =>
        _mcp.UiListThemesAsync(new UiListThemesRequest(scenePath), cancellationToken);

    /// <summary>
    /// Applies a UI theme to a control.
    /// </summary>
    /// <param name="scenePath">Scene resource path.</param>
    /// <param name="controlPath">Control node path.</param>
    /// <param name="themeName">Theme name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The theme application result, or <c>null</c> when no payload is returned.</returns>
    [KernelFunction("apply_theme")]
    [Description("Applies a UI theme to a control.")]
    public Task<UiThemeResult?> ApplyThemeAsync(
        [Description("Scene resource path.")] string scenePath,
        [Description("Control node path.")] string controlPath,
        [Description("Theme name.")] string themeName,
        CancellationToken cancellationToken = default) =>
        _mcp.UiApplyThemeAsync(new UiApplyThemeRequest(scenePath, controlPath, themeName), cancellationToken);
}
