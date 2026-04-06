using GodotMcp.Core.Interfaces;
using GodotMcp.Core.Models;

namespace GodotMcp.Infrastructure.Client;

/// <summary>
/// Typed UI wrappers over <see cref="IMcpClient"/> while preserving the base transport contract.
/// </summary>
public static class McpClientUiExtensions
{
    /// <summary>
    /// Lists UI controls in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The UI list-controls request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of controls. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<ControlInfo>> UiListControlsAsync(
        this IMcpClient client,
        UiListControlsRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<ControlInfo>>(
            "ui.list_controls",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<ControlInfo>();
    }

    /// <summary>
    /// Creates a UI control in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The UI create-control request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created control information, or <c>null</c> when no payload is returned.</returns>
    public static Task<ControlInfo?> UiCreateControlAsync(
        this IMcpClient client,
        UiCreateControlRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ControlInfo>(
            "ui.create_control",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["parentPath"] = request.ParentPath,
                ["controlName"] = request.ControlName,
                ["controlType"] = request.ControlType
            },
            cancellationToken);
    }

    /// <summary>
    /// Updates a UI control in a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The UI update-control request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated control information, or <c>null</c> when no payload is returned.</returns>
    public static Task<ControlInfo?> UiUpdateControlAsync(
        this IMcpClient client,
        UiUpdateControlRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<ControlInfo>(
            "ui.update_control",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["controlPath"] = request.ControlPath,
                ["properties"] = request.Properties
            },
            cancellationToken);
    }

    /// <summary>
    /// Applies a UI layout preset to a control.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The UI apply-layout-preset request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The layout preset operation result, or <c>null</c> when no payload is returned.</returns>
    public static Task<UiLayoutPresetResult?> UiApplyLayoutPresetAsync(
        this IMcpClient client,
        UiApplyLayoutPresetRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<UiLayoutPresetResult>(
            "ui.apply_layout_preset",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["controlPath"] = request.ControlPath,
                ["presetName"] = request.PresetName
            },
            cancellationToken);
    }

    /// <summary>
    /// Lists available themes for a scene.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The UI list-themes request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of available theme names. Returns an empty list when the payload is empty.</returns>
    public static async Task<IReadOnlyList<string>> UiListThemesAsync(
        this IMcpClient client,
        UiListThemesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<string>>(
            "ui.list_themes",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath
            },
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<string>();
    }

    /// <summary>
    /// Applies a UI theme to a control.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="request">The UI apply-theme request payload.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The theme application result, or <c>null</c> when no payload is returned.</returns>
    public static Task<UiThemeResult?> UiApplyThemeAsync(
        this IMcpClient client,
        UiApplyThemeRequest request,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync<UiThemeResult>(
            "ui.apply_theme",
            new Dictionary<string, object?>
            {
                ["scenePath"] = request.ScenePath,
                ["controlPath"] = request.ControlPath,
                ["themeName"] = request.ThemeName
            },
            cancellationToken);
    }
}
