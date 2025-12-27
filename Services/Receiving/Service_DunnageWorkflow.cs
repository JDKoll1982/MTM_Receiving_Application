using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Services.Receiving
{
    public class Service_DunnageWorkflow : IService_DunnageWorkflow
    {
        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_DunnageCSVWriter _csvWriter;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly ILoggingService _logger;
        private readonly IService_ErrorHandler _errorHandler;

        public Enum_DunnageWorkflowStep CurrentStep { get; private set; }
        public Model_DunnageSession CurrentSession { get; private set; } = new();

        public event EventHandler? StepChanged;
        public event EventHandler<string>? StatusMessageRaised;

        public Service_DunnageWorkflow(
            IService_MySQL_Dunnage dunnageService,
            IService_DunnageCSVWriter csvWriter,
            IService_UserSessionManager sessionManager,
            ILoggingService logger,
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
            GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
            StatusMessageRaised?.Invoke(this, "Workflow started");
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
    }
}
