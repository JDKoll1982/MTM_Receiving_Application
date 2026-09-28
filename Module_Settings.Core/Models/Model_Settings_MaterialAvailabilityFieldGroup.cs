using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Collapsible group of field options shown on the Material Availability settings page.
/// </summary>
public sealed partial class Model_Settings_MaterialAvailabilityFieldGroup : ObservableObject
{
    public Model_Settings_MaterialAvailabilityFieldGroup(string title)
    {
        Title = title;
    }

    public string Title { get; }

    public ObservableCollection<Model_Settings_MaterialAvailabilityFieldOption> Fields { get; } =
    [];

    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>
    /// Selection counts surfaced in the always-visible header so collapsed groups still report
    /// which of their fields are shown in the UI and included in print.
    /// </summary>
    public string SelectionSummary =>
        $"{Fields.Count(option => option.IsVisibleInUi)} of {Fields.Count} in UI · "
        + $"{Fields.Count(option => option.IsVisibleInPrint)} of {Fields.Count} in print";

    /// <summary>
    /// Adds a field option and tracks its checkbox changes so <see cref="SelectionSummary"/> stays current.
    /// </summary>
    public void AddField(Model_Settings_MaterialAvailabilityFieldOption option)
    {
        option.PropertyChanged += OnFieldPropertyChanged;
        Fields.Add(option);
    }

    private void OnFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName
                is nameof(Model_Settings_MaterialAvailabilityFieldOption.IsVisibleInUi)
                    or nameof(Model_Settings_MaterialAvailabilityFieldOption.IsVisibleInPrint)
        )
        {
            OnPropertyChanged(nameof(SelectionSummary));
        }
    }
}
