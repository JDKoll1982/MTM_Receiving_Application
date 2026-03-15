using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    public class Service_DunnageWorkflow : IService_DunnageWorkflow
    {
        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_ViewModelRegistry _viewModelRegistry;

        public Enum_DunnageWorkflowStep CurrentStep { get; private set; }
        public Model_DunnageSession CurrentSession { get; private set; } = new();

        public event EventHandler? StepChanged;
        public event EventHandler<string>? StatusMessageRaised;

        public Service_DunnageWorkflow(
            IService_MySQL_Dunnage dunnageService,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_ErrorHandler errorHandler,
            IService_ViewModelRegistry viewModelRegistry)
        {
            _dunnageService = dunnageService;
            _sessionManager = sessionManager;
            _logger = logger;
            _errorHandler = errorHandler;
            _viewModelRegistry = viewModelRegistry;

            // Clear any stale session data when the user's session times out,
            // so a subsequent login cannot see a prior user's workflow loads.
            _sessionManager.SessionTimedOut += OnSessionTimedOut;
        }

        private void OnSessionTimedOut(object? sender, object e)
        {
            ClearSession();
        }

        public Task<bool> StartWorkflowAsync()
        {
            ClearSession();

            // Check if user has a default mode set
            var currentUser = _sessionManager.CurrentSession?.User;
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultDunnageMode))
            {
                // Skip mode selection and go directly to the default mode
                switch (currentUser.DefaultDunnageMode.ToLower())
                {
                    case "guided":
                        GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
                        StatusMessageRaised?.Invoke(this, "Starting Guided Wizard mode");
                        break;
                    case "manual":
                        GoToStep(Enum_DunnageWorkflowStep.ManualEntry);
                        StatusMessageRaised?.Invoke(this, "Starting Manual Entry mode");
                        break;
                    case "edit":
                        GoToStep(Enum_DunnageWorkflowStep.EditMode);
                        StatusMessageRaised?.Invoke(this, "Starting Edit mode");
                        break;
                    default:
                        // Invalid default, show mode selection
                        GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
                        StatusMessageRaised?.Invoke(this, "Workflow started");
                        break;
                }
            }
            else
            {
                // No default mode, show mode selection
                GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
                StatusMessageRaised?.Invoke(this, "Workflow started");
            }

            return Task.FromResult(true);
        }

        public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync()
        {
            try
            {
                switch (CurrentStep)
                {
                    case Enum_DunnageWorkflowStep.ModeSelection:
                        GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
                        break;

                    case Enum_DunnageWorkflowStep.TypeSelection:
                        if (CurrentSession.SelectedTypeId <= 0)
                        {
                            return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please select a dunnage type." });
                        }
                        GoToStep(Enum_DunnageWorkflowStep.PartSelection);
                        break;

                    case Enum_DunnageWorkflowStep.PartSelection:
                        if (CurrentSession.SelectedPart == null)
                        {
                            return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please select a part." });
                        }
                        GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
                        break;

                    case Enum_DunnageWorkflowStep.QuantityEntry:
                        if (CurrentSession.Quantity <= 0)
                        {
                            return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Quantity must be greater than zero." });
                        }
                        GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
                        break;

                    case Enum_DunnageWorkflowStep.DetailsEntry:
                        if (string.IsNullOrWhiteSpace(CurrentSession.PONumber))
                        {
                            return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please enter a PO Number before continuing." });
                        }
                        AddCurrentLoadToSession();
                        GoToStep(Enum_DunnageWorkflowStep.Review);
                        break;

                    case Enum_DunnageWorkflowStep.Review:
                        return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Already at Review step. Use Save to finish." });
                }

                return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = true, TargetStep = CurrentStep });
            }
            catch (Exception ex)
            {
                _errorHandler.HandleErrorAsync("Error advancing step", Enum_ErrorSeverity.Error, ex, true);
                return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        public void GoToStep(Enum_DunnageWorkflowStep step)
        {
            // If navigating to TypeSelection from a mid-workflow step (not normal back-navigation)
            // clear accumulated session loads so stale data from a previous run is not shown at Review.
            if (step == Enum_DunnageWorkflowStep.TypeSelection
                && CurrentStep != Enum_DunnageWorkflowStep.ModeSelection
                && CurrentStep != Enum_DunnageWorkflowStep.PartSelection
                && CurrentStep != Enum_DunnageWorkflowStep.QuantityEntry
                && CurrentStep != Enum_DunnageWorkflowStep.DetailsEntry)
            {
                ClearSession();
            }

            CurrentStep = step;
            StepChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
        {
            try
            {
                var loads = new System.Collections.Generic.List<Model_DunnageLoad>(CurrentSession.Loads);

                if (loads.Count == 0)
                {
                    if (CurrentSession.SelectedPart != null && CurrentSession.Quantity > 0)
                    {
                        var load = CreateLoadFromCurrentSession();
                        loads.Add(load);
                    }
                    else
                    {
                        return new Model_SaveResult { IsSuccess = false, ErrorMessage = "No data to save." };
                    }
                }

                var dbResult = await _dunnageService.SaveLoadsAsync(loads);

                return new Model_SaveResult
                {
                    IsSuccess = dbResult.IsSuccess,
                    ErrorMessage = dbResult.ErrorMessage,
                    RecordsSaved = dbResult.IsSuccess ? loads.Count : 0
                };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync("Error saving to database", Enum_ErrorSeverity.Error, ex, true);
                return new Model_SaveResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<Model_SaveResult> SaveSessionAsync()
        {
            return await SaveToDatabaseOnlyAsync();
        }

        public void ClearSession()
        {
            CurrentSession = new Model_DunnageSession();
            _viewModelRegistry.ClearAllInputs();
            StatusMessageRaised?.Invoke(this, "Session cleared");
        }

        public async Task<Model_Dao_Result<int>> ClearLabelDataAsync()
        {
            try
            {
                var result = await _dunnageService.ClearLabelDataAsync();
                if (result.IsSuccess)
                {
                    StatusMessageRaised?.Invoke(this, $"Label data cleared — {result.Data} row(s) archived");
                }
                else
                {
                    StatusMessageRaised?.Invoke(this, $"Clear Label Data failed: {result.ErrorMessage}");
                }
                return result;
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync("Error clearing label data", Enum_ErrorSeverity.Error, ex, true);
                return Model_Dao_Result_Factory.Failure<int>($"Error clearing label data: {ex.Message}");
            }
        }

        public void AddCurrentLoadToSession()
        {
            try
            {
                if (CurrentSession.SelectedPart != null && CurrentSession.Quantity > 0)
                {
                    // Default location to part's home location if not specified
                    var location = string.IsNullOrWhiteSpace(CurrentSession.Location)
                        ? CurrentSession.SelectedPart.HomeLocation
                        : CurrentSession.Location;

                    // Default PO number if not specified
                    var poNumber = string.IsNullOrWhiteSpace(CurrentSession.PONumber)
                        ? "Nothing Entered"
                        : CurrentSession.PONumber;

                    var load = new Model_DunnageLoad
                    {
                        LoadUuid = Guid.NewGuid(),
                        PartId = CurrentSession.SelectedPart.PartId,
                        Quantity = CurrentSession.Quantity,
                        PoNumber = poNumber,
                        Location = location,
                        TypeName = CurrentSession.SelectedTypeName,
                        TypeIcon = CurrentSession.SelectedType?.Icon ?? "Help",
                        DunnageType = CurrentSession.SelectedTypeName,
                        TypeId = CurrentSession.SelectedTypeId,
                        Specs = CurrentSession.SpecValues ?? new Dictionary<string, object>(),
                        ReceivedDate = DateTime.Now,
                        CreatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown"
                    };

                    CurrentSession.Loads.Add(load);
                    _logger.LogInfo($"Added load to session: Part {load.PartId}, Qty {load.Quantity}", "DunnageWorkflow");
                    StatusMessageRaised?.Invoke(this, $"Added load to session");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleErrorAsync("Error adding load to session", Enum_ErrorSeverity.Medium, ex, false);
            }
        }

        private Model_DunnageLoad CreateLoadFromCurrentSession()
        {
            // Default location to part's home location if not specified
            var location = string.IsNullOrWhiteSpace(CurrentSession.Location)
                ? CurrentSession.SelectedPart?.HomeLocation
                : CurrentSession.Location;

            // Default PO number if not specified
            var poNumber = string.IsNullOrWhiteSpace(CurrentSession.PONumber)
                ? "Nothing Entered"
                : CurrentSession.PONumber;

            return new Model_DunnageLoad
            {
                LoadUuid = Guid.NewGuid(),
                PartId = CurrentSession.SelectedPart?.PartId ?? "Unknown",
                Quantity = CurrentSession.Quantity,
                PoNumber = poNumber,
                Location = location,
                DunnageType = CurrentSession.SelectedTypeName,
                TypeName = CurrentSession.SelectedTypeName,
                TypeIcon = CurrentSession.SelectedType?.Icon ?? "Help",
                TypeId = CurrentSession.SelectedTypeId,
                Specs = CurrentSession.SpecValues ?? new Dictionary<string, object>(),
                ReceivedDate = DateTime.Now,
                CreatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown"
            };
        }
    }
}



