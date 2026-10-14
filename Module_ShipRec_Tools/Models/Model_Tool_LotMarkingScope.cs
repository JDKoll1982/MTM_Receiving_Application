using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One entry on the list of part numbers that receive heat/lot markings,
/// as stored in the lot_marking_part_scope table. Removing a part deactivates
/// it rather than deleting the row, so markings already recorded keep their meaning.
/// </summary>
public partial class Model_Tool_LotMarkingScope : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _partId = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private string _addedBy = string.Empty;

    [ObservableProperty]
    private DateTime? _addedAt;
}
