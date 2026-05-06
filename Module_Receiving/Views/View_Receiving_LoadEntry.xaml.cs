using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_LoadEntry : UserControl, IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_LoadEntry ViewModel { get; }

        private readonly IService_Focus _focusService;

        public View_Receiving_LoadEntry(
            ViewModel_Receiving_LoadEntry viewModel,
            IService_Focus focusService
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);

            ViewModel = viewModel;
            _focusService = focusService;
            DataContext = ViewModel;
            InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
        }

        /// <summary>
        /// Moves focus to the primary load-count input whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            _focusService.SetFocus(NumberOfLoadsNumberBox);
        }

        private async void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
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

        private void RecommendedLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (
                sender is not Button button
                || button.Tag is not Model_ReceivingRecommendedLocation location
            )
            {
                return;
            }

            ViewModel.ApplyRecommendedLocationCommand.Execute(location);
        }
    }
}
