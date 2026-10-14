using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Settings page for Heat / Lot Management: the feature switch, collision behaviour, how a marking
/// reads, the palette entries on offer, and the part list. This file only supplies the fuzzy-picker
/// dialog and forwards the Enter key to the ViewModel.
/// </summary>
public sealed partial class View_Settings_LotMarking : Page
{
    public ViewModel_Settings_LotMarking ViewModel { get; }

    public View_Settings_LotMarking(ViewModel_Settings_LotMarking viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
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

    private void PartSearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.AddPartCommand.CanExecute(null))
        {
            ViewModel.AddPartCommand.Execute(null);
        }
    }
}
