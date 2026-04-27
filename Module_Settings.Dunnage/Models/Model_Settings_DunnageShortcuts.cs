using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Models;

/// <summary>
/// Dunnage workflow keyboard shortcut settings persisted per user.
/// </summary>
public sealed class Model_Settings_DunnageShortcuts
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

    public static Model_Settings_DunnageShortcuts CreateDefault() => new();

    public Model_Settings_DunnageShortcuts Clone() =>
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
