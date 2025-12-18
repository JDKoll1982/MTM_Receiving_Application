using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MTM_Receiving_Application.Services.Receiving
{
    /// <summary>
    /// Service for orchestrating the receiving workflow state machine.
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
    {
        private readonly IService_SessionManager _sessionManager;
        private readonly IService_CSVWriter _csvWriter;
        private readonly IService_MySQL_Receiving _mysqlReceiving;
        private readonly IService_ReceivingValidation _validation;
        private readonly ILoggingService _logger;
        private List<Model_ReceivingLoad> _currentBatchLoads = new();

        public event EventHandler? StepChanged;
        public event EventHandler<string>? StatusMessageRaised;

        public void RaiseStatusMessage(string message)
        {
            StatusMessageRaised?.Invoke(this, message);
        }

        private WorkflowStep _currentStep = WorkflowStep.POEntry;
        public WorkflowStep CurrentStep
        {
            get => _currentStep;
            private set
            {
                if (_currentStep != value)
                {
                    _logger.LogInfo($"Changing step from {_currentStep} to {value}");
                    _currentStep = value;
                    StepChanged?.Invoke(this, EventArgs.Empty);
                    _logger.LogInfo($"Step changed to {value} (event fired)");
                }
            }
        }
        public Model_ReceivingSession CurrentSession { get; private set; } = new();
        public string? CurrentPONumber { get; set; }
        public Model_InforVisualPart? CurrentPart { get; set; }
        public bool IsNonPOItem { get; set; }
        public int NumberOfLoads { get; set; } = 1;

        public Service_ReceivingWorkflow(
            IService_SessionManager sessionManager,
            IService_CSVWriter csvWriter,
            IService_MySQL_Receiving mysqlReceiving,
            IService_ReceivingValidation validation,
            ILoggingService logger)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            _mysqlReceiving = mysqlReceiving ?? throw new ArgumentNullException(nameof(mysqlReceiving));
            _validation = validation ?? throw new ArgumentNullException(nameof(validation));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> StartWorkflowAsync()
        {
            _logger.LogInfo("Starting receiving workflow.");
            // Try to load existing session
            var existingSession = await _sessionManager.LoadSessionAsync();

            if (existingSession != null && existingSession.HasLoads)
            {
                _logger.LogInfo("Restoring existing session.");
                CurrentSession = existingSession;
                // Determine which step to restore to based on session state
                CurrentStep = WorkflowStep.Review; // Default to review if session exists
                return true; // Session restored
            }

            // Start fresh
            CurrentSession = new Model_ReceivingSession();
            NumberOfLoads = 1;
            CurrentStep = WorkflowStep.POEntry;
            return false; // New session
        }

        public async Task<WorkflowStepResult> AdvanceToNextStepAsync()
        {
            // Validate current step before advancing
            var validationErrors = new List<string>();

            switch (CurrentStep)
            {
                case WorkflowStep.POEntry:
                    if (string.IsNullOrEmpty(CurrentPONumber) && !IsNonPOItem)
                    {
                         validationErrors.Add("PO Number is required.");
                         return WorkflowStepResult.ErrorResult(validationErrors);
                    }
                    if (CurrentPart == null)
                    {
                         validationErrors.Add("Part selection is required.");
                         return WorkflowStepResult.ErrorResult(validationErrors);
                    }
                    
                    // Update session with PO/Part info
                    CurrentSession.IsNonPO = IsNonPOItem;
                    CurrentSession.PoNumber = IsNonPOItem ? null : CurrentPONumber;
                    
                    CurrentStep = WorkflowStep.LoadEntry;
                    break;

                case WorkflowStep.LoadEntry:
                    if (NumberOfLoads < 1)
                    {
                        validationErrors.Add("Number of loads must be at least 1.");
                        return WorkflowStepResult.ErrorResult(validationErrors);
                    }
                    GenerateLoads();
                    CurrentStep = WorkflowStep.WeightQuantityEntry;
                    break;

                case WorkflowStep.WeightQuantityEntry:
                    foreach (var load in CurrentSession.Loads)
                    {
                        var result = _validation.ValidateWeightQuantity(load.WeightQuantity);
                        if (!result.IsValid)
                        {
                            validationErrors.Add($"Load {load.LoadNumber}: {result.Message}");
                        }
                    }
                    if (validationErrors.Count > 0)
                    {
                        return WorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = WorkflowStep.HeatLotEntry;
                    break;

                case WorkflowStep.HeatLotEntry:
                    foreach (var load in CurrentSession.Loads)
                    {
                        var result = _validation.ValidateHeatLotNumber(load.HeatLotNumber);
                        if (!result.IsValid)
                        {
                            validationErrors.Add($"Load {load.LoadNumber}: {result.Message}");
                        }
                    }
                    if (validationErrors.Count > 0)
                    {
                        return WorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = WorkflowStep.PackageTypeEntry;
                    break;

                case WorkflowStep.PackageTypeEntry:
                    foreach (var load in CurrentSession.Loads)
                    {
                        var result = _validation.ValidatePackageCount(load.PackagesPerLoad);
                        if (!result.IsValid)
                        {
                            validationErrors.Add($"Load {load.LoadNumber}: {result.Message}");
                        }
                        if (string.IsNullOrWhiteSpace(load.PackageTypeName))
                        {
                            validationErrors.Add($"Load {load.LoadNumber}: Package Type is required.");
                        }
                    }
                    if (validationErrors.Count > 0)
                    {
                        return WorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = WorkflowStep.Review;
                    break;

                case WorkflowStep.Review:
                    _logger.LogInfo("Transitioning from Review to Saving...");
                    CurrentStep = WorkflowStep.Saving;
                    break;

                case WorkflowStep.Saving:
                    CurrentStep = WorkflowStep.Complete;
                    break;

                default:
                    validationErrors.Add($"Cannot advance from step {CurrentStep}");
                    return WorkflowStepResult.ErrorResult(validationErrors);
            }

            // Persist session after step change
            _logger.LogInfo("Persisting session...");
            await PersistSessionAsync();
            _logger.LogInfo("Session persisted.");

            return WorkflowStepResult.SuccessResult(CurrentStep, $"Advanced to {CurrentStep}");
        }

        private void GenerateLoads()
        {
            // Remove previous loads from this batch if they exist
            if (_currentBatchLoads.Any())
            {
                foreach (var load in _currentBatchLoads)
                {
                    CurrentSession.Loads.Remove(load);
                }
                _currentBatchLoads.Clear();
            }
            
            for (int i = 1; i <= NumberOfLoads; i++)
            {
                var load = new Model_ReceivingLoad
                {
                    PartID = CurrentPart?.PartID ?? string.Empty,
                    PartType = CurrentPart?.PartType ?? string.Empty,
                    PoNumber = IsNonPOItem ? null : CurrentPONumber,
                    PoLineNumber = CurrentPart?.POLineNumber ?? string.Empty,
                    LoadNumber = CurrentSession.Loads.Count + 1, // Increment load number globally
                    IsNonPOItem = IsNonPOItem
                };
                CurrentSession.Loads.Add(load);
                _currentBatchLoads.Add(load);
            }
        }

        public WorkflowStepResult GoToPreviousStep()
        {
            switch (CurrentStep)
            {
                case WorkflowStep.LoadEntry:
                    CurrentStep = WorkflowStep.POEntry;
                    break;

                case WorkflowStep.WeightQuantityEntry:
                    CurrentStep = WorkflowStep.LoadEntry;
                    break;

                case WorkflowStep.HeatLotEntry:
                    CurrentStep = WorkflowStep.WeightQuantityEntry;
                    break;

                case WorkflowStep.PackageTypeEntry:
                    CurrentStep = WorkflowStep.HeatLotEntry;
                    break;

                case WorkflowStep.Review:
                    CurrentStep = WorkflowStep.PackageTypeEntry;
                    break;

                default:
                    return WorkflowStepResult.ErrorResult(new List<string> { $"Cannot go back from step {CurrentStep}" });
            }

            return WorkflowStepResult.SuccessResult(CurrentStep, $"Returned to {CurrentStep}");
        }
        public WorkflowStepResult GoToStep(WorkflowStep step)
        {
            CurrentStep = step;
            return WorkflowStepResult.SuccessResult(CurrentStep, $"Navigated to {CurrentStep}");
        }

        public async Task AddCurrentPartToSessionAsync()
        {
            // Commit current batch
            _currentBatchLoads.Clear();
            
            // Reset for next part
            CurrentPONumber = null;
            CurrentPart = null;
            IsNonPOItem = false;
            NumberOfLoads = 1;
            
            await PersistSessionAsync();
            CurrentStep = WorkflowStep.POEntry;
        }

        public async Task<SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null)
        {
            _logger.LogInfo("Starting session save.");
            var result = new SaveResult();

            messageProgress?.Report("Validating session...");
            percentProgress?.Report(10);

            // Validate session
            _logger.LogInfo("Validating session before save...");
            var validation = _validation.ValidateSession(CurrentSession.Loads);
            if (!validation.IsValid)
            {
                _logger.LogWarning($"Session validation failed: {string.Join(", ", validation.Errors)}");
                result.Success = false;
                result.Errors = validation.Errors;
                return result;
            }

            try
            {
                _logger.LogInfo("Reporting progress: Saving to local CSV...");
                messageProgress?.Report("Saving to local CSV...");
                percentProgress?.Report(30);

                // Save to CSV
                _logger.LogInfo("Calling _csvWriter.WriteToCSVAsync...");
                var csvResult = await _csvWriter.WriteToCSVAsync(CurrentSession.Loads);
                _logger.LogInfo($"CSV Write completed. Local: {csvResult.LocalSuccess}, Network: {csvResult.NetworkSuccess}");
                
                result.LocalCSVSuccess = csvResult.LocalSuccess;
                result.NetworkCSVSuccess = csvResult.NetworkSuccess;
                result.LocalCSVPath = _csvWriter.GetLocalCSVPath();
                result.NetworkCSVPath = _csvWriter.GetNetworkCSVPath();

                if (!csvResult.LocalSuccess)
                {var msg = $"Local CSV write failed: {csvResult.LocalError}";
                    result.Errors.Add(msg);
                    _logger.LogError(msg);
                }

                if (!csvResult.NetworkSuccess)
                {
                    var msg = $"Network CSV write failed: {csvResult.NetworkError}";
                    result.Warnings.Add(msg);
                    _logger.LogWarning(msg);
                }

                _logger.LogInfo("Reporting progress: Saving to database...");
                messageProgress?.Report("Saving to database...");
                percentProgress?.Report(60);

                // Save to database
                try
                {
                    _logger.LogInfo("Calling _mysqlReceiving.SaveReceivingLoadsAsync...");
                    int savedCount = await _mysqlReceiving.SaveReceivingLoadsAsync(CurrentSession.Loads);
                    result.DatabaseSuccess = true;
                    result.LoadsSaved = savedCount;
                    _logger.LogInfo($"Successfully saved {savedCount} loads to database.");
                }
                catch (Exception ex)
                {
                    result.DatabaseSuccess = false;
                    result.Errors.Add($"Database save failed: {ex.Message}");
                    _logger.LogError("Database save failed", ex);
                }

                _logger.LogInfo("Reporting progress: Finalizing...");
                messageProgress?.Report("Finalizing...");
                percentProgress?.Report(90);

                // Determine overall success
                result.Success = result.LocalCSVSuccess && result.DatabaseSuccess;

                // Clean up session file if successful
                if (result.Success)
                {
                    await _sessionManager.ClearSessionAsync();
                    _logger.LogInfo("Session saved and cleared successfully.");
                }
                else
                {
                    _logger.LogWarning("Session save completed with errors.");
                }

                percentProgress?.Report(100);
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Save operation failed: {ex.Message}");
                _logger.LogError("Save operation failed", ex);
                return result;
            }
        }

        public async Task ResetWorkflowAsync()
        {
            CurrentSession = new Model_ReceivingSession();
            NumberOfLoads = 1;
            CurrentStep = WorkflowStep.POEntry;
            await _sessionManager.ClearSessionAsync();
        }

        public async Task PersistSessionAsync()
        {
            if (CurrentSession.HasLoads)
            {
                await _sessionManager.SaveSessionAsync(CurrentSession);
            }
        }
    }
}
