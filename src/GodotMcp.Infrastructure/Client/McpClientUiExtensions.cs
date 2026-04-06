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
        var payload = new Dictionary<string, object?>
        {
            ["scenePath"] = request.ScenePath,
            ["parentNodePath"] = request.ParentPath,
            ["parentPath"] = request.ParentPath,
            ["controlName"] = request.ControlName,
            ["nodeName"] = request.ControlName,
            ["controlType"] = request.ControlType
        };

        return InvokeUiWithFallbackAsync<ControlInfo>(
            client,
            "ui.add_control",
            "ui.create_control",
            payload,
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
        var payload = new Dictionary<string, object?>
        {
            ["scenePath"] = request.ScenePath,
            ["controlNodePath"] = request.ControlPath,
            ["controlPath"] = request.ControlPath,
            ["properties"] = request.Properties
        };

        return InvokeUiWithFallbackAsync<ControlInfo>(
            client,
            "ui.set_control_properties",
            "ui.update_control",
            payload,
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
        var payload = new Dictionary<string, object?>
        {
            ["scenePath"] = request.ScenePath,
            ["controlNodePath"] = request.ControlPath,
            ["controlPath"] = request.ControlPath,
            ["preset"] = request.PresetName,
            ["presetName"] = request.PresetName
        };

        return InvokeUiWithFallbackAsync<UiLayoutPresetResult>(
            client,
            "ui.set_layout_preset",
            "ui.apply_layout_preset",
            payload,
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

    private static async Task<T?> InvokeUiWithFallbackAsync<T>(
        IMcpClient client,
        string primaryCommand,
        string fallbackCommand,
        IReadOnlyDictionary<string, object?> payload,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.SendAsync<T>(primaryCommand, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (McpServerException ex) when (IsToolNotFound(ex))
        {
            return await client.SendAsync<T>(fallbackCommand, payload, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsToolNotFound(McpServerException ex)
        => ex.ErrorCode == -32601
           || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
           || ex.Message.Contains("unknown tool", StringComparison.OrdinalIgnoreCase);
}
