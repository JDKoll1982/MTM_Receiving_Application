using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTM_Receiving_Application.Models.Dunnage;

/// <summary>
/// Represents an icon definition for the Add Type Dialog icon picker.
/// Contains Segoe Fluent Icons glyph codes and metadata.
/// </summary>
public class Model_IconDefinition : INotifyPropertyChanged
{
    private string _glyph = string.Empty;
    private string _name = string.Empty;
    private string _category = string.Empty;
    private int _usageCount;
    private bool _isRecentlyUsed;

    /// <summary>
    /// Gets or sets the Unicode glyph code (e.g., "\uE7B8" for Box icon)
    /// </summary>
    public string Glyph
    {
        get => _glyph;
        set => SetField(ref _glyph, value);
    }

    /// <summary>
    /// Gets or sets the icon name (e.g., "Box", "Package", "Clipboard")
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Gets or sets the icon category for filtering (e.g., "Objects", "Actions", "Symbols")
    /// </summary>
    public string Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    /// <summary>
    /// Gets or sets the usage count for popularity sorting
    /// </summary>
    public int UsageCount
    {
        get => _usageCount;
        set => SetField(ref _usageCount, value);
    }

    /// <summary>
    /// Gets or sets whether this icon was recently used by current user
    /// </summary>
    public bool IsRecentlyUsed
    {
        get => _isRecentlyUsed;
        set => SetField(ref _isRecentlyUsed, value);
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
