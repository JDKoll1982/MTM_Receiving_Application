using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// The part-number list, hosted inside its own modal window. The list decides which material the
/// heat / lot tool answers for, so it is edited away from the lot list rather than beside it, where a
/// stray click could quietly change what the list covers.
/// </summary>
public sealed partial class View_Tool_LotMarkingPartsDialog : Page
{
    public ViewModel_Tool_LotMarking ViewModel { get; }

    public View_Tool_LotMarkingPartsDialog(ViewModel_Tool_LotMarking viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ShowFuzzyPickerAsync = ShowFuzzyPickerDialogAsync;
        ViewModel.ConfirmBackfillAsync = ConfirmBackfillDialogAsync;
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

    private async Task<bool> ConfirmBackfillDialogAsync(
        string partId,
        Model_Tool_LotMarkingBackfillPreview preview
    )
    {
        var dialog = new Dialog_LotMarkingBackfillConfirm(partId, preview)
        {
            XamlRoot = XamlRoot,
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private void PartSearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SearchPartsCommand.CanExecute(null))
        {
            ViewModel.SearchPartsCommand.Execute(null);
        }
    }

    /// <summary>
    /// Sends a row's own Remove button to the ViewModel. The row travels on the button's Tag, so
    /// nothing here decides what removing a part means — it only forwards the row it was given.
    /// </summary>
    private async void RemovePartRowButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Model_Tool_LotMarkingScope row)
        {
            await ViewModel.RemovePartRowAsync(row);
        }
    }
}
