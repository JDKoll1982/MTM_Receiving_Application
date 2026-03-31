using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_UserPreferences : Page
{
    public ViewModel_Settings_Dunnage_UserPreferences ViewModel { get; }

    public View_Settings_Dunnage_UserPreferences(
        ViewModel_Settings_Dunnage_UserPreferences viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void DefaultLocationTextBox_LostFocus(
        object sender,
        Microsoft.UI.Xaml.RoutedEventArgs e
    )
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var validation = await ViewModel.ValidateLocationAsync();
        if (validation.IsValid)
        {
            return;
        }

        var suggestionsResult = await ViewModel.GetLocationSuggestionsAsync();
        if (suggestionsResult.IsSuccess && suggestionsResult.Data?.Count > 0)
        {
            var dialog = new Dialog_FuzzySearchPicker(
                suggestionsResult.Data,
                "Select Default Dunnage Location",
                $"No exact match was found for '{ViewModel.DefaultLocation?.Trim()}'. Select a matching location."
            )
            {
                XamlRoot = textBox.XamlRoot,
            };

            var dialogResult = await dialog.ShowAsync();
            if (
                dialogResult == ContentDialogResult.Primary
                && dialog.SelectedResult is not null
                && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
            )
            {
                ViewModel.DefaultLocation = dialog.SelectedResult.Label.Trim();
                return;
            }
        }

        var statusMessage = validation.Message;
        if (
            !suggestionsResult.IsSuccess
            && string.IsNullOrWhiteSpace(suggestionsResult.ErrorMessage) is false
        )
        {
            statusMessage = $"{validation.Message} {suggestionsResult.ErrorMessage}";
        }

        ViewModel.ShowStatus(statusMessage, Module_Core.Models.Enums.InfoBarSeverity.Warning);
    }
}
