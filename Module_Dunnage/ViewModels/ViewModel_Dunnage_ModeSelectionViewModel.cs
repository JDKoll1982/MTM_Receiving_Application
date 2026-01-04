using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Interfaces;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Mode Selection - allows user to choose between Guided Wizard, Manual Entry, or Edit Mode
/// </summary>
public partial class ViewModel_Dunnage_ModeSelection : ViewModel_Shared_Base
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Help _helpService;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPreferences _userPreferencesService;
    private readonly IService_Window _windowService;

    public ViewModel_Dunnage_ModeSelection(
        IService_DunnageWorkflow workflowService,
        IService_Help helpService,
        IService_UserSessionManager sessionManager,
        IService_UserPreferences userPreferencesService,
        IService_Window windowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _helpService = helpService;
        _sessionManager = sessionManager;
        _userPreferencesService = userPreferencesService;
        _windowService = windowService;

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
    private async Task SelectGuidedModeAsync()
    {
        _logger.LogInfo("User selected Guided Wizard mode");

        if (await ConfirmModeChangeAsync())
        {
            ClearWorkflowData();
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
        }
    }

    [RelayCommand]
    private async Task SelectManualModeAsync()
    {
        _logger.LogInfo("User selected Manual Entry mode");

        if (await ConfirmModeChangeAsync())
        {
            ClearWorkflowData();
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.ManualEntry);
        }
    }

    [RelayCommand]
    private async Task SelectEditModeAsync()
    {
        _logger.LogInfo("User selected Edit Mode");

        if (await ConfirmModeChangeAsync())
        {
            ClearWorkflowData();
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.EditMode);
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

        // Check if there's any type/part selection
        if (_workflowService.CurrentSession.SelectedTypeId > 0 ||
            _workflowService.CurrentSession.SelectedPart != null ||
            !string.IsNullOrEmpty(_workflowService.CurrentSession.PONumber) ||
            !string.IsNullOrEmpty(_workflowService.CurrentSession.Location))
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

            _logger.LogInfo("Dunnage workflow data and UI inputs cleared for mode change");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing dunnage workflow data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Clears UI input properties across all Dunnage workflow ViewModels
    /// </summary>
    private void ClearAllUIInputs()
    {
        try
        {
            // Clear TypeSelection ViewModel
            var typeSelectionVM = App.GetService<ViewModel_Dunnage_TypeSelection>();
            if (typeSelectionVM != null)
            {
                typeSelectionVM.SelectedType = null;
            }

            // Clear PartSelection ViewModel
            var partSelectionVM = App.GetService<ViewModel_Dunnage_PartSelection>();
            if (partSelectionVM != null)
            {
                partSelectionVM.SelectedPart = null;
            }

            // Clear DetailsEntry ViewModel
            var detailsEntryVM = App.GetService<ViewModel_Dunnage_DetailsEntry>();
            if (detailsEntryVM != null)
            {
                detailsEntryVM.PoNumber = string.Empty;
                detailsEntryVM.Location = string.Empty;
                detailsEntryVM.SpecInputs?.Clear();
            }

            // Clear QuantityEntry ViewModel
            var quantityEntryVM = App.GetService<ViewModel_Dunnage_QuantityEntry>();
            if (quantityEntryVM != null)
            {
                quantityEntryVM.Quantity = 1;
            }

            _logger.LogInfo("UI inputs cleared across all Dunnage ViewModels");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
        }
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


