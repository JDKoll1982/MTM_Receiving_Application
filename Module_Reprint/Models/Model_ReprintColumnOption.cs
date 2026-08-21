using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Reprint.Models;

/// <summary>
/// A toggleable column on the Reprint history grid. The selection checkbox column is always
/// visible and is not represented here.
/// </summary>
public partial class Model_ReprintColumnOption : ObservableObject
{
    /// <summary>Stable key used to identify the column and persist its visibility.</summary>
    public required string Key { get; init; }

    /// <summary>Display text shown in the column chooser dialog.</summary>
    public required string Header { get; init; }

    /// <summary>True when the column can never be hidden.</summary>
    public bool IsAlwaysVisible { get; init; }

    [ObservableProperty]
    private bool _isVisible;
}
