using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text.Json;

namespace MTM_Receiving_Application.DunnageModule.Models;

public class Model_DunnagePart : INotifyPropertyChanged
{
    private int _id;
    private string _partId = string.Empty;
    private int _typeId;
    private string _specValues = string.Empty; // JSON string
    private Dictionary<string, object> _specValuesDict = new();
    private string _dunnageTypeName = string.Empty;
    private string _createdBy = string.Empty;
    private DateTime _createdDate = DateTime.Now;
    private string? _modifiedBy;
    private DateTime? _modifiedDate;

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string PartId
    {
        get => _partId;
        set => SetField(ref _partId, value);
    }

    public int TypeId
    {
        get => _typeId;
        set => SetField(ref _typeId, value);
    }

    public string SpecValues
    {
        get => _specValues;
        set
        {
            if (SetField(ref _specValues, value))
            {
                DeserializeSpecValues();
            }
        }
    }

    public Dictionary<string, object> SpecValuesDict
    {
        get => _specValuesDict;
        set => SetField(ref _specValuesDict, value);
    }

    private string _homeLocation = string.Empty;

    public string HomeLocation
    {
        get => _homeLocation;
        set => SetField(ref _homeLocation, value);
    }

    public string DunnageTypeName
    {
        get => _dunnageTypeName;
        set => SetField(ref _dunnageTypeName, value);
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

    private void DeserializeSpecValues()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SpecValues))
            {
                SpecValuesDict = new Dictionary<string, object>();
                return;
            }

            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(SpecValues);
            SpecValuesDict = dict ?? new Dictionary<string, object>();
        }
        catch
        {
            SpecValuesDict = new Dictionary<string, object>();
        }
    }
}
