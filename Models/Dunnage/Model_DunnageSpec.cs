using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text.Json;

namespace MTM_Receiving_Application.Models.Dunnage;

public class Model_DunnageSpec : INotifyPropertyChanged
{
    private int _id;
    private int _typeId;
    private string _specKey = string.Empty;
    private string _specValue = string.Empty; // JSON string
    private Dictionary<string, object> _specsDefinition = new();
    private string _createdBy = string.Empty;
    private DateTime _createdDate = DateTime.Now;
    private string? _modifiedBy;
    private DateTime? _modifiedDate;

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public int TypeId
    {
        get => _typeId;
        set => SetField(ref _typeId, value);
    }

    public string SpecKey
    {
        get => _specKey;
        set => SetField(ref _specKey, value);
    }

    public string SpecValue
    {
        get => _specValue;
        set
        {
            if (SetField(ref _specValue, value))
            {
                DeserializeSpecs();
            }
        }
    }

    public Dictionary<string, object> SpecsDefinition
    {
        get => _specsDefinition;
        set => SetField(ref _specsDefinition, value);
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

    private void DeserializeSpecs()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SpecValue))
            {
                SpecsDefinition = new Dictionary<string, object>();
                return;
            }
            
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(SpecValue);
            SpecsDefinition = dict ?? new Dictionary<string, object>();
        }
        catch
        {
            SpecsDefinition = new Dictionary<string, object>();
        }
    }
}
