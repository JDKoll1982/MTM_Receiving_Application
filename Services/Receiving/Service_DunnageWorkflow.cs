using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Services.Receiving
{
    public class Service_DunnageWorkflow : IService_DunnageWorkflow
    {
        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_DunnageCSVWriter _csvWriter;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ErrorHandler _errorHandler;

        public Enum_DunnageWorkflowStep CurrentStep { get; private set; }
        public Model_DunnageSession CurrentSession { get; private set; } = new();

        public event EventHandler? StepChanged;
        public event EventHandler<string>? StatusMessageRaised;

        public Service_DunnageWorkflow(
            IService_MySQL_Dunnage dunnageService,
            IService_DunnageCSVWriter csvWriter,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_ErrorHandler errorHandler)
        {
            _dunnageService = dunnageService;
            _csvWriter = csvWriter;
            _sessionManager = sessionManager;
            _logger = logger;
            _errorHandler = errorHandler;
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
                        // Add current load to session before advancing to Review
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
            CurrentStep = step;
            StepChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Model_SaveResult> SaveSessionAsync()
        {
            try
            {
                var loads = new System.Collections.Generic.List<Model_DunnageLoad>(CurrentSession.Loads);

                // If no loads in list, try to create one from current session state
                if (loads.Count == 0)
                {
                    if (CurrentSession.SelectedPart != null && CurrentSession.Quantity > 0)
                    {
                        var load = new Model_DunnageLoad
                        {
                            LoadUuid = Guid.NewGuid(),
                            PartId = CurrentSession.SelectedPart.PartId,
                            Quantity = CurrentSession.Quantity,
                            PoNumber = CurrentSession.PONumber,
                            DunnageType = CurrentSession.SelectedTypeName,
                            TypeName = CurrentSession.SelectedTypeName,
                            TypeIcon = CurrentSession.SelectedType?.Icon ?? "Help",
                            Specs = CurrentSession.SelectedPart.SpecValuesDict,
                            ReceivedDate = DateTime.Now,
                            CreatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown"
                        };
                        loads.Add(load);
                        CurrentSession.Loads.Add(load);
                    }
                    else
                    {
                        return new Model_SaveResult { IsSuccess = false, ErrorMessage = "No data to save." };
                    }
                }

                // Save to DB
                var dbResult = await _dunnageService.SaveLoadsAsync(loads);
                if (!dbResult.IsSuccess)
                {
                    return new Model_SaveResult { IsSuccess = false, ErrorMessage = dbResult.ErrorMessage };
                }

                // Export to CSV
                var csvResult = await _csvWriter.WriteToCSVAsync(loads);

                return new Model_SaveResult
                {
                    IsSuccess = true,
                    RecordsSaved = loads.Count,
                    CSVExportResult = csvResult
                };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync("Error saving session", Enum_ErrorSeverity.Error, ex, true);
                return new Model_SaveResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public void ClearSession()
        {
            CurrentSession = new Model_DunnageSession();
            StatusMessageRaised?.Invoke(this, "Session cleared");
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
    }
}
