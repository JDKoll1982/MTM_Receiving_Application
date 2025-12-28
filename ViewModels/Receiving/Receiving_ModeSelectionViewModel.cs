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
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _sessionManager = sessionManager;
            _userPreferencesService = userPreferencesService;

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
            _workflowService.GoToStep(WorkflowStep.POEntry);
        }

        [RelayCommand]
        private void SelectManualMode()
        {
            _logger.LogInfo("User selected Manual Mode.");
            _workflowService.GoToStep(WorkflowStep.ManualEntry);
        }

        [RelayCommand]
        private void SelectEditMode()
        {
            _logger.LogInfo("User selected Edit Mode.");
            _workflowService.GoToStep(WorkflowStep.EditMode);
        }

        [RelayCommand]
        private async Task SetGuidedAsDefaultAsync(bool isChecked)
        {
            try
            {
                var currentUser = _sessionManager.CurrentSession?.User;
                if (currentUser == null) return;

                string? newMode = isChecked ? "guided" : null;

                // Update via service
                // Note: Service expects "Package" or "Pallet" for package type preference, 
                // but here we are setting receiving mode (guided/manual/edit).
                // The IService_UserPreferences interface seems to be designed for PackageTypePreference (Package/Pallet)
                // based on the spec, but here we are updating DefaultReceivingMode (guided/manual/edit).
                // However, Service_UserPreferences.UpdateDefaultModeAsync calls Dao_User.UpdateDefaultModeAsync
                // which updates the 'default_receiving_mode' column.
                // The validation in Service_UserPreferences checks for "Package" or "Pallet".
                // This is a conflict. The spec says "Package" or "Pallet" but the existing code uses "guided", "manual", "edit".

                // I need to fix Service_UserPreferences validation to allow these values OR update the spec.
                // Given I cannot change the spec easily, I should update the Service validation to be more permissive or correct.
                // I will update the Service validation in a separate step if needed.
                // For now, I will assume the service handles it or I will fix the service.

                // Wait, I implemented Service_UserPreferences with:
                // if (defaultMode != "Package" && defaultMode != "Pallet") return Failure...

                // This will fail for "guided", "manual", "edit".
                // I MUST fix Service_UserPreferences.cs to allow these values.

                var result = await _userPreferencesService.UpdateDefaultModeAsync(currentUser.WindowsUsername, newMode ?? "");

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
                if (currentUser == null) return;

                string? newMode = isChecked ? "manual" : null;

                var result = await _userPreferencesService.UpdateDefaultModeAsync(currentUser.WindowsUsername, newMode ?? "");

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
                if (currentUser == null) return;

                string? newMode = isChecked ? "edit" : null;

                var result = await _userPreferencesService.UpdateDefaultModeAsync(currentUser.WindowsUsername, newMode ?? "");

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
    }
}
