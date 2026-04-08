using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Read-only card-based material availability board view.
/// </summary>
public sealed partial class View_Tool_MaterialAvailabilityBoard : Page
{
    public ViewModel_Tool_MaterialAvailabilityBoard ViewModel { get; }

    public View_Tool_MaterialAvailabilityBoard(ViewModel_Tool_MaterialAvailabilityBoard viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ShowFuzzyPickerAsync = ShowFuzzyPickerDialogAsync;
    }

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
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? dialog.SelectedResult : null;
    }

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.SearchCommand.CanExecute(null))
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ClearCommand.CanExecute(null))
        {
            ViewModel.ClearCommand.Execute(null);
        }

        SearchBox.DispatcherQueue?.TryEnqueue(() => SearchBox.Focus(FocusState.Programmatic));
    }
}
