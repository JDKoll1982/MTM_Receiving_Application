using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace MTM_Receiving_Application.DunnageModule.Models;

/// <summary>
/// Represents a received dunnage load transaction.
/// Database Table: dunnage_loads
/// </summary>
public partial class Model_DunnageLoad : ObservableObject
{
    [ObservableProperty]
    private Guid _loadUuid;

    [ObservableProperty]
    private string _partId = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private string _dunnageType = string.Empty;

    [ObservableProperty]
    private int? _typeId;

    [ObservableProperty]
    private Dictionary<string, object> _specs = new();

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private string _homeLocation = string.Empty;

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TypeIconKind))]
    private string _typeIcon = "Help";

    /// <summary>
    /// Gets the MaterialIconKind for the dunnage type
    /// </summary>
    public MaterialIconKind TypeIconKind
    {
        get
        {
            if (!string.IsNullOrEmpty(TypeIcon) && Enum.TryParse<MaterialIconKind>(TypeIcon, true, out var kind))
            {
                return kind;
            }
            return MaterialIconKind.PackageVariantClosed;
        }
    }

    [ObservableProperty]
    private string _inventoryMethod = "Adjust In";

    [ObservableProperty]
    private Dictionary<string, object>? _specValues;

    [ObservableProperty]
    private DateTime _receivedDate = DateTime.Now;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private string? _modifiedBy;

    [ObservableProperty]
    private DateTime? _modifiedDate;

    [ObservableProperty]
    private string? _labelNumber;

    [ObservableProperty]
    private int _loadNumber;

    [ObservableProperty]
    private bool _isSelected;
}
