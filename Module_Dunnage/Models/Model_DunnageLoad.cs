using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Dunnage.Helpers;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Represents a received dunnage load transaction.
/// Database Table: dunnage_history
/// </summary>
public partial class Model_DunnageLoad : ObservableObject
{
    [ObservableProperty]
    private int _queueRowId;

    [ObservableProperty]
    private Guid _loadUuid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PartIdButtonDisplayText))]
    private string _partId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(QuantityWholeNumber))]
    [NotifyPropertyChangedFor(nameof(QuantityDisplay))]
    private decimal _quantity;

    [ObservableProperty]
    private string _quantityType = "Quantity";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditablePoNumber))]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private string _dunnageType = string.Empty;

    [ObservableProperty]
    private int? _typeId;

    private readonly string?[] _udcValues = new string?[10];

    public string? Udc1 { get => _udcValues[0]; set => SetProperty(ref _udcValues[0], value); }
    public string? Udc2 { get => _udcValues[1]; set => SetProperty(ref _udcValues[1], value); }
    public string? Udc3 { get => _udcValues[2]; set => SetProperty(ref _udcValues[2], value); }
    public string? Udc4 { get => _udcValues[3]; set => SetProperty(ref _udcValues[3], value); }
    public string? Udc5 { get => _udcValues[4]; set => SetProperty(ref _udcValues[4], value); }
    public string? Udc6 { get => _udcValues[5]; set => SetProperty(ref _udcValues[5], value); }
    public string? Udc7 { get => _udcValues[6]; set => SetProperty(ref _udcValues[6], value); }
    public string? Udc8 { get => _udcValues[7]; set => SetProperty(ref _udcValues[7], value); }
    public string? Udc9 { get => _udcValues[8]; set => SetProperty(ref _udcValues[8], value); }
    public string? Udc10 { get => _udcValues[9]; set => SetProperty(ref _udcValues[9], value); }

    public string? GetUdcValue(int slot) =>
        slot >= 1 && slot <= 10 ? _udcValues[slot - 1] : null;

    public void SetUdcValue(int slot, string? value)
    {
        if (slot >= 1 && slot <= 10)
        {
            SetProperty(ref _udcValues[slot - 1], value);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpecSummaryDisplay))]
    private IReadOnlyList<Model_CustomFieldDefinition>? _udcFields;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocationButtonDisplayText))]
    private string? _location = string.Empty;

    [ObservableProperty]
    private string? _homeLocation = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TypeButtonDisplayText))]
    private string _typeName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TypeIconKind))]
    private string _typeIcon = "Help";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTypeImagePath))]
    [NotifyPropertyChangedFor(nameof(TypeImageSource))]
    private string? _typeImagePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPartImagePath))]
    [NotifyPropertyChangedFor(nameof(PartImageSource))]
    private string? _partImagePath;

    /// <summary>
    /// Gets the MaterialIconKind for the dunnage type
    /// </summary>
    public MaterialIconKind TypeIconKind
    {
        get
        {
            if (
                !string.IsNullOrEmpty(TypeIcon)
                && Enum.TryParse<MaterialIconKind>(TypeIcon, true, out var kind)
            )
            {
                return kind;
            }
            return MaterialIconKind.PackageVariantClosed;
        }
    }

    public bool HasTypeImagePath => string.IsNullOrWhiteSpace(TypeImagePath) is false;

    public bool HasPartImagePath => string.IsNullOrWhiteSpace(PartImagePath) is false;

    public ImageSource? TypeImageSource =>
        Helper_DunnageImagePaths.CreateImageSource(TypeImagePath);

    public ImageSource? PartImageSource =>
        Helper_DunnageImagePaths.CreateImageSource(PartImagePath);

    [ObservableProperty]
    private string _inventoryMethod = "Adjust In";

    [ObservableProperty]
    private DateTime _receivedDate = DateTime.Now;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    [ObservableProperty]
    private int? _employeeNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CreatedDateDisplay))]
    [NotifyPropertyChangedFor(nameof(CreatedTimeDisplay))]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private string? _modifiedBy;

    [ObservableProperty]
    private DateTime? _modifiedDate;

    [ObservableProperty]
    private string? _labelNumber;

    [ObservableProperty]
    private int? _partSkidSequence;

    [ObservableProperty]
    private int? _partSkidTotal;

    [ObservableProperty]
    private int _loadNumber;

    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets the text shown for the Type selector button.
    /// </summary>
    public string TypeButtonDisplayText =>
        string.IsNullOrWhiteSpace(TypeName) ? "Select Type" : TypeName;

    /// <summary>
    /// Gets the text shown for the Part ID selector button.
    /// </summary>
    public string PartIdButtonDisplayText =>
        string.IsNullOrWhiteSpace(PartId) ? "Select Part ID" : PartId;

    /// <summary>
    /// Gets the text shown for the Location selector button.
    /// </summary>
    public string LocationButtonDisplayText =>
        string.IsNullOrWhiteSpace(Location) ? "Select Location" : Location!;

    /// <summary>
    /// Gets or sets the quantity as a whole number for edit-mode display and editing.
    /// </summary>
    public int QuantityWholeNumber
    {
        get => decimal.ToInt32(decimal.Truncate(Quantity));
        set => Quantity = Math.Max(0, value);
    }

    /// <summary>
    /// Gets the quantity formatted without decimal places.
    /// </summary>
    public string QuantityDisplay => QuantityWholeNumber.ToString(CultureInfo.InvariantCulture);

    public string LoadUuidDisplay => LoadUuid == Guid.Empty ? string.Empty : LoadUuid.ToString();

    /// <summary>
    /// Gets or sets the PO number through the Edit Mode formatter.
    /// </summary>
    public string EditablePoNumber
    {
        get => PoNumber;
        set => PoNumber = NormalizePoNumber(value);
    }

    /// <summary>
    /// Gets the created date formatted as a date-only string.
    /// </summary>
    public string CreatedDateDisplay =>
        CreatedDate.ToString("M/d/yyyy", CultureInfo.InvariantCulture);

    public string CreatedDateTimeDisplay =>
        CreatedDate == default
            ? string.Empty
            : CreatedDate
                .ToString("M/d/yyyy h:mm tt", CultureInfo.InvariantCulture)
                .Replace("AM", "A.M.", StringComparison.Ordinal)
                .Replace("PM", "P.M.", StringComparison.Ordinal);

    /// <summary>
    /// Gets the created time formatted in 12-hour AM/PM style.
    /// </summary>
    public string CreatedTimeDisplay =>
        CreatedDate
            .ToString("h:mm tt", CultureInfo.InvariantCulture)
            .Replace("AM", "A.M.", StringComparison.Ordinal)
            .Replace("PM", "P.M.", StringComparison.Ordinal);

    public string ModifiedDateTimeDisplay =>
        ModifiedDate.HasValue
            ? ModifiedDate
                .Value.ToString("M/d/yyyy h:mm tt", CultureInfo.InvariantCulture)
                .Replace("AM", "A.M.", StringComparison.Ordinal)
                .Replace("PM", "P.M.", StringComparison.Ordinal)
            : string.Empty;

    public string ReceivedDateTimeDisplay =>
        ReceivedDate == default
            ? string.Empty
            : ReceivedDate
                .ToString("M/d/yyyy h:mm tt", CultureInfo.InvariantCulture)
                .Replace("AM", "A.M.", StringComparison.Ordinal)
                .Replace("PM", "P.M.", StringComparison.Ordinal);

    public string SpecSummaryDisplay
    {
        get
        {
            var pairs = new List<string>();
            var labels = UdcFields;
            if (labels is { Count: > 0 })
            {
                foreach (var customField in labels)
                {
                    var value = GetUdcValue(customField.DisplayOrder);
                    if (value is not null)
                    {
                        pairs.Add($"{customField.FieldName}: {value}");
                    }
                }
            }
            else
            {
                for (var i = 1; i <= 10; i++)
                {
                    var value = GetUdcValue(i);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        pairs.Add($"UDC{i}: {value}");
                    }
                }
            }

            return pairs.Count > 0 ? string.Join(" | ", pairs) : "No specs";
        }
    }

    public string PartSkidDisplay =>
        PartSkidSequence.HasValue && PartSkidTotal.HasValue
            ? $"{PartSkidSequence.Value} of {PartSkidTotal.Value}"
            : string.Empty;

    private static string NormalizePoNumber(string? value)
    {
        return Helper_DunnagePoNumber.FormatForEntry(value);
    }

    partial void OnPoNumberChanged(string value)
    {
        var normalizedPoNumber = Helper_DunnagePoNumber.FormatForEntry(value);
        if (string.Equals(value, normalizedPoNumber, StringComparison.Ordinal))
        {
            return;
        }

        PoNumber = normalizedPoNumber;
    }
}
