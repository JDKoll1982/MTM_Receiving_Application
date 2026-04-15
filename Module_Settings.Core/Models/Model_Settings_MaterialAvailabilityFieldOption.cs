using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Editable row in the Material Availability work-order field settings page.
/// </summary>
public partial class Model_Settings_MaterialAvailabilityFieldOption : ObservableObject
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsLogicOnly { get; set; }

    [ObservableProperty]
    private bool _isVisibleInUi;

    [ObservableProperty]
    private bool _isVisibleInPrint;
}
