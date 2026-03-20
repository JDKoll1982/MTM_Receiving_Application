using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Outside Service Provider History lookup tool view.
/// Displays dispatch history for a part number or vendor retrieved from Infor Visual (read-only).
/// </summary>
public sealed partial class View_Tool_OutsideServiceHistory : Page
{
    public ViewModel_Tool_OutsideServiceHistory ViewModel { get; }

    public View_Tool_OutsideServiceHistory(ViewModel_Tool_OutsideServiceHistory viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        this.InitializeComponent();

        ViewModel.ShowFuzzyPickerAsync = ShowFuzzyPickerDialogAsync;
        ViewModel.ResetSortIndicators = ResetAllColumnSortDirections;
    }

    // ─── Fuzzy Picker Delegate ───────────────────────────────────────────────

    private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerDialogAsync(
        IReadOnlyList<Model_FuzzySearchResult> candidates,
        string title
    )
    {
        var dialog = new Dialog_FuzzySearchPicker(
            candidates,
            title,
            subtitle: $"{candidates.Count} possible matches — select the correct one."
        )
        {
            XamlRoot = this.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? dialog.SelectedResult : null;
    }

    // ─── Keyboard Shortcut ───────────────────────────────────────────────────

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.SearchCommand.CanExecute(null))
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    // ─── Column Sorting ─────────────────────────────────────────────────

    private void ResultsGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        var propertyName = e.Column.Tag as string;
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        bool ascending = ViewModel.SortBy(propertyName);

        foreach (var col in ResultsGrid.Columns)
        {
            col.SortDirection = null;
        }

        e.Column.SortDirection = ascending
            ? DataGridSortDirection.Ascending
            : DataGridSortDirection.Descending;
    }

    private void ResetAllColumnSortDirections()
    {
        foreach (var col in ResultsGrid.Columns)
        {
            col.SortDirection = null;
        }
    }
}
