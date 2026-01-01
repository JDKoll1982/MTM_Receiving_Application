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

    public Dunnage_ModeSelectionViewModel(
        IService_DunnageWorkflow workflowService,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _helpService = helpService;
    }

    #region Observable Properties

    [ObservableProperty]
    private bool _isGuidedModeDefault;

    [ObservableProperty]
    private bool _isManualModeDefault;

    [ObservableProperty]
    private bool _isEditModeDefault;

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
    private void SetGuidedAsDefault(bool isChecked)
    {
        IsGuidedModeDefault = isChecked;
        if (isChecked)
        {
            IsManualModeDefault = false;
            IsEditModeDefault = false;
            // TODO: Persist preference when user preferences service supports dunnage mode
            _logger.LogInfo("Set Guided Mode as default");
        }
    }

    [RelayCommand]
    private void SetManualAsDefault(bool isChecked)
    {
        IsManualModeDefault = isChecked;
        if (isChecked)
        {
            IsGuidedModeDefault = false;
            IsEditModeDefault = false;
            // TODO: Persist preference when user preferences service supports dunnage mode
            _logger.LogInfo("Set Manual Mode as default");
        }
    }

    /// <summary>\n    /// Shows contextual help for mode selection\n    /// </summary>
    /// <param name="isChecked"></param>\n    [RelayCommand]\n    private async Task ShowHelpAsync()\n    {\n        await _helpService.ShowHelpAsync(\"Dunnage.ModeSelection\");\n    }

    [RelayCommand]
    private void SetEditAsDefault(bool isChecked)
    {
        IsEditModeDefault = isChecked;
        if (isChecked)
        {
            IsGuidedModeDefault = false;
            IsManualModeDefault = false;
            // TODO: Persist preference when user preferences service supports dunnage mode
            _logger.LogInfo("Set Edit Mode as default");
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

