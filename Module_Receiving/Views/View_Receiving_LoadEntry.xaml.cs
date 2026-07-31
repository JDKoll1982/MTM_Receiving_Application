using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_LoadEntry : UserControl, IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_LoadEntry ViewModel { get; }

        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public double ViewportWidth { get; private set; } = double.MaxValue;

        public double ViewportHeight { get; private set; } = double.MaxValue;

        public View_Receiving_LoadEntry(
            ViewModel_Receiving_LoadEntry viewModel,
            IService_Focus focusService,
            IService_AdaptiveLayout adaptiveLayout
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);
            ArgumentNullException.ThrowIfNull(adaptiveLayout);

            ViewModel = viewModel;
            _focusService = focusService;
            _adaptiveLayout = adaptiveLayout;
            DataContext = ViewModel;
            InitializeComponent();

            SizeChanged += View_Receiving_LoadEntry_SizeChanged;
            UpdateViewportBounds();

            _focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
        }

        private void View_Receiving_LoadEntry_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewportBounds();
        }

        private void UpdateViewportBounds()
        {
            ViewportWidth = ActualWidth > 0 ? ActualWidth : double.MaxValue;
            ViewportHeight = ActualHeight > 0 ? ActualHeight : double.MaxValue;

            ReceivingLoadEntryRootGrid.Padding = _adaptiveLayout.GetReceivingContentPadding(
                ActualWidth
            );

            var state = _adaptiveLayout.ResolveReceivingLayoutState(ActualWidth);
            _ = VisualStateManager.GoToState(this, state, false);

            var recommendedLocationsBoundedHeight = _adaptiveLayout.CalculateBoundedViewportHeight(
                containerHeightEpx: ActualHeight,
                occupiedHeightsEpx:
                [
                    LoadEntrySummaryBorder.ActualHeight,
                    RecommendedLocationsHeaderTextBlock.ActualHeight,
                    RecommendedLocationsProgressRing.ActualHeight,
                    96,
                ]
            );

            RecommendedLocationsScrollViewer.MaxHeight = recommendedLocationsBoundedHeight;
            Bindings.Update();
        }

        /// <summary>
        /// Moves focus to the primary load-count input whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            _focusService.SetFocus(NumberOfLoadsNumberBox);
        }

        private async void LocationLookupControl_ValidationCompleted(
            object sender,
            Model_SharedLookupValidationCompletedEventArgs e
        )
        {
            if (!e.Result.IsValid)
            {
                ViewModel.ShowStatus(
                    e.Result.Message,
                    MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
                return;
            }

            if (e.Result.UsedFuzzyFallback && e.Result.HasExactMatch is false)
            {
                var selectedResult = await ShowFuzzyPickerAsync(
                    e.Result.FormattedValue,
                    e.Result.FuzzyCandidates
                );

                if (selectedResult is null)
                {
                    LocationLookupControl.InputValue = string.Empty;
                    ViewModel.Location = string.Empty;
                    return;
                }

                var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                    .Trim();

                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    LocationLookupControl.InputValue = string.Empty;
                    ViewModel.Location = string.Empty;
                    return;
                }

                LocationLookupControl.InputValue = selectedValue;
                ViewModel.Location = selectedValue;
                await LocationLookupControl.ValidateAsync();
            }
        }

        private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerAsync(
            string searchTerm,
            IReadOnlyList<Model_FuzzySearchResult> items
        )
        {
            if (items.Count == 0)
            {
                return null;
            }

            var xamlRoot = XamlRoot;
            if (xamlRoot is null)
            {
                return null;
            }

            var dialog = new Dialog_FuzzySearchPicker(
                items,
                "Select Location",
                $"No exact location match was found for '{searchTerm}'. Select a similar location to continue."
            )
            {
                XamlRoot = xamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            return dialog.SelectedResult;
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
