using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Models;

/// <summary>
/// Receiving workflow keyboard shortcut settings persisted per user.
/// </summary>
public sealed class Model_Settings_ReceivingShortcuts
{
    public Model_KeyboardShortcutBinding ModeSelectionShortcut { get; set; } =
        new() { Key = "M", IsCtrlEnabled = true };

    public Model_KeyboardShortcutBinding ClearLabelDataShortcut { get; set; } =
        new()
        {
            Key = "C",
            IsCtrlEnabled = true,
            IsShiftEnabled = true,
        };

    public Model_KeyboardShortcutBinding NextStepShortcut { get; set; } =
        new() { Key = "Right", IsCtrlEnabled = true };

    public Model_KeyboardShortcutBinding BackStepShortcut { get; set; } =
        new() { Key = "Left", IsCtrlEnabled = true };

    public Model_KeyboardShortcutBinding HelpShortcut { get; set; } =
        new() { Key = "Slash", IsShiftEnabled = true };

    public bool IsToggleSimpleNavigationEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new instance containing the receiving workflow defaults.
    /// </summary>
    /// <returns>A default keyboard shortcut settings model.</returns>
    public static Model_Settings_ReceivingShortcuts CreateDefault() => new();

    /// <summary>
    /// Clones shortcut settings so callers can edit them without mutating cached instances.
    /// </summary>
    /// <returns>A deep copy of the shortcut settings.</returns>
    public Model_Settings_ReceivingShortcuts Clone() =>
        new()
        {
            ModeSelectionShortcut = ModeSelectionShortcut.Clone(),
            ClearLabelDataShortcut = ClearLabelDataShortcut.Clone(),
            NextStepShortcut = NextStepShortcut.Clone(),
            BackStepShortcut = BackStepShortcut.Clone(),
            HelpShortcut = HelpShortcut.Clone(),
            IsToggleSimpleNavigationEnabled = IsToggleSimpleNavigationEnabled,
        };
}
