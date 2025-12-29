using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Models.Dunnage;

/// <summary>
/// Represents an icon definition for the Add Type Dialog icon picker.
/// Contains Segoe Fluent Icons glyph codes and metadata.
/// </summary>
public partial class Model_IconDefinition : ObservableObject
{
    [ObservableProperty]
    private string _glyph = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int _usageCount;

    [ObservableProperty]
    private bool _isRecentlyUsed;

    // Alias for compatibility
    public string IconName
    {
        get => Name;
        set => Name = value;
    }
}
