using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Mode Selection - allows user to choose between Guided Wizard, Manual Entry, or Edit Mode
/// </summary>
public partial class Dunnage_ModeSelectionViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Help _helpService;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPreferences _userPreferencesService;

    public Dunnage_ModeSelectionViewModel(
        IService_DunnageWorkflow workflowService,
        IService_Help helpService,
        IService_UserSessionManager sessionManager,
        IService_UserPreferences userPreferencesService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _helpService = helpService;
        _sessionManager = sessionManager;
        _userPreferencesService = userPreferencesService;

        // Load current default mode
        LoadDefaultMode();
    }

    #region Observable Properties

    [ObservableProperty]
    private bool _isGuidedModeDefault;

    [ObservableProperty]
    private bool _isManualModeDefault;

    [ObservableProperty]
    private bool _isEditModeDefault;

    #endregion

    #region Initialization

    /// <summary>
    /// Load the user's default dunnage mode preference
    /// </summary>
    private void LoadDefaultMode()
    {
        var currentUser = _sessionManager.CurrentSession?.User;
        if (currentUser != null)
        {
            IsGuidedModeDefault = currentUser.DefaultDunnageMode == "guided";
            IsManualModeDefault = currentUser.DefaultDunnageMode == "manual";
            IsEditModeDefault = currentUser.DefaultDunnageMode == "edit";
        }
    }

    #endregion

    #region Mode Selection Commands

    [RelayCommand]
    private void SelectGuidedMode()
    {
        _logger.LogInfo("User selected Guided Wizard mode");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
    }

    [RelayCommand]
    private void SelectManualMode()
    {
        _logger.LogInfo("User selected Manual Entry mode");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ManualEntry);
    }

    [RelayCommand]
    private void SelectEditMode()
    {
        _logger.LogInfo("User selected Edit Mode");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.EditMode);
    }

    #endregion

    #region Set Default Mode Commands

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

            var result = await _userPreferencesService.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, newMode ?? "");

            if (result.IsSuccess)
            {
                // Update in-memory user object
                currentUser.DefaultDunnageMode = newMode;

                // Update UI state
                IsGuidedModeDefault = isChecked;
                if (isChecked)
                {
                    IsManualModeDefault = false;
                    IsEditModeDefault = false;
                }

                _logger.LogInfo($"Dunnage default mode set to: {newMode ?? "none"}");
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
            await _errorHandler.HandleErrorAsync($"Failed to set default dunnage mode: {ex.Message}",
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

            var result = await _userPreferencesService.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, newMode ?? "");

            if (result.IsSuccess)
            {
                // Update in-memory user object
                currentUser.DefaultDunnageMode = newMode;

                // Update UI state
                IsManualModeDefault = isChecked;
                if (isChecked)
                {
                    IsGuidedModeDefault = false;
                    IsEditModeDefault = false;
                }

                _logger.LogInfo($"Dunnage default mode set to: {newMode ?? "none"}");
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
            await _errorHandler.HandleErrorAsync($"Failed to set default dunnage mode: {ex.Message}",
                Enum_ErrorSeverity.Error, ex, true);
            // Revert checkbox
            IsManualModeDefault = !isChecked;
        }
    }

    /// <summary>
    /// Shows contextual help for mode selection
    /// </summary>
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.ModeSelection");
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

            var result = await _userPreferencesService.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, newMode ?? "");

            if (result.IsSuccess)
            {
                // Update in-memory user object
                currentUser.DefaultDunnageMode = newMode;

                // Update UI state
                IsEditModeDefault = isChecked;
                if (isChecked)
                {
                    IsGuidedModeDefault = false;
                    IsManualModeDefault = false;
                }

                _logger.LogInfo($"Dunnage default mode set to: {newMode ?? "none"}");
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
            await _errorHandler.HandleErrorAsync($"Failed to set default dunnage mode: {ex.Message}",
                Enum_ErrorSeverity.Error, ex, true);
            // Revert checkbox
            IsEditModeDefault = !isChecked;
        }
    }

    #endregion

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

