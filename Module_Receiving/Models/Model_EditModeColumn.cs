using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents one column in the Edit Mode DataGrid with its header text,
/// setting key, and current visibility state.
/// </summary>
public partial class Model_EditModeColumn : ObservableObject
{
    /// <summary>Stable identifier used as the settings storage key.</summary>
    public required string Key { get; init; }

    /// <summary>Display text shown in the column chooser dialog.</summary>
    public required string Header { get; init; }

    /// <summary>
    /// When true, this column is always shown and its checkbox in the
    /// column chooser dialog is disabled.
    /// </summary>
    public bool IsAlwaysVisible { get; init; }

    [ObservableProperty]
    private bool _isVisible;
}
