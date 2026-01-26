using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Hub;

/// <summary>
/// Orchestrates the main workflow hub for the Receiving module.
/// Manages mode selection and navigation to Wizard, Manual, or Edit workflows.
/// </summary>
public partial class ViewModel_Receiving_Hub_Orchestration_MainWorkflow : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    /// <summary>
    /// Currently selected workflow mode (Wizard, Manual, Edit).
    /// </summary>
    [ObservableProperty]
    private Enum_Receiving_Mode_WorkflowMode _selectedMode = Enum_Receiving_Mode_WorkflowMode.Wizard;

    /// <summary>
    /// Indicates whether Non-PO receiving mode is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isNonPO = false;

    /// <summary>
    /// Name of the currently displayed view/page.
    /// </summary>
    [ObservableProperty]
    private string _currentView = string.Empty;

    /// <summary>
    /// Indicates if any workflow is currently active.
    /// </summary>
    [ObservableProperty]
    private bool _isWorkflowActive = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the hub orchestration ViewModel.
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="errorHandler"></param>
    /// <param name="logger"></param>
    /// <param name="notificationService"></param>
    public ViewModel_Receiving_Hub_Orchestration_MainWorkflow(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Hub Orchestration ViewModel initialized");
    }

    #endregion

    #region Commands

    /// <summary>
    /// Selects Wizard Mode and navigates to the wizard workflow.
    /// </summary>
    [RelayCommand]
    private async Task SelectWizardModeAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Starting Wizard Mode...";
            _logger.LogInfo("User selected Wizard Mode");

            SelectedMode = Enum_Receiving_Mode_WorkflowMode.Wizard;
            CurrentView = "WizardWorkflow";
            IsWorkflowActive = true;

            StatusMessage = "Wizard Mode activated";
            
            // Navigation will be handled by the View binding to CurrentView
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                ex.Message,
                Enum_ErrorSeverity.Medium,
                ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Selects Manual Entry Mode and navigates to manual workflow.
    /// </summary>
    [RelayCommand]
    private async Task SelectManualModeAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Starting Manual Entry Mode...";
            _logger.LogInfo("User selected Manual Entry Mode");

            SelectedMode = Enum_Receiving_Mode_WorkflowMode.Manual;
            CurrentView = "ManualWorkflow";
            IsWorkflowActive = true;

            StatusMessage = "Manual Entry Mode activated";
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                ex.Message,
                Enum_ErrorSeverity.Medium,
                ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Selects Edit Mode and navigates to transaction history.
    /// </summary>
    [RelayCommand]
    private async Task SelectEditModeAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Starting Edit Mode...";
            _logger.LogInfo("User selected Edit Mode");

            SelectedMode = Enum_Receiving_Mode_WorkflowMode.Edit;
            CurrentView = "EditWorkflow";
            IsWorkflowActive = true;

            StatusMessage = "Edit Mode activated";
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                ex.Message,
                Enum_ErrorSeverity.Medium,
                ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Toggles Non-PO receiving mode on/off.
    /// </summary>
    [RelayCommand]
    private void ToggleNonPOMode()
    {
        IsNonPO = !IsNonPO;
        _logger.LogInfo($"Non-PO mode toggled: {IsNonPO}");
        StatusMessage = IsNonPO ? "Non-PO mode enabled" : "Non-PO mode disabled";
    }

    /// <summary>
    /// Returns to the hub home screen (mode selection).
    /// </summary>
    [RelayCommand]
    private void ReturnToHub()
    {
        CurrentView = "ModeSelection";
        IsWorkflowActive = false;
        StatusMessage = "Returned to mode selection";
        _logger.LogInfo("User returned to hub");
    }

    #endregion
}
