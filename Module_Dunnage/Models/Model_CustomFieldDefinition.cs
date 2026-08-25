using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Represents a user-defined custom field (UDC slot) for dunnage types.
/// Corresponds to dunnage_custom_fields table.
/// </summary>
public partial class Model_CustomFieldDefinition : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _dunnageTypeId;

    [ObservableProperty]
    private string _fieldName = string.Empty;

    [ObservableProperty]
    private string _fieldType = "Text";

    [ObservableProperty]
    private int _displayOrder;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private string? _unit;

    [ObservableProperty]
    private decimal? _minValue;

    [ObservableProperty]
    private decimal? _maxValue;

    [ObservableProperty]
    private string? _defaultValue;

    [ObservableProperty]
    private string? _validationRules;

    [ObservableProperty]
    private List<string> _choices = new();

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    /// <summary>
    /// Gets the udc slot column name (udc1..udc10) that backs this field.
    /// DisplayOrder is 1-based; values outside 1-10 map to an empty string.
    /// </summary>
    public string UdcColumnName =>
        DisplayOrder is >= 1 and <= 10 ? $"udc{DisplayOrder}" : string.Empty;

    /// <summary>
    /// Returns a summary string for display in the UI
    /// </summary>
    public string GetSummary()
    {
        string required = IsRequired ? " (Required)" : string.Empty;
        return $"{FieldType}{required}";
    }
}
