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
