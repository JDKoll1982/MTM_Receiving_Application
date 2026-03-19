using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Defaults;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Flat DAO-layer representation of a single PO line row returned from an Infor Visual SQL query.
/// Used internally by DAOs and converted to <see cref="Model_InforVisualPO"/> at the service layer.
/// </summary>
public partial class Model_InforVisualPOLine : ObservableObject
{
    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private int _poLine;

    [ObservableProperty]
    private string _partNumber = string.Empty;

    [ObservableProperty]
    private string _partDescription = string.Empty;

    [ObservableProperty]
    private decimal _orderedQty;

    [ObservableProperty]
    private decimal _receivedQty;

    [ObservableProperty]
    private decimal _remainingQty;

    [ObservableProperty]
    private string _unitOfMeasure = InforVisualDefaults.DefaultUom;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private string _vendorCode = string.Empty;

    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private string _poStatus = string.Empty;

    [ObservableProperty]
    private string _siteId = InforVisualDefaults.DefaultSiteId;

    [ObservableProperty]
    private string _defaultLocationId = string.Empty;
}
