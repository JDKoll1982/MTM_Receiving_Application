using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using System;
using MTM_Receiving_Application.Module_Receiving.Settings;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Wizard_Display_ReviewAndConfirmation : ViewModel_Shared_Base
    {
        private readonly IService_Receiving_Infrastructure_Workflow _workflowService;
        private readonly IService_Receiving_Infrastructure_Validation _validationService;
        private readonly IService_Help _helpService;
        private readonly IService_Window _windowService;
        private readonly IService_Receiving_Infrastructure_Settings _receivingSettings;

        [ObservableProperty]
        private ObservableCollection<Model_Receiving_Entity_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private bool _isSingleView = true;  // Default to single entry view

        [ObservableProperty]
        private int _currentEntryIndex = 0;

        [ObservableProperty]
        private Model_Receiving_Entity_ReceivingLoad? _currentEntry;

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _reviewEntryLabelText = "Entry";

        [ObservableProperty]
        private string _reviewOfLabelText = "of";

        [ObservableProperty]
        private string _reviewLoadNumberText = "Load Number";

        [ObservableProperty]
        private string _reviewPurchaseOrderNumberText = "Purchase Order Number";

        [ObservableProperty]
        private string _reviewPartIdText = "Part ID";

        [ObservableProperty]
        private string _reviewRemainingQuantityText = "Remaining Quantity";

        [ObservableProperty]
        private string _reviewWeightQuantityText = "Weight/Quantity";

        [ObservableProperty]
        private string _reviewHeatLotNumberText = "Heat/Lot Number";

        [ObservableProperty]
        private string _reviewPackagesPerLoadText = "Packages Per Load";

        [ObservableProperty]
        private string _reviewPackageTypeText = "Package Type";

        [ObservableProperty]
        private string _reviewPreviousLabelText = "Previous";

        [ObservableProperty]
        private string _reviewNextLabelText = "Next";

        [ObservableProperty]
        private string _reviewTableViewLabelText = "Table View";

        [ObservableProperty]
        private string _reviewSingleViewLabelText = "Single View";

        [ObservableProperty]
        private string _reviewAddAnotherText = "Add Another Part/PO";

        [ObservableProperty]
        private string _reviewSaveToDatabaseText = "Save to Database";

        [ObservableProperty]
        private string _reviewColumnLoadNumberText = "Load #";

        [ObservableProperty]
        private string _reviewColumnPoNumberText = "PO Number";

        [ObservableProperty]
        private string _reviewColumnPartIdText = "Part ID";

        [ObservableProperty]
        private string _reviewColumnRemainingQtyText = "Remaining Qty";

        [ObservableProperty]
        private string _reviewColumnWeightQtyText = "Weight/Qty";

        [ObservableProperty]
        private string _reviewColumnHeatLotText = "Heat/Lot #";

        [ObservableProperty]
        private string _reviewColumnPkgsText = "Pkgs";

        [ObservableProperty]
        private string _reviewColumnPkgTypeText = "Pkg Type";

        // Accessibility Properties
        [ObservableProperty]
        private string _reviewPreviousEntryAccessibilityName = "Previous Entry";

        [ObservableProperty]
        private string _reviewNextEntryAccessibilityName = "Next Entry";

        [ObservableProperty]
        private string _reviewSwitchToTableAccessibilityName = "Switch to Table View";

        [ObservableProperty]
        private string _reviewSwitchToSingleAccessibilityName = "Switch to Single View";

        [ObservableProperty]
        private string _reviewEntriesTableAccessibilityName = "Receiving Entries Table";

        [ObservableProperty]
        private string _reviewAddAnotherAccessibilityName = "Add Another Part or PO";

        [ObservableProperty]
        private string _reviewSaveToDatabaseAccessibilityName = "Save to Database";

        /// <summary>
        /// Display index (1-based) for UI
        /// </summary>
        public int DisplayIndex => CurrentEntryIndex + 1;

        /// <summary>
        /// Indicates if the Previous button should be enabled
        /// </summary>
        public bool CanGoBack => CurrentEntryIndex > 0;

        /// <summary>
        /// Indicates if the Next button should be enabled
        /// </summary>
        public bool CanGoNext => Loads.Count > 0 && CurrentEntryIndex < Loads.Count - 1;

        /// <summary>
        /// Inverse of IsSingleView for binding table view visibility
        /// </summary>
        public bool IsTableView => !IsSingleView;

        public ViewModel_Receiving_Wizard_Display_ReviewAndConfirmation(
            IService_Receiving_Infrastructure_Workflow workflowService,
            IService_Receiving_Infrastructure_Validation validationService,
            IService_Help helpService,
            IService_Window windowService,
            IService_Receiving_Infrastructure_Settings receivingSettings,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService)
            : base(errorHandler, logger, notificationService)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;
            _windowService = windowService;
            _receivingSettings = receivingSettings;

            _workflowService.StepChanged += OnStepChanged;

            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                ReviewEntryLabelText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewEntryLabel);
                ReviewOfLabelText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewOfLabel);
                ReviewLoadNumberText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewLoadNumber);
                ReviewPurchaseOrderNumberText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPurchaseOrderNumber);
                ReviewPartIdText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPartId);
                ReviewRemainingQuantityText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewRemainingQuantity);
                ReviewWeightQuantityText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewWeightQuantity);
                ReviewHeatLotNumberText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewHeatLotNumber);
                ReviewPackagesPerLoadText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPackagesPerLoad);
                ReviewPackageTypeText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPackageType);
                ReviewPreviousLabelText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPreviousLabel);
                ReviewNextLabelText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewNextLabel);
                ReviewTableViewLabelText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewTableViewLabel);
                ReviewSingleViewLabelText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewSingleViewLabel);
                ReviewAddAnotherText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewAddAnother);
                ReviewSaveToDatabaseText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewSaveToDatabase);

                ReviewColumnLoadNumberText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnLoadNumber);
                ReviewColumnPoNumberText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPoNumber);
                ReviewColumnPartIdText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPartId);
                ReviewColumnRemainingQtyText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnRemainingQty);
                ReviewColumnWeightQtyText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnWeightQty);
                ReviewColumnHeatLotText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnHeatLot);
                ReviewColumnPkgsText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPkgs);
                ReviewColumnPkgTypeText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPkgType);

                ReviewPreviousEntryAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewPreviousEntry);
                ReviewNextEntryAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewNextEntry);
                ReviewSwitchToTableAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewSwitchToTable);
                ReviewSwitchToSingleAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewSwitchToSingle);
                ReviewEntriesTableAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewEntriesTable);
                ReviewAddAnotherAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewAddAnother);
                ReviewSaveToDatabaseAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewSaveToDatabase);

                _logger.LogInfo("Review UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading Review UI text from settings: {ex.Message}", ex);
            }
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Review)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public async Task OnNavigatedToAsync()
        {
            Loads.Clear();
            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                }

                // Set current entry to first load if available
                if (Loads.Count > 0)
                {
                    CurrentEntryIndex = 0;
                    CurrentEntry = Loads[0];
                }
            }

            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(DisplayIndex));
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void PreviousEntry()
        {
            if (CurrentEntryIndex > 0)
            {
                CurrentEntryIndex--;
                CurrentEntry = Loads[CurrentEntryIndex];
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(DisplayIndex));
            }
        }

        [RelayCommand]
        private void NextEntry()
        {
            if (CurrentEntryIndex < Loads.Count - 1)
            {
                CurrentEntryIndex++;
                CurrentEntry = Loads[CurrentEntryIndex];
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(DisplayIndex));
            }
        }

        [RelayCommand]
        private void SwitchToTableView()
        {
            IsSingleView = false;
            OnPropertyChanged(nameof(IsTableView));
        }

        [RelayCommand]
        private void SwitchToSingleView()
        {
            IsSingleView = true;
            OnPropertyChanged(nameof(IsTableView));
        }

        [RelayCommand]
        private async Task AddAnotherPartAsync()
        {
            _logger.LogInfo("User requested to add another part/PO");

            try
            {
                // Show confirmation dialog to prevent accidental data loss
                if (!await ConfirmAddAnotherAsync())
                {
                    _logger.LogInfo("User cancelled add another part/PO");
                    return;
                }

                // Save current session to CSV before clearing
                IsBusy = true;
                StatusMessage = "Saving to CSV...";
                var saveResult = await _workflowService.SaveToCSVOnlyAsync();

                if (!saveResult.Success)
                {
                    await _errorHandler.HandleErrorAsync(
                        $"Failed to save CSV backup: {string.Join(", ", saveResult.Errors)}",
                        Enum_ErrorSeverity.Warning,
                        null,
                        true);
                    IsBusy = false;
                    return;
                }
                IsBusy = false;

                // Clear transient workflow data FIRST (before navigation)
                ClearTransientWorkflowData();

                // Preserve current session loads
                await _workflowService.AddCurrentPartToSessionAsync();

                // Navigate to PO Entry to start new part/PO entry
                // Note: AddCurrentPartToSessionAsync already sets CurrentStep to POEntry
                // so this is redundant, but explicit for clarity
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);

                _logger.LogInfo("Navigated to PO Entry for new part, workflow data cleared");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                _logger.LogError($"Error in AddAnotherPartAsync: {ex.Message}", ex);
                await _errorHandler.HandleErrorAsync(
                    "Failed to prepare for new part entry",
                    Enum_ErrorSeverity.Medium,
                    ex,
                    true);
            }
        }

        /// <summary>
        /// Shows confirmation dialog before clearing data for new entry
        /// </summary>
        private async Task<bool> ConfirmAddAnotherAsync()
        {
            try
            {
                var xamlRoot = _windowService.GetXamlRoot();
                if (xamlRoot == null)
                {
                    _logger.LogWarning("XamlRoot is null, proceeding without confirmation");
                    return true;
                }

                var dialog = new ContentDialog
                {
                    Title = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherTitle),
                    Content = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherContent),
                    PrimaryButtonText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherContinue),
                    CloseButtonText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherCancel),
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot
                };

                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex);
                return true; // Proceed if dialog fails
            }
        }

        /// <summary>
        /// Clears transient workflow data from intermediate steps (not the session loads)
        /// This prevents data duplication when adding another part
        /// </summary>
        private void ClearTransientWorkflowData()
        {
            try
            {
                // Clear the current session properties that hold form data
                // but preserve the Loads collection (already saved loads)
                var session = _workflowService.CurrentSession;
                if (session != null)
                {
                    // Clear PO-related properties if they exist in session
                    session.PoNumber = null;
                    session.IsNonPO = false;
                }

                // Clear UI inputs in connected ViewModels
                _workflowService.ClearUIInputs();

                _logger.LogInfo("Transient workflow data and UI inputs cleared for new entry");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex);
            }
        }



        [RelayCommand]
        private async Task SaveAsync()
        {
            // Trigger the save workflow
            // This moves to "Saving" step
            await _workflowService.AdvanceToNextStepAsync();
        }

        public void HandleCascadingUpdate(Model_Receiving_Entity_ReceivingLoad changedLoad, string propertyName)
        {
            if (changedLoad == null)
            {
                return;
            }

            // Logic for cascading updates
            // If user changes a value in row X, should it update X+1, X+2...?
            // Spec says: "Review Grid with Cascading Updates"
            // Usually this means if I change Heat# on Load 1, it might ask to update others, 
            // or if I change PO# it updates all for that part?

            // T044 says: "cascading update logic for Part# and PO#"

            var index = Loads.IndexOf(changedLoad);
            if (index < 0)
            {
                return;
            }

            if (propertyName == nameof(Model_Receiving_Entity_ReceivingLoad.PoNumber))
            {
                // Update all subsequent loads? Or all loads for this part?
                // If we are reviewing the current session, and it's a single part (US1), 
                // changing PO probably implies changing it for all loads of this part.
                foreach (var load in Loads)
                {
                    if (load != changedLoad)
                    {
                        load.PoNumber = changedLoad.PoNumber;
                    }
                }
            }
            else if (propertyName == nameof(Model_Receiving_Entity_ReceivingLoad.PartID))
            {
                // If PartID changes, we might need to re-validate or update description?
                // For now, let's assume we update all loads to match if it's the same "group".
                foreach (var load in Loads)
                {
                    if (load != changedLoad)
                    {
                        load.PartID = changedLoad.PartID;
                    }
                }
            }
            // Add other cascading logic if needed (e.g. Heat Number cascading downwards)
        }

        /// <summary>
        /// Shows contextual help for review grid
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.Review");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

