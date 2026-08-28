using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// A coil part that requires inner-diameter welding before it goes on the press cradle.
/// Maps to the settings_weldedcoils table (id, partid, isActive). Rows are read-only
/// after add; IsActive drives the Activate/Deactivate button label and status text.
/// </summary>
public partial class Model_Tool_WeldedCoil : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _partId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleLabel))]
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
    private bool _isActive;

    /// <summary>Label for the row toggle button ("Deactivate" when active).</summary>
    public string ToggleLabel => IsActive ? "Deactivate" : "Activate";

    /// <summary>Human-readable active state for the status column.</summary>
    public string StatusLabel => IsActive ? "Active" : "Inactive";
}
