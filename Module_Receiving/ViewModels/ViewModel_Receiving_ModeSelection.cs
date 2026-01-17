using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System;
using System.Threading.Tasks;
namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_ModeSelection : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_UserPreferences _userPreferencesService;
        private readonly IService_Help _helpService;
        private readonly IService_Window _windowService;

        [ObservableProperty]
        private bool _isGuidedModeDefault;

        [ObservableProperty]
        private bool _isManualModeDefault;

        [ObservableProperty]
        private bool _isEditModeDefault;

        public ViewModel_Receiving_ModeSelection(
            IService_ReceivingWorkflow workflowService,
            IService_UserSessionManager sessionManager,
            IService_UserPreferences userPreferencesService,
            IService_Help helpService,
            IService_Window windowService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _sessionManager = sessionManager;
            _userPreferencesService = userPreferencesService;
            _helpService = helpService;
            _windowService = windowService;

            // Load current default mode
            LoadDefaultMode();
        }

        private void LoadDefaultMode()
        {
            var currentUser = _sessionManager.CurrentSession?.User;
            if (currentUser != null)
            {
                IsGuidedModeDefault = currentUser.DefaultReceivingMode == "guided";
                IsManualModeDefault = currentUser.DefaultReceivingMode == "manual";
                IsEditModeDefault = currentUser.DefaultReceivingMode == "edit";
            }
        }

        [RelayCommand]
        private async Task SelectGuidedModeAsync()
        {
            _logger.LogInfo("User selected Guided Mode.");

            if (await ConfirmModeChangeAsync())
            {
                ClearWorkflowData();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
            }
        }

        [RelayCommand]
        private async Task SelectManualModeAsync()
        {
            _logger.LogInfo("User selected Manual Mode.");

            if (await ConfirmModeChangeAsync())
            {
                ClearWorkflowData();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
            }
        }

        [RelayCommand]
        private async Task SelectEditModeAsync()
        {
            _logger.LogInfo("User selected Edit Mode.");

            if (await ConfirmModeChangeAsync())
            {
                ClearWorkflowData();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.EditMode);
            }
        }

        /// <summary>
        /// Shows confirmation dialog before mode change
        /// </summary>
        /// <returns>True if user confirmed, false if cancelled</returns>
        /// <summary>
        /// Checks if there is any unsaved data in the current workflow
        /// </summary>
        private bool HasUnsavedData()
        {
            if (_workflowService.CurrentSession == null)
            {
                return false;
            }

            // Check if there are any loads in the session
            if (_workflowService.CurrentSession.Loads?.Count > 0)
            {
                return true;
            }

            // Check if there's any PO/Part selection
            if (!string.IsNullOrEmpty(_workflowService.CurrentSession.PoNumber) ||
                _workflowService.CurrentPart != null)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> ConfirmModeChangeAsync()
        {
            // Skip confirmation if there's no unsaved data
            if (!HasUnsavedData())
            {
                _logger.LogInfo("No unsaved data detected, skipping confirmation dialog");
                return true;
            }

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
                    Title = "Confirm Mode Selection",
                    Content = "Selecting a new mode will reset all unsaved data in the current workflow. Do you want to continue?",
                    PrimaryButtonText = "Continue",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
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
        /// Clears all transient workflow data without affecting persisted data
        /// </summary>
        private void ClearWorkflowData()
        {
            try
            {
                // Clear current session data (in-memory only, not database or CSV)
                if (_workflowService.CurrentSession != null)
                {
                    _workflowService.CurrentSession.Loads?.Clear();
                }

                // Clear UI inputs in all connected ViewModels
                ClearAllUIInputs();

                _logger.LogInfo("Workflow data and UI inputs cleared for mode change");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing workflow data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Clears UI input properties across all Receiving workflow ViewModels
        /// </summary>
        private void ClearAllUIInputs()
        {
            try
            {
                // Clear POEntry ViewModel
                var poEntryVM = App.GetService<ViewModel_Receiving_POEntry>();
                if (poEntryVM != null)
                {
                    poEntryVM.PoNumber = string.Empty;
                    poEntryVM.PartID = string.Empty;
                    poEntryVM.SelectedPart = null;
                    poEntryVM.IsNonPOItem = false;
                    poEntryVM.Parts?.Clear();
                }

                // Clear PackageType ViewModel
                var packageTypeVM = App.GetService<ViewModel_Receiving_PackageType>();
                if (packageTypeVM != null)
                {
                    packageTypeVM.SelectedPackageType = string.Empty;
                    packageTypeVM.CustomPackageTypeName = string.Empty;
                    packageTypeVM.IsCustomTypeVisible = false;
                }

                // Clear LoadEntry ViewModel
                var loadEntryVM = App.GetService<ViewModel_Receiving_LoadEntry>();
                if (loadEntryVM != null)
                {
                    loadEntryVM.NumberOfLoads = 1;
                }

                // Clear WeightQuantity ViewModel
                var weightQuantityVM = App.GetService<ViewModel_Receiving_WeightQuantity>();
                if (weightQuantityVM != null)
                {
                    weightQuantityVM.Loads?.Clear();
                }

                // Clear HeatLot ViewModel
                var heatLotVM = App.GetService<ViewModel_Receiving_HeatLot>();
                if (heatLotVM != null)
                {
                    heatLotVM.Loads?.Clear();
                }

                _logger.LogInfo("UI inputs cleared across all Receiving ViewModels");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private async Task SetGuidedAsDefaultAsync(bool isChecked)
        {
            try
            {
                var currentUser = _sessionManager.CurrentSession?.User;
                if (currentUser == null)
                {
                    return;
                }

                string? newMode = isChecked ? "guided" : null;

                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode ?? "");

                if (result.IsSuccess)
                {
                    // Update in-memory user object
                    currentUser.DefaultReceivingMode = newMode;

                    // Update UI state
                    IsGuidedModeDefault = isChecked;
                    if (isChecked)
                    {
                        IsManualModeDefault = false;
                        IsEditModeDefault = false;
                    }

                    _logger.LogInfo($"Default mode set to: {newMode ?? "none"}");
                    StatusMessage = isChecked ? "Guided mode set as default" : "Default mode cleared";
                }
                else
                {
                    await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
                    // Revert checkbox
                    IsGuidedModeDefault = !isChecked;
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Failed to set default mode: {ex.Message}",
                    Enum_ErrorSeverity.Error, ex, true);
                // Revert checkbox
                IsGuidedModeDefault = !isChecked;
            }
        }

        [RelayCommand]
        private async Task SetManualAsDefaultAsync(bool isChecked)
        {
            try
            {
                var currentUser = _sessionManager.CurrentSession?.User;
                if (currentUser == null)
                {
                    return;
                }

                string? newMode = isChecked ? "manual" : null;

                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode ?? "");

                if (result.IsSuccess)
                {
                    // Update in-memory user object
                    currentUser.DefaultReceivingMode = newMode;

                    // Update UI state
                    IsManualModeDefault = isChecked;
                    if (isChecked)
                    {
                        IsGuidedModeDefault = false;
                        IsEditModeDefault = false;
                    }

                    _logger.LogInfo($"Default mode set to: {newMode ?? "none"}");
                    StatusMessage = isChecked ? "Manual mode set as default" : "Default mode cleared";
                }
                else
                {
                    await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
                    // Revert checkbox
                    IsManualModeDefault = !isChecked;
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Failed to set default mode: {ex.Message}",
                    Enum_ErrorSeverity.Error, ex, true);
                // Revert checkbox
                IsManualModeDefault = !isChecked;
            }
        }

        [RelayCommand]
        private async Task SetEditAsDefaultAsync(bool isChecked)
        {
            try
            {
                var currentUser = _sessionManager.CurrentSession?.User;
                if (currentUser == null)
                {
                    return;
                }

                string? newMode = isChecked ? "edit" : null;

                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode ?? "");

                if (result.IsSuccess)
                {
                    // Update in-memory user object
                    currentUser.DefaultReceivingMode = newMode;

                    // Update UI state
                    IsEditModeDefault = isChecked;
                    if (isChecked)
                    {
                        IsGuidedModeDefault = false;
                        IsManualModeDefault = false;
                    }

                    _logger.LogInfo($"Default mode set to: {newMode ?? "none"}");
                    StatusMessage = isChecked ? "Edit mode set as default" : "Default mode cleared";
                }
                else
                {
                    await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
                    // Revert checkbox
                    IsEditModeDefault = !isChecked;
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Failed to set default mode: {ex.Message}",
                    Enum_ErrorSeverity.Error, ex, true);
                // Revert checkbox
                IsEditModeDefault = !isChecked;
            }
        }

        /// <summary>
        /// Shows contextual help for receiving mode selection
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.ModeSelection");
        }

        #region Help Content Helpers

        /// <summary>
        /// Gets a tooltip by key from the help service
        /// </summary>
        /// <param name="key"></param>
        public string GetTooltip(string key) => _helpService.GetTooltip(key);

        /// <summary>
        /// Gets a placeholder by key from the help service
        /// </summary>
        /// <param name="key"></param>
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

        /// <summary>
        /// Gets a tip by key from the help service
        /// </summary>
        /// <param name="key"></param>
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

