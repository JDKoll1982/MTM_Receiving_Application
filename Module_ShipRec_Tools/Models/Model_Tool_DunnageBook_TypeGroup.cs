using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// A dunnage type group: a type name with its selectable parts and a group-level
/// include/exclude checkbox.
/// </summary>
public partial class Model_Tool_DunnageBook_TypeGroup : ObservableObject
{
    private bool _isUpdatingSelection;

    [ObservableProperty]
    private int _typeId;

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private bool _isTypeSelected;

    public ObservableCollection<Model_Tool_DunnageBook_Entry> Entries { get; } = [];

    public bool HasEntries => Entries.Count > 0;

    public int SelectedCount => Entries.Count(entry => entry.IsSelected);

    partial void OnIsTypeSelectedChanged(bool value)
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        _isUpdatingSelection = true;
        foreach (var entry in Entries)
        {
            entry.IsSelected = value;
        }

        OnPropertyChanged(nameof(SelectedCount));
        _isUpdatingSelection = false;
    }

    internal void NotifyEntrySelectionChanged()
    {
        if (_isUpdatingSelection)
        {
            return;
        }

        _isUpdatingSelection = true;
        IsTypeSelected = Entries.Count > 0 && Entries.All(entry => entry.IsSelected);
        OnPropertyChanged(nameof(SelectedCount));
        _isUpdatingSelection = false;
    }
}
