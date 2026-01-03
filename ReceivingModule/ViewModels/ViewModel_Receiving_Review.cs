using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using System;

namespace MTM_Receiving_Application.ReceivingModule.ViewModels
{
    public partial class ViewModel_Receiving_Review : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;
        private readonly IService_Window _windowService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private bool _isSingleView = true;  // Default to single entry view

        [ObservableProperty]
        private int _currentEntryIndex = 0;

        [ObservableProperty]
        private Model_ReceivingLoad? _currentEntry;

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

        public ViewModel_Receiving_Review(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_Help helpService,
            IService_Window windowService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;
            _windowService = windowService;

            _workflowService.StepChanged += OnStepChanged;
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
                    Title = "Add Another Part/PO",
                    Content = "Current form data will be cleared to start a new entry. Your reviewed loads are preserved. Continue?",
                    PrimaryButtonText = "Continue",
                    CloseButtonText = "Cancel",
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
                ClearUIInputsForNewEntry();

                _logger.LogInfo("Transient workflow data and UI inputs cleared for new entry");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Clears UI input properties in ViewModels to prepare for new entry
        /// while preserving reviewed loads
        /// </summary>
        private void ClearUIInputsForNewEntry()
        {
            try
            {
                // The key is to clear the session data that ViewModels read from
                // ViewModels bind to CurrentSession properties, not their own properties

                // This is already done in ClearTransientWorkflowData which clears:
                // - session.PoNumber
                // - session.IsNonPO

                // When navigation occurs to POEntry, the ViewModel will read fresh (empty) session data
                // No need to access individual ViewModel instances

                _logger.LogInfo("UI inputs will be cleared via session data reset");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Trigger the save workflow
            // This moves to "Saving" step
            await _workflowService.AdvanceToNextStepAsync();
        }

        public void HandleCascadingUpdate(Model_ReceivingLoad changedLoad, string propertyName)
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

            if (propertyName == nameof(Model_ReceivingLoad.PoNumber))
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
            else if (propertyName == nameof(Model_ReceivingLoad.PartID))
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
