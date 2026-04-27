using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Core.Models.Core;

/// <summary>
/// Shared keyboard shortcut definition used by workflow views and settings pages.
/// </summary>
public partial class Model_KeyboardShortcutBinding : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private bool _isCtrlEnabled;

    [ObservableProperty]
    private bool _isShiftEnabled;

    [ObservableProperty]
    private bool _isAltEnabled;

    [ObservableProperty]
    private bool _isWindowsEnabled;

    /// <summary>
    /// Creates a deep copy so callers can safely edit settings without mutating shared instances.
    /// </summary>
    /// <returns>A copied shortcut binding.</returns>
    public Model_KeyboardShortcutBinding Clone() =>
        new()
        {
            Key = Key,
            IsCtrlEnabled = IsCtrlEnabled,
            IsShiftEnabled = IsShiftEnabled,
            IsAltEnabled = IsAltEnabled,
            IsWindowsEnabled = IsWindowsEnabled,
        };
}
