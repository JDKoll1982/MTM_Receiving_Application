using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.ViewModels.Dunnage.Helpers;

/// <summary>
/// Helper ViewModel for dynamic specification input controls in Details Entry view.
/// Represents a single spec field with data type, value, and metadata.
/// </summary>
public partial class SpecInputViewModel : ObservableObject
{
    [ObservableProperty]
    private string _specName = string.Empty;

    [ObservableProperty]
    private string _dataType = string.Empty; // "text", "number", "boolean"

    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private string _unit = string.Empty; // e.g., "inches", "lbs"

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private object? _defaultValue;
}
