using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// A coil part that requires inner-diameter welding before it goes on the press cradle.
/// Maps to the settings_weldedcoils table (id, partid, isActive). Rows are read-only
/// after add; the only list mutation is removing a row.
/// </summary>
public partial class Model_Tool_WeldedCoil : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _partId = string.Empty;

    /// <summary>
    /// Retained so the table mapping stays complete. The tool no longer deactivates
    /// rows, so this value is never surfaced or changed by the UI.
    /// </summary>
    [ObservableProperty]
    private bool _isActive;
}
