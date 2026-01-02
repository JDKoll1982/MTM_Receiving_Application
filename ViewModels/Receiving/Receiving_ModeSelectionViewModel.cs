using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Threading.Tasks;
namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class Receiving_ReceivingModeSelectionViewModel : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_UserPreferences _userPreferencesService;
        private readonly IService_Help _helpService;

        [ObservableProperty]
        private bool _isGuidedModeDefault;

        [ObservableProperty]
        private bool _isManualModeDefault;

        [ObservableProperty]
        private bool _isEditModeDefault;

        public Receiving_ReceivingModeSelectionViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_UserSessionManager sessionManager,
            IService_UserPreferences userPreferencesService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _sessionManager = sessionManager;
            _userPreferencesService = userPreferencesService;
            _helpService = helpService;

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
        private void SelectGuidedMode()
        {
            _logger.LogInfo("User selected Guided Mode.");
            _workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        }

        [RelayCommand]
        private void SelectManualMode()
        {
            _logger.LogInfo("User selected Manual Mode.");
            _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
        }

        [RelayCommand]
        private void SelectEditMode()
        {
            _logger.LogInfo("User selected Edit Mode.");
            _workflowService.GoToStep(Enum_ReceivingWorkflowStep.EditMode);
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
