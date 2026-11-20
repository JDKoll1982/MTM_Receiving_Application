using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Models;

/// <summary>
/// A part number whose coil needs its inner diameter welded before it goes on the press cradle.
/// </summary>
/// <remarks>
/// Reads the <c>settings_weldedcoils</c> table, the same list the Extra Tools Welded Coils page
/// maintains. Rows are read-only here: a part is added or removed, never edited in place.
/// </remarks>
public partial class Model_Settings_ReceivingWeldedPart : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _partId = string.Empty;
}
