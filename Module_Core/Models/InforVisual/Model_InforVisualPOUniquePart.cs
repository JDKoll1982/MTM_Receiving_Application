using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Flat DAO-layer representation of one unique part on a purchase order,
/// including the PO header fields and the part's total on-hand quantity and
/// current location display. Returned from an Infor Visual SQL query and
/// converted to <see cref="Model_InforVisualPO"/> (unique
/// <see cref="Model_InforVisualPart"/> rows) at the service layer.
/// </summary>
public partial class Model_InforVisualPOUniquePart : ObservableObject
{
    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private string _poStatus = string.Empty;

    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private DateTime? _headerPromiseDate;

    [ObservableProperty]
    private DateTime? _headerDesiredReceiveDate;

    [ObservableProperty]
    private string _freeOnBoard = string.Empty;

    [ObservableProperty]
    private string _partNumber = string.Empty;

    [ObservableProperty]
    private string _poLineNumber = string.Empty;

    [ObservableProperty]
    private string _partDescription = string.Empty;

    [ObservableProperty]
    private decimal _onHandQty;

    [ObservableProperty]
    private string _location = string.Empty;
}
