using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTM_Receiving_Application.Models.Dunnage;

/// <summary>
/// Represents a user-defined custom field for dunnage types.
/// Corresponds to custom_field_definitions table.
/// </summary>
public class Model_CustomFieldDefinition : INotifyPropertyChanged
{
    private int _id;
    private int _dunnageTypeId;
    private string _fieldName = string.Empty;
    private string _databaseColumnName = string.Empty;
    private string _fieldType = "Text";
    private int _displayOrder;
    private bool _isRequired;
    private string? _validationRules;
    private DateTime _createdDate = DateTime.Now;
    private string _createdBy = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    /// <summary>
    /// Gets or sets the dunnage type ID this field belongs to
    /// </summary>
    public int DunnageTypeId
    {
        get => _dunnageTypeId;
        set => SetField(ref _dunnageTypeId, value);
    }

    /// <summary>
    /// Gets or sets the display name of the field (e.g., "Weight (lbs)")
    /// </summary>
    public string FieldName
    {
        get => _fieldName;
        set => SetField(ref _fieldName, value);
    }

    /// <summary>
    /// Gets or sets the sanitized database column name (e.g., "weight_lbs")
    /// </summary>
    public string DatabaseColumnName
    {
        get => _databaseColumnName;
        set => SetField(ref _databaseColumnName, value);
    }

    /// <summary>
    /// Gets or sets the field type: Text, Number, Date, Boolean
    /// </summary>
    public string FieldType
    {
        get => _fieldType;
        set => SetField(ref _fieldType, value);
    }

    /// <summary>
    /// Gets or sets the display order in UI (1, 2, 3, ...)
    /// </summary>
    public int DisplayOrder
    {
        get => _displayOrder;
        set => SetField(ref _displayOrder, value);
    }

    /// <summary>
    /// Gets or sets whether field is mandatory during data entry
    /// </summary>
    public bool IsRequired
    {
        get => _isRequired;
        set => SetField(ref _isRequired, value);
    }

    /// <summary>
    /// Gets or sets the JSON validation rules (e.g., {"min": 1, "max": 9999, "decimals": 2})
    /// </summary>
    public string? ValidationRules
    {
        get => _validationRules;
        set => SetField(ref _validationRules, value);
    }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetField(ref _createdDate, value);
    }

    /// <summary>
    /// Gets or sets the username of the creator
    /// </summary>
    public string CreatedBy
    {
        get => _createdBy;
        set => SetField(ref _createdBy, value);
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
