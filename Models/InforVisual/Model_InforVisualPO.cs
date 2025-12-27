using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Models.InforVisual;

public partial class Model_InforVisualPO : ObservableObject
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
    private string _unitOfMeasure = "EA";

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private string _vendorCode = string.Empty;

    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private string _poStatus = string.Empty;

    [ObservableProperty]
    private string _siteId = "002";
}
