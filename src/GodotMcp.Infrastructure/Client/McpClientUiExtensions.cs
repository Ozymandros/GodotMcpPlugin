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
    public static async Task<IReadOnlyList<ControlInfo>> UiListControlsAsync(
        this IMcpClient client,
        UiListControlsRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<ControlInfo>>(
            "ui.list_controls",
            McpProjectFilePayload.ToDictionary(request.Scene),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<ControlInfo>();
    }

    /// <summary>
    /// Creates a UI control in a scene.
    /// </summary>
    public static Task<ControlInfo?> UiCreateControlAsync(
        this IMcpClient client,
        UiCreateControlRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["parentNodePath"] = request.ParentNodePath;
        d["controlType"] = request.ControlType;
        d["controlName"] = request.ControlName;

        return InvokeUiWithFallbackAsync<ControlInfo>(
            client,
            "ui.add_control",
            "ui.create_control",
            d,
            cancellationToken);
    }

    /// <summary>
    /// Updates a UI control in a scene.
    /// </summary>
    public static Task<ControlInfo?> UiUpdateControlAsync(
        this IMcpClient client,
        UiUpdateControlRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["controlNodePath"] = request.ControlNodePath;
        d["properties"] = request.Properties;

        return InvokeUiWithFallbackAsync<ControlInfo>(
            client,
            "ui.set_control_properties",
            "ui.update_control",
            d,
            cancellationToken);
    }

    /// <summary>
    /// Applies a UI layout preset to a control.
    /// </summary>
    public static Task<UiLayoutPresetResult?> UiApplyLayoutPresetAsync(
        this IMcpClient client,
        UiApplyLayoutPresetRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["controlNodePath"] = request.ControlNodePath;
        d["preset"] = request.Preset;

        return InvokeUiWithFallbackAsync<UiLayoutPresetResult>(
            client,
            "ui.set_layout_preset",
            "ui.apply_layout_preset",
            d,
            cancellationToken);
    }

    /// <summary>
    /// Lists available themes for a scene.
    /// </summary>
    public static async Task<IReadOnlyList<string>> UiListThemesAsync(
        this IMcpClient client,
        UiListThemesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.SendAsync<IReadOnlyList<string>>(
            "ui.list_themes",
            McpProjectFilePayload.ToDictionary(request.Scene),
            cancellationToken).ConfigureAwait(false) ?? Array.Empty<string>();
    }

    /// <summary>
    /// Applies a UI theme to a control.
    /// </summary>
    public static Task<UiThemeResult?> UiApplyThemeAsync(
        this IMcpClient client,
        UiApplyThemeRequest request,
        CancellationToken cancellationToken = default)
    {
        var d = McpProjectFilePayload.ToDictionary(request.Scene);
        d["controlNodePath"] = request.ControlNodePath;
        d["themeName"] = request.ThemeName;

        return client.SendAsync<UiThemeResult>(
            "ui.apply_theme",
            d,
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
