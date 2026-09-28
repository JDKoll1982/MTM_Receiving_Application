using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Dunnage.Helpers;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

public class Model_DunnagePart : INotifyPropertyChanged
{
    private int _id;
    private string _partId = string.Empty;
    private int _typeId;
    private readonly string?[] _udcValues = new string?[10];
    private string _dunnageTypeName = string.Empty;
    private string _quantityType = "Quantity";
    private string? _imagePath;
    private string? _dunnageTypeImagePath;
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

    /// <summary>User Defined Column slot 1 - display name from dunnage_custom_fields.</summary>
    public string? Udc1 { get => _udcValues[0]; set => SetField(ref _udcValues[0], value); }
    public string? Udc2 { get => _udcValues[1]; set => SetField(ref _udcValues[1], value); }
    public string? Udc3 { get => _udcValues[2]; set => SetField(ref _udcValues[2], value); }
    public string? Udc4 { get => _udcValues[3]; set => SetField(ref _udcValues[3], value); }
    public string? Udc5 { get => _udcValues[4]; set => SetField(ref _udcValues[4], value); }
    public string? Udc6 { get => _udcValues[5]; set => SetField(ref _udcValues[5], value); }
    public string? Udc7 { get => _udcValues[6]; set => SetField(ref _udcValues[6], value); }
    public string? Udc8 { get => _udcValues[7]; set => SetField(ref _udcValues[7], value); }
    public string? Udc9 { get => _udcValues[8]; set => SetField(ref _udcValues[8], value); }
    public string? Udc10 { get => _udcValues[9]; set => SetField(ref _udcValues[9], value); }

    public string? GetUdcValue(int slot) =>
        slot >= 1 && slot <= 10 ? _udcValues[slot - 1] : null;

    public void SetUdcValue(int slot, string? value)
    {
        if (slot >= 1 && slot <= 10)
        {
            SetField(ref _udcValues[slot - 1], value);
        }
    }

    /// <summary>
    /// Builds the labeled FieldName -> udc value pairs for this part using the
    /// type's custom-field definitions (DisplayOrder is the udc slot).
    /// </summary>
    public IEnumerable<KeyValuePair<string, string?>> BuildLabeledValues(
        IEnumerable<Model_CustomFieldDefinition> fields
    )
    {
        foreach (var field in fields)
        {
            var value = GetUdcValue(field.DisplayOrder);
            if (value is not null)
            {
                yield return new KeyValuePair<string, string?>(field.FieldName, value);
            }
        }
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

    public string QuantityType
    {
        get => _quantityType;
        set =>
            SetField(
                ref _quantityType,
                string.IsNullOrWhiteSpace(value) ? "Quantity" : value.Trim()
            );
    }

    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            if (SetField(ref _imagePath, value))
            {
                OnPropertyChanged(nameof(HasImagePath));
                OnPropertyChanged(nameof(ImageSource));
                OnPropertyChanged(nameof(HasPreferredVisual));
                OnPropertyChanged(nameof(PreferredVisualSource));
            }
        }
    }

    public string? DunnageTypeImagePath
    {
        get => _dunnageTypeImagePath;
        set
        {
            if (SetField(ref _dunnageTypeImagePath, value))
            {
                OnPropertyChanged(nameof(HasDunnageTypeImagePath));
                OnPropertyChanged(nameof(DunnageTypeImageSource));
                OnPropertyChanged(nameof(HasPreferredVisual));
                OnPropertyChanged(nameof(PreferredVisualSource));
            }
        }
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

    public bool HasImagePath => string.IsNullOrWhiteSpace(ImagePath) is false;

    public bool HasDunnageTypeImagePath => string.IsNullOrWhiteSpace(DunnageTypeImagePath) is false;

    public ImageSource? ImageSource => Helper_DunnageImagePaths.CreateImageSource(ImagePath);

    public ImageSource? DunnageTypeImageSource =>
        Helper_DunnageImagePaths.CreateImageSource(DunnageTypeImagePath);

    public ImageSource? PreferredVisualSource => ImageSource ?? DunnageTypeImageSource;

    public bool HasPreferredVisual => PreferredVisualSource is not null;

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
