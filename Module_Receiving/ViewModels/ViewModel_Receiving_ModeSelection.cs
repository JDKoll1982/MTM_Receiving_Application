using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Receiving.Settings;
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
        private readonly IService_ReceivingSettings _receivingSettings;

        [ObservableProperty]
        private bool _isGuidedModeDefault;

        [ObservableProperty]
        private bool _isManualModeDefault;

        [ObservableProperty]
        private bool _isEditModeDefault;

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _guidedTitleText = "Guided Wizard";

        [ObservableProperty]
        private string _guidedDescriptionText = "Step-by-step process for standard receiving workflow.";

        [ObservableProperty]
        private string _manualTitleText = "Manual Entry";

        [ObservableProperty]
        private string _manualDescriptionText = "Customizable grid for bulk data entry and editing.";

        [ObservableProperty]
        private string _editTitleText = "Edit Mode";

        [ObservableProperty]
        private string _editDescriptionText = "Edit existing loads without adding new ones.";

        [ObservableProperty]
        private string _setAsDefaultText = "Set as default mode";

        // Accessibility Properties
        [ObservableProperty]
        private string _guidedAccessibilityName = "Guided Wizard Mode";

        [ObservableProperty]
        private string _manualAccessibilityName = "Manual Entry Mode";

        [ObservableProperty]
        private string _editAccessibilityName = "Edit Mode";

        public ViewModel_Receiving_ModeSelection(
            IService_ReceivingWorkflow workflowService,
            IService_UserSessionManager sessionManager,
            IService_UserPreferences userPreferencesService,
            IService_Help helpService,
            IService_Window windowService,
            IService_ReceivingSettings receivingSettings,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService) : base(errorHandler, logger, notificationService)
        {
            _workflowService = workflowService;
            _sessionManager = sessionManager;
            _userPreferencesService = userPreferencesService;
            _helpService = helpService;
            _windowService = windowService;
            _receivingSettings = receivingSettings;

            // Load current default mode
            LoadDefaultMode();

            // Load UI text from settings
            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                // Load UI text
                GuidedTitleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionGuidedTitle);
                GuidedDescriptionText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionGuidedDescription);
                ManualTitleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionManualTitle);
                ManualDescriptionText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionManualDescription);
                EditTitleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionEditTitle);
                EditDescriptionText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionEditDescription);
                SetAsDefaultText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ModeSelectionSetDefault);

                // Load accessibility text
                GuidedAccessibilityName = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Accessibility.ModeSelectionGuidedButton);
                ManualAccessibilityName = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Accessibility.ModeSelectionManualButton);
                EditAccessibilityName = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Accessibility.ModeSelectionEditButton);

                _logger.LogInfo("Mode Selection UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading UI text from settings: {ex.Message}", ex);
                // Defaults are already set in the property declarations
            }
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
                    Title = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionTitle),
                    Content = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContent),
                    PrimaryButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContinue),
                    CloseButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionCancel),
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
            // Transient ViewModels cannot be cleared this way as GetService returns a new instance.
            // State should represent the current session which is cleared in ClearWorkflowData.
            _logger.LogInfo("UI inputs cleared via session reset.");
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

                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode);

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

                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode);

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

                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode);

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

