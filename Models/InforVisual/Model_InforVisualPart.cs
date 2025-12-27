using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Models.InforVisual;

public partial class Model_InforVisualPart : ObservableObject
{
    /// <summary>Part Number (unique identifier)</summary>
    [ObservableProperty]
    private string _partNumber = string.Empty;

    /// <summary>Part Description</summary>
    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>Part Type (PURCHASED, MANUFACTURED, etc.)</summary>
    [ObservableProperty]
    private string _partType = string.Empty;

    /// <summary>Standard Unit Cost</summary>
    [ObservableProperty]
    private decimal _unitCost;

    /// <summary>Primary Unit of Measure</summary>
    [ObservableProperty]
    private string _primaryUom = "EA";

    /// <summary>On-Hand Quantity at Warehouse 002</summary>
    [ObservableProperty]
    private decimal _onHandQty;

    /// <summary>Allocated Quantity (reserved for orders)</summary>
    [ObservableProperty]
    private decimal _allocatedQty;

    /// <summary>Available Quantity (OnHandQty - AllocatedQty)</summary>
    [ObservableProperty]
    private decimal _availableQty;

    /// <summary>Default Warehouse/Site</summary>
    [ObservableProperty]
    private string _defaultSite = "002";

    /// <summary>Part Status (ACTIVE, OBSOLETE, etc.)</summary>
    [ObservableProperty]
    private string _partStatus = "ACTIVE";

    /// <summary>Product Line/Category</summary>
    [ObservableProperty]
    private string _productLine = string.Empty;
}
