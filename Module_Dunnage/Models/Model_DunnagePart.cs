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
    private string _specValues = string.Empty; // JSON string
    private Dictionary<string, object> _specValuesDict = new();
    private Dictionary<string, SpecDefinition> _partSpecificSpecDefinitions = new();
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

    public Dictionary<string, SpecDefinition> PartSpecificSpecDefinitions
    {
        get => _partSpecificSpecDefinitions;
        set => SetField(ref _partSpecificSpecDefinitions, value);
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

    public string DunnageSpecValuesJson =>
        string.IsNullOrWhiteSpace(SpecValues) ? "{}" : SpecValues;

    public bool UsesDefinitionBasedPartSpecificSpecs => PartSpecificSpecDefinitions.Count > 0;

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
        var rawElements = Helper_Dunnage_PartSpecs.DeserializeRawElements(SpecValues);
        if (rawElements.Count == 0)
        {
            SpecValuesDict = new Dictionary<string, object>();
            PartSpecificSpecDefinitions = new Dictionary<string, SpecDefinition>();
            return;
        }

        var scalarValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var definitionValues = new Dictionary<string, SpecDefinition>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var pair in rawElements)
        {
            if (Helper_Dunnage_PartSpecs.TryGetSpecDefinition(pair.Value, out var definition))
            {
                definitionValues[pair.Key] = definition;
                continue;
            }

            var plainValue = Helper_Dunnage_PartSpecs.ConvertJsonElementToPlainObject(pair.Value);
            if (
                string.Equals(pair.Key, "image_path", StringComparison.OrdinalIgnoreCase)
                && plainValue is string imagePath
                && string.IsNullOrWhiteSpace(ImagePath)
            )
            {
                ImagePath = imagePath;
                continue;
            }

            if (plainValue is not null)
            {
                scalarValues[pair.Key] = plainValue;
            }
        }

        SpecValuesDict = scalarValues;
        PartSpecificSpecDefinitions = definitionValues;
    }
}
