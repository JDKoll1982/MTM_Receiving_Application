using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Dunnage;

public class Model_DunnageLoad : INotifyPropertyChanged
{
    private Guid _loadUuid;
    private string _partId = string.Empty;
    private decimal _quantity;
    private string _poNumber = string.Empty;
    private string _dunnageType = string.Empty;
    private Dictionary<string, object> _specs = new();
    private string _location = string.Empty;
    private string _typeName = string.Empty;
    private string _inventoryMethod = "Adjust In";
    private Dictionary<string, object>? _specValues;
    private DateTime _receivedDate = DateTime.Now;
    private string _createdBy = string.Empty;
    private DateTime _createdDate = DateTime.Now;
    private string? _modifiedBy;
    private DateTime? _modifiedDate;

    public Guid LoadUuid
    {
        get => _loadUuid;
        set => SetField(ref _loadUuid, value);
    }

    public string PartId
    {
        get => _partId;
        set => SetField(ref _partId, value);
    }

    public decimal Quantity
    {
        get => _quantity;
        set => SetField(ref _quantity, value);
    }

    public string PoNumber
    {
        get => _poNumber;
        set => SetField(ref _poNumber, value);
    }

    public string DunnageType
    {
        get => _dunnageType;
        set => SetField(ref _dunnageType, value);
    }

    public Dictionary<string, object> Specs
    {
        get => _specs;
        set => SetField(ref _specs, value);
    }

    public string Location
    {
        get => _location;
        set => SetField(ref _location, value);
    }

    public string TypeName
    {
        get => _typeName;
        set => SetField(ref _typeName, value);
    }

    public string InventoryMethod
    {
        get => _inventoryMethod;
        set => SetField(ref _inventoryMethod, value);
    }

    public Dictionary<string, object>? SpecValues
    {
        get => _specValues;
        set => SetField(ref _specValues, value);
    }

    public DateTime ReceivedDate
    {
        get => _receivedDate;
        set => SetField(ref _receivedDate, value);
    }

    public string CreatedBy
    {
        get => _createdBy;
        set => SetField(ref _createdBy, value);
    }

    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetField(ref _createdDate, value);
    }

    public string? ModifiedBy
    {
        get => _modifiedBy;
        set => SetField(ref _modifiedBy, value);
    }

    public DateTime? ModifiedDate
    {
        get => _modifiedDate;
        set => SetField(ref _modifiedDate, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
