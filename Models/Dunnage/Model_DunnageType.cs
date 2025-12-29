using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTM_Receiving_Application.Models.Dunnage;

public class Model_DunnageType : INotifyPropertyChanged
{
    private int _id;
    private string _typeName = string.Empty;
    private string _icon = "\uDB81\uDF20"; // Default to box icon (Fluent System Icons)
    private string _specsJson = string.Empty;
    private string _createdBy = string.Empty;
    private DateTime _createdDate = DateTime.Now;
    private string? _modifiedBy;
    private DateTime? _modifiedDate;

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string TypeName
    {
        get => _typeName;
        set => SetField(ref _typeName, value);
    }

    public string Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public string SpecsJson
    {
        get => _specsJson;
        set => SetField(ref _specsJson, value);
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
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
