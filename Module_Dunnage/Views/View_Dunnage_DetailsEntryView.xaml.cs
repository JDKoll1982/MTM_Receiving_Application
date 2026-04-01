using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_DetailsEntryView : UserControl
{
    public ViewModel_Dunnage_DetailsEntry ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_DetailsEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_DetailsEntry>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSpecsForSelectedPartAsync();
    }

    private async void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        ViewModel.Location = textBox.Text.Trim();

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
                "Select Location",
                $"No exact match was found for '{ViewModel.Location?.Trim()}'. Select a matching location."
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
                ViewModel.Location = dialog.SelectedResult.Label.Trim();
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
