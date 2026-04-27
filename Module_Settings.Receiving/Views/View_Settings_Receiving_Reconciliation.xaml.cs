using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_Reconciliation : Page
{
    public ViewModel_Settings_Receiving_PartFormatting ViewModel { get; }

    public View_Settings_Receiving_Reconciliation(
        ViewModel_Settings_Receiving_PartFormatting viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void IgnoredLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        await ValidateAndAddIgnoredLocationAsync(sender as TextBox);
    }

    private async void AddIgnoredLocationButton_Click(object sender, RoutedEventArgs e)
    {
        await ValidateAndAddIgnoredLocationAsync(IgnoredLocationTextBox);
    }

    private async Task ValidateAndAddIgnoredLocationAsync(TextBox? textBox)
    {
        if (textBox == null || string.IsNullOrWhiteSpace(ViewModel.PendingIgnoredLocation))
        {
            return;
        }

        var validation = await ViewModel.ValidateIgnoredLocationAsync();
        if (validation.IsValid)
        {
            if (
                ViewModel.TryAddIgnoredLocation(
                    ViewModel.PendingIgnoredLocation,
                    out var successMessage
                )
            )
            {
                ViewModel.ShowStatus(
                    successMessage,
                    Module_Core.Models.Enums.InfoBarSeverity.Success
                );
            }
            else if (!string.IsNullOrWhiteSpace(successMessage))
            {
                ViewModel.ShowStatus(
                    successMessage,
                    Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
            }

            return;
        }

        var suggestionsResult = await ViewModel.GetIgnoredLocationSuggestionsAsync();
        if (suggestionsResult.IsSuccess && suggestionsResult.Data?.Count > 0)
        {
            var dialog = new Dialog_FuzzySearchPicker(
                suggestionsResult.Data,
                "Select Ignored Reconciliation Location",
                $"No exact match was found for '{ViewModel.PendingIgnoredLocation?.Trim()}'. Select a matching location."
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
                if (
                    ViewModel.TryAddIgnoredLocation(
                        dialog.SelectedResult.Label.Trim(),
                        out var successMessage
                    )
                )
                {
                    ViewModel.ShowStatus(
                        successMessage,
                        Module_Core.Models.Enums.InfoBarSeverity.Success
                    );
                }
                else if (!string.IsNullOrWhiteSpace(successMessage))
                {
                    ViewModel.ShowStatus(
                        successMessage,
                        Module_Core.Models.Enums.InfoBarSeverity.Warning
                    );
                }

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
