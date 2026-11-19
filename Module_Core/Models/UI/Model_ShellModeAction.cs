using System.Windows.Input;

namespace MTM_Receiving_Application.Module_Core.Models.UI;

/// <summary>
/// One entry-mode control in the shell action bar's left-hand group.
/// </summary>
/// <remarks>
/// The command enters that mode immediately and never changes the stored default. The checkbox is
/// a separate control: ticking it persists the default and does not navigate.
/// </remarks>
public sealed class Model_ShellModeAction
{
    /// <summary>
    /// Gets the mode this control enters: guided, manual, edit, or overlay.
    /// </summary>
    public string Mode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the button text.
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip { get; init; } = string.Empty;

    /// <summary>
    /// Gets the automation and screen-reader name.
    /// </summary>
    public string AccessibilityName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command that enters this mode.
    /// </summary>
    public ICommand? Command { get; init; }

    /// <summary>
    /// Gets a value indicating whether the default badge sits under this control. True only for
    /// guided, manual, and edit; Overlay cannot be a default mode.
    /// </summary>
    public bool CanBeDefault { get; init; }

    /// <summary>
    /// Gets the badge text shown under the control when this mode is the stored default, such as
    /// Default. Empty for Overlay.
    /// </summary>
    public string DefaultBadge { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tooltip that explains how to make this mode the default.
    /// </summary>
    public string DefaultTooltip { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether a divider follows this control.
    /// </summary>
    public bool ShowDividerAfter { get; init; }

    /// <summary>
    /// Gets a control that is not shown.
    /// </summary>
    public static Model_ShellModeAction Hidden { get; } = new();
}
