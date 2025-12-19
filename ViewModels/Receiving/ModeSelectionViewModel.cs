using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class ModeSelectionViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_UserSessionManager _sessionManager;

        [ObservableProperty]
        private bool _isGuidedModeDefault;

        [ObservableProperty]
        private bool _isManualModeDefault;

        public ModeSelectionViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_UserSessionManager sessionManager,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _sessionManager = sessionManager;
            
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
        private async Task SetGuidedAsDefaultAsync(bool isChecked)
        {
            try
            {
                var currentUser = _sessionManager.CurrentSession?.User;
                if (currentUser == null) return;

                string? newMode = isChecked ? "guided" : null;
                
                // Update database
                var connectionString = Helper_Database_Variables.GetConnectionString();
                var dao = new Dao_User(connectionString);
                var result = await dao.UpdateDefaultModeAsync(currentUser.EmployeeNumber, newMode);
                
                if (result.IsSuccess)
                {
                    // Update in-memory user object
                    currentUser.DefaultReceivingMode = newMode;
                    
                    // Update UI state
                    IsGuidedModeDefault = isChecked;
                    if (isChecked)
                    {
                        IsManualModeDefault = false;
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
                
                // Update database
                var connectionString = Helper_Database_Variables.GetConnectionString();
                var dao = new Dao_User(connectionString);
                var result = await dao.UpdateDefaultModeAsync(currentUser.EmployeeNumber, newMode);
                
                if (result.IsSuccess)
                {
                    // Update in-memory user object
                    currentUser.DefaultReceivingMode = newMode;
                    
                    // Update UI state
                    IsManualModeDefault = isChecked;
                    if (isChecked)
                    {
                        IsGuidedModeDefault = false;
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
    }
}
