using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Handlers.Commands;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Orchestration;

/// <summary>
/// Orchestrates the 3-step Wizard Mode workflow for guided receiving operations.
/// Manages step navigation, validation, and session state persistence.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Orchestration_MainWorkflow : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    /// <summary>
    /// Current step in the wizard workflow.
    /// </summary>
    [ObservableProperty]
    private Enum_Receiving_State_WorkflowStep _currentStep = Enum_Receiving_State_WorkflowStep.OrderAndPartSelection;

    /// <summary>
    /// Indicates if Step 1 (Order & Part) validation passed.
    /// </summary>
    [ObservableProperty]
    private bool _step1Valid = false;

    /// <summary>
    /// Indicates if Step 2 (Load Details) validation passed.
    /// </summary>
    [ObservableProperty]
    private bool _step2Valid = false;

    /// <summary>
    /// Indicates if all steps are valid and workflow can be saved.
    /// </summary>
    [ObservableProperty]
    private bool _allStepsValid = false;

    /// <summary>
    /// Unique session identifier for this workflow instance.
    /// </summary>
    [ObservableProperty]
    private string _sessionId = Guid.NewGuid().ToString();

    /// <summary>
    /// PO Number entered in Step 1.
    /// </summary>
    [ObservableProperty]
    private string _poNumber = string.Empty;

    /// <summary>
    /// Part Number selected in Step 1.
    /// </summary>
    [ObservableProperty]
    private string _partNumber = string.Empty;

    /// <summary>
    /// Number of loads to receive (entered in Step 1).
    /// </summary>
    [ObservableProperty]
    private int _loadCount = 0;

    /// <summary>
    /// Collection of load grid rows for Step 2 data entry.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loads = new();

    /// <summary>
    /// Current step number for display (1, 2, or 3).
    /// </summary>
    [ObservableProperty]
    private int _currentStepNumber = 1;

    /// <summary>
    /// Total number of steps in wizard.
    /// </summary>
    [ObservableProperty]
    private int _totalSteps = 3;

    /// <summary>
    /// Progress percentage (0-100) based on completed steps.
    /// </summary>
    [ObservableProperty]
    private int _progressPercentage = 0;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the wizard orchestration ViewModel.
    /// </summary>
    public ViewModel_Receiving_Wizard_Orchestration_MainWorkflow(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo($"Wizard Orchestration ViewModel initialized. SessionId: {SessionId}");
        
        UpdateNavigationState();
        UpdateProgressIndicator();
    }

    #endregion

    #region Navigation Commands

    /// <summary>
    /// Advances to the next step in the wizard workflow.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoToNextStepAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Validating step...";
            
            // Validate current step before proceeding
            await ValidateCurrentStepAsync();
            
            // Check if current step is valid
            if (CurrentStep == Enum_Receiving_State_WorkflowStep.OrderAndPartSelection && !Step1Valid)
            {
                await _errorHandler.ShowUserErrorAsync(
                    "Please complete all required fields in Step 1 before proceeding.",
                    "Validation Error",
                    nameof(GoToNextStepAsync));
                return;
            }
            
            if (CurrentStep == Enum_Receiving_State_WorkflowStep.LoadDetailsEntry && !Step2Valid)
            {
                await _errorHandler.ShowUserErrorAsync(
                    "Please complete all load details in Step 2 before proceeding.",
                    "Validation Error",
                    nameof(GoToNextStepAsync));
                return;
            }
            
            // Save session state before moving to next step
            await SaveSessionStateAsync();
            
            // Move to next step
            CurrentStep = CurrentStep switch
            {
                Enum_Receiving_State_WorkflowStep.OrderAndPartSelection => Enum_Receiving_State_WorkflowStep.LoadDetailsEntry,
                Enum_Receiving_State_WorkflowStep.LoadDetailsEntry => Enum_Receiving_State_WorkflowStep.ReviewAndSave,
                _ => CurrentStep
            };
            
            _logger.LogInfo($"Advanced to step: {CurrentStep}");
            UpdateNavigationState();
            UpdateProgressIndicator();
            StatusMessage = $"Moved to Step {CurrentStepNumber}";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(GoToNextStepAsync),
                nameof(ViewModel_Receiving_Wizard_Orchestration_MainWorkflow));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Returns to the previous step in the wizard workflow.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task GoToPreviousStepAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            
            // Save current state before going back
            await SaveSessionStateAsync();
            
            // Move to previous step
            CurrentStep = CurrentStep switch
            {
                Enum_Receiving_State_WorkflowStep.LoadDetailsEntry => Enum_Receiving_State_WorkflowStep.OrderAndPartSelection,
                Enum_Receiving_State_WorkflowStep.ReviewAndSave => Enum_Receiving_State_WorkflowStep.LoadDetailsEntry,
                _ => CurrentStep
            };
            
            _logger.LogInfo($"Returned to step: {CurrentStep}");
            UpdateNavigationState();
            UpdateProgressIndicator();
            StatusMessage = $"Returned to Step {CurrentStepNumber}";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(GoToPreviousStepAsync),
                nameof(ViewModel_Receiving_Wizard_Orchestration_MainWorkflow));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Determines if "Next" navigation is allowed.
    /// </summary>
    private bool CanGoNext()
    {
        return CurrentStep switch
        {
            Enum_Receiving_State_WorkflowStep.OrderAndPartSelection => Step1Valid && !IsBusy,
            Enum_Receiving_State_WorkflowStep.LoadDetailsEntry => Step2Valid && !IsBusy,
            _ => false
        };
    }

    /// <summary>
    /// Determines if "Previous" navigation is allowed.
    /// </summary>
    private bool CanGoPrevious()
    {
        return CurrentStep != Enum_Receiving_State_WorkflowStep.OrderAndPartSelection && !IsBusy;
    }

    #endregion

    #region Session Management Commands

    /// <summary>
    /// Saves the current workflow session state to the database.
    /// </summary>
    [RelayCommand]
    private async Task SaveSessionStateAsync()
    {
        try
        {
            var command = new CommandRequest_Receiving_Shared_Save_WorkflowSession
            {
                SessionId = Guid.TryParse(SessionId, out var sessionGuid) ? sessionGuid : Guid.NewGuid(),
                CurrentStep = (int)CurrentStep,
                PONumber = PoNumber,
                PartId = PartNumber,
                LoadCount = LoadCount,
                LoadDetailsJson = JsonSerializer.Serialize(Loads.ToList()),
                ModifiedBy = Environment.UserName
            };

            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning($"Failed to save session state: {result.ErrorMessage}");
            }
            else
            {
                _logger.LogInfo("Session state saved successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving session state: {ex.Message}");
            // Don't show error to user - session save is background operation
        }
    }

    /// <summary>
    /// Loads a previously saved workflow session.
    /// </summary>
    /// <param name="sessionId">The session ID to load.</param>
    [RelayCommand]
    private async Task LoadSessionStateAsync(string sessionId)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Loading session...";
            
            var query = new QueryRequest_Receiving_Shared_Get_WorkflowSession 
            { 
                SessionId = Guid.TryParse(sessionId, out var sessionGuid) ? sessionGuid : (Guid?)null 
            };
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                SessionId = result.Data.SessionId.ToString();
                CurrentStep = result.Data.CurrentStep;
                PoNumber = result.Data.PONumber ?? string.Empty;
                PartNumber = result.Data.PartId ?? string.Empty;
                LoadCount = result.Data.LoadCount;
                
                // Deserialize LoadDetailsJson
                Loads.Clear();
                if (!string.IsNullOrWhiteSpace(result.Data.LoadDetailsJson))
                {
                    try
                    {
                        var loadDetails = JsonSerializer.Deserialize<List<Model_Receiving_DataTransferObjects_LoadGridRow>>(result.Data.LoadDetailsJson);
                        if (loadDetails != null)
                        {
                            foreach (var load in loadDetails)
                            {
                                Loads.Add(load);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Failed to deserialize LoadDetailsJson: {ex.Message}");
                    }
                }

                UpdateNavigationState();
                UpdateProgressIndicator();
                _logger.LogInfo($"Session loaded: {SessionId}");
                StatusMessage = "Session loaded successfully";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Session not found",
                    "Load Session Error",
                    nameof(LoadSessionStateAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSessionStateAsync),
                nameof(ViewModel_Receiving_Wizard_Orchestration_MainWorkflow));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validates the current step and updates step validation flags.
    /// </summary>
    [RelayCommand]
    private async Task ValidateCurrentStepAsync()
    {
        try
        {
            switch (CurrentStep)
            {
                case Enum_Receiving_State_WorkflowStep.OrderAndPartSelection:
                    Step1Valid = !string.IsNullOrWhiteSpace(PoNumber) &&
                                 !string.IsNullOrWhiteSpace(PartNumber) &&
                                 LoadCount > 0 && LoadCount <= 99;
                    break;

                case Enum_Receiving_State_WorkflowStep.LoadDetailsEntry:
                    Step2Valid = Loads.All(load =>
                        load.Weight.HasValue && load.Weight > 0 &&
                        load.Quantity.HasValue && load.Quantity > 0 &&
                        !string.IsNullOrWhiteSpace(load.PackageType) &&
                        load.PackagesPerLoad.HasValue && load.PackagesPerLoad > 0);
                    break;

                case Enum_Receiving_State_WorkflowStep.ReviewAndSave:
                    AllStepsValid = Step1Valid && Step2Valid;
                    break;
            }

            UpdateNavigationState();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating step: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancels the current workflow and returns to hub.
    /// </summary>
    [RelayCommand]
    private void CancelWorkflow()
    {
        var confirmCancel = true; // Will be confirmed via dialog in View
        
        if (confirmCancel)
        {
            _logger.LogInfo("Workflow cancelled by user");
            StatusMessage = "Workflow cancelled";
            
            // Clear workflow state
            PoNumber = string.Empty;
            PartNumber = string.Empty;
            LoadCount = 0;
            Loads.Clear();
            CurrentStep = Enum_Receiving_State_WorkflowStep.OrderAndPartSelection;
            
            UpdateNavigationState();
            UpdateProgressIndicator();
            
            // Navigation to hub will be handled by parent View
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Updates navigation button states based on current step and validation.
    /// </summary>
    private void UpdateNavigationState()
    {
        CurrentStepNumber = CurrentStep switch
        {
            Enum_Receiving_State_WorkflowStep.OrderAndPartSelection => 1,
            Enum_Receiving_State_WorkflowStep.LoadDetailsEntry => 2,
            Enum_Receiving_State_WorkflowStep.ReviewAndSave => 3,
            _ => 1
        };

        GoToNextStepCommand.NotifyCanExecuteChanged();
        GoToPreviousStepCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Updates the progress indicator percentage.
    /// </summary>
    private void UpdateProgressIndicator()
    {
        ProgressPercentage = CurrentStep switch
        {
            Enum_Receiving_State_WorkflowStep.OrderAndPartSelection => 33,
            Enum_Receiving_State_WorkflowStep.LoadDetailsEntry => 66,
            Enum_Receiving_State_WorkflowStep.ReviewAndSave => 100,
            _ => 0
        };
    }

    #endregion
}

