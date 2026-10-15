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
/// Heat / Lot Management tool view. Business logic stays in the ViewModel; this file only supplies
/// the fuzzy-picker and confirmation dialogs, the help call, and the Enter key on the two
/// search boxes.
/// </summary>
public sealed partial class View_Tool_LotMarking : Page
{
    public ViewModel_Tool_LotMarking ViewModel { get; }

    public View_Tool_LotMarking(ViewModel_Tool_LotMarking viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ShowFuzzyPickerAsync = ShowFuzzyPickerDialogAsync;
        ViewModel.ConfirmBackfillAsync = ConfirmBackfillDialogAsync;
        ViewModel.ConfirmRealignAsync = ConfirmRealignDialogAsync;
    }

    private async void OnHelpClick(object sender, RoutedEventArgs e)
    {
        var helpService = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Help>();
        await helpService.ShowHelpAsync("ShipRecTools.LotMarking", XamlRoot);
    }

    private async void OnShowPaletteClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Dialog_LotMarkingPalette(
            ViewModel.LegendShapes,
            ViewModel.LegendColors,
            ViewModel.PaletteSummary
        )
        {
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
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

    private async Task<bool> ConfirmRealignDialogAsync(Model_Tool_LotMarkingRealignPreview preview)
    {
        var dialog = new Dialog_LotMarkingRealignConfirm(preview) { XamlRoot = XamlRoot };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private void HeatSearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SearchMarkingsCommand.CanExecute(null))
        {
            ViewModel.SearchMarkingsCommand.Execute(null);
        }
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
