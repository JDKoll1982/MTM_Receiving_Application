using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.DunnageModule.Models;

/// <summary>
/// Represents a dynamic spec input field for dunnage label generation
/// </summary>
public partial class Model_SpecInput : ObservableObject
{
    [ObservableProperty]
    private string _specName = string.Empty;

    [ObservableProperty]
    private string _specType = "text"; // "text", "number", "boolean"

    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private string? _unit;

    [ObservableProperty]
    private bool _isRequired;
}
