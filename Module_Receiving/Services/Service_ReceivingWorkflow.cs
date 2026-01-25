using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for orchestrating the receiving workflow state machine.
    /// OBSOLETE: Use CQRS commands/queries via MediatR instead.
    /// This class implements the old 12-step wizard and is deprecated in favor of the consolidated 3-step workflow.
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    [Obsolete("Use CQRS commands/queries (StartWorkflowCommand, NavigateToStepCommand, etc.) via MediatR. This 12-step wizard service is replaced by the consolidated 3-step workflow.", DiagnosticId = "RECV001", UrlFormat = "https://github.com/JDKoll1982/MTM_Receiving_Application/wiki/CQRS-Migration")]
    public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
    {
        private readonly IService_SessionManager _sessionManager;
        private readonly IService_CSVWriter _csvWriter;
        private readonly IService_MySQL_Receiving _mysqlReceiving;
        private readonly IService_ReceivingValidation _validation;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_UserSessionManager _userSessionManager;
        private readonly List<Model_ReceivingLoad> _currentBatchLoads = new();

        [Obsolete("Use NavigateToStepCommand via MediatR.", DiagnosticId = "RECV001")]
        public event EventHandler? StepChanged;
        
        [Obsolete("Use MediatR handlers for status updates.", DiagnosticId = "RECV001")]
        public event EventHandler<string>? StatusMessageRaised;

        [Obsolete("Use MediatR handlers for status updates.", DiagnosticId = "RECV001")]
        public void RaiseStatusMessage(string message)
        {
            StatusMessageRaised?.Invoke(this, message);
        }

        private Enum_ReceivingWorkflowStep _currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
        
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public Enum_ReceivingWorkflowStep CurrentStep
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
        
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public Model_ReceivingSession CurrentSession { get; private set; } = new();
        
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public string? CurrentPONumber { get; set; }
        
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public Model_InforVisualPart? CurrentPart { get; set; }
        
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public bool IsNonPOItem { get; set; }
        
        [Obsolete("Use GetSessionQuery via MediatR.", DiagnosticId = "RECV001")]
        public int NumberOfLoads { get; set; } = 1;

        public Service_ReceivingWorkflow(
            IService_SessionManager sessionManager,
            IService_CSVWriter csvWriter,
            IService_MySQL_Receiving mysqlReceiving,
            IService_ReceivingValidation validation,
            IService_LoggingUtility logger,
            IService_ViewModelRegistry viewModelRegistry,
            IService_UserSessionManager userSessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            _mysqlReceiving = mysqlReceiving ?? throw new ArgumentNullException(nameof(mysqlReceiving));
            _validation = validation ?? throw new ArgumentNullException(nameof(validation));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _viewModelRegistry = viewModelRegistry ?? throw new ArgumentNullException(nameof(viewModelRegistry));
            _userSessionManager = userSessionManager ?? throw new ArgumentNullException(nameof(userSessionManager));
        }

        public async Task<bool> StartWorkflowAsync()
        {
            _logger.LogInfo("Starting receiving workflow.");
            // Try to load existing session
            var existingSession = await _sessionManager.LoadSessionAsync();

            if (existingSession?.HasLoads == true)
            {
                _logger.LogInfo("Restoring existing session.");
                CurrentSession = existingSession;
                // Determine which step to restore to based on session state
                CurrentStep = Enum_ReceivingWorkflowStep.Review; // Default to review if session exists
                return true; // Session restored
            }

            // Start fresh
            CurrentSession = new Model_ReceivingSession();
            NumberOfLoads = 1;

            // Check if user has a default receiving mode set
            var currentUser = _userSessionManager?.CurrentSession?.User;

            if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultReceivingMode))
            {
                // Skip mode selection and go directly to the default mode
                switch (currentUser.DefaultReceivingMode.ToLower())
                {
                    case "guided":
                        CurrentStep = Enum_ReceivingWorkflowStep.POEntry;
                        _logger.LogInfo("Starting in Guided mode (default)");
                        break;
                    case "manual":
                        CurrentStep = Enum_ReceivingWorkflowStep.ManualEntry;
                        _logger.LogInfo("Starting in Manual Entry mode (default)");
                        break;
                    case "edit":
                        CurrentStep = Enum_ReceivingWorkflowStep.EditMode;
                        _logger.LogInfo("Starting in Edit mode (default)");
                        break;
                    default:
                        // Invalid default, show mode selection
                        CurrentStep = Enum_ReceivingWorkflowStep.ModeSelection;
                        _logger.LogInfo("Invalid default mode, showing mode selection");
                        break;
                }
            }
            else
            {
                // No default mode, show mode selection
                CurrentStep = Enum_ReceivingWorkflowStep.ModeSelection;
                _logger.LogInfo("No default mode set, showing mode selection");
            }

            return false; // New session
        }

        public async Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync()
        {
            // Validate current step before advancing
            var validationErrors = new List<string>();

            switch (CurrentStep)
            {
                case Enum_ReceivingWorkflowStep.ModeSelection:
                    // Transition handled by GoToStep
                    return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep);

                case Enum_ReceivingWorkflowStep.ManualEntry:
                    // Manual entry goes directly to saving/review
                    // Validation happens on save
                    // Check for quality holds and block if not acknowledged
                    var loadsWithHolds = CurrentSession.Loads.Where(l => l.IsQualityHoldRequired && !l.IsQualityHoldAcknowledged).ToList();
                    if (loadsWithHolds.Count > 0)
                    {
                        validationErrors.Add($"Quality hold acknowledgment required for {loadsWithHolds.Count} load(s) before proceeding.");
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = Enum_ReceivingWorkflowStep.Saving;
                    return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep);

                case Enum_ReceivingWorkflowStep.EditMode:
                    // Edit mode goes to saving/review
                    CurrentStep = Enum_ReceivingWorkflowStep.Saving;
                    return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep);

                case Enum_ReceivingWorkflowStep.POEntry:
                    if (string.IsNullOrEmpty(CurrentPONumber) && !IsNonPOItem)
                    {
                        validationErrors.Add("PO Number is required.");
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    if (CurrentPart == null)
                    {
                        validationErrors.Add("Part selection is required.");
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }

                    // Update session with PO/Part info
                    CurrentSession.IsNonPO = IsNonPOItem;
                    CurrentSession.PoNumber = IsNonPOItem ? null : CurrentPONumber;

                    CurrentStep = Enum_ReceivingWorkflowStep.LoadEntry;
                    break;

                case Enum_ReceivingWorkflowStep.LoadEntry:
                    if (NumberOfLoads < 1)
                    {
                        validationErrors.Add("Number of loads must be at least 1.");
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    GenerateLoads();
                    CurrentStep = Enum_ReceivingWorkflowStep.WeightQuantityEntry;
                    break;

                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
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
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = Enum_ReceivingWorkflowStep.HeatLotEntry;
                    break;

                case Enum_ReceivingWorkflowStep.HeatLotEntry:
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
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = Enum_ReceivingWorkflowStep.PackageTypeEntry;
                    break;

                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
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
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = Enum_ReceivingWorkflowStep.Review;
                    break;

                case Enum_ReceivingWorkflowStep.Review:
                    _logger.LogInfo("Transitioning from Review to Saving...");
                    CurrentStep = Enum_ReceivingWorkflowStep.Saving;
                    break;

                case Enum_ReceivingWorkflowStep.Saving:
                    CurrentStep = Enum_ReceivingWorkflowStep.Complete;
                    break;

                default:
                    validationErrors.Add($"Cannot advance from step {CurrentStep}");
                    return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
            }

            // Persist session after step change
            _logger.LogInfo("Persisting session...");
            await PersistSessionAsync();
            _logger.LogInfo("Session persisted.");

            return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Advanced to {CurrentStep}");
        }

        private void GenerateLoads()
        {
            // Remove previous loads from this batch if they exist
            if (_currentBatchLoads.Count > 0)
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

        public Model_ReceivingWorkflowStepResult GoToPreviousStep()
        {
            switch (CurrentStep)
            {
                case Enum_ReceivingWorkflowStep.LoadEntry:
                    CurrentStep = Enum_ReceivingWorkflowStep.POEntry;
                    break;

                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
                    CurrentStep = Enum_ReceivingWorkflowStep.LoadEntry;
                    break;

                case Enum_ReceivingWorkflowStep.HeatLotEntry:
                    CurrentStep = Enum_ReceivingWorkflowStep.WeightQuantityEntry;
                    break;

                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
                    CurrentStep = Enum_ReceivingWorkflowStep.HeatLotEntry;
                    break;

                case Enum_ReceivingWorkflowStep.Review:
                    CurrentStep = Enum_ReceivingWorkflowStep.PackageTypeEntry;
                    break;

                default:
                    return Model_ReceivingWorkflowStepResult.ErrorResult(new List<string> { $"Cannot go back from step {CurrentStep}" });
            }

            return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Returned to {CurrentStep}");
        }
        public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step)
        {
            CurrentStep = step;
            return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Navigated to {CurrentStep}");
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
            CurrentStep = Enum_ReceivingWorkflowStep.POEntry;
        }

        public void ClearUIInputs()
        {
            _viewModelRegistry.ClearAllInputs();
        }

        public async Task<Model_SaveResult> SaveToCSVOnlyAsync()
        {
            var result = new Model_SaveResult();

            // Validate session
            var validation = _validation.ValidateSession(CurrentSession.Loads);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors = validation.Errors;
                return result;
            }

            try
            {
                var csvResult = await _csvWriter.WriteToCSVAsync(CurrentSession.Loads);

                result.LocalCSVSuccess = csvResult.LocalSuccess;
                result.NetworkCSVSuccess = csvResult.NetworkSuccess;
                result.LocalCSVPath = _csvWriter.GetLocalCSVPath();
                result.NetworkCSVPath = _csvWriter.GetNetworkCSVPath();

                if (!csvResult.LocalSuccess)
                {
                    result.Errors.Add($"Local CSV write failed: {csvResult.LocalError}");
                }

                if (!csvResult.NetworkSuccess)
                {
                    result.Warnings.Add($"Network CSV write failed: {csvResult.NetworkError}");
                }

                result.Success = result.LocalCSVSuccess; // Network failure is just a warning
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"CSV save failed: {ex.Message}");
                _logger.LogError("CSV save failed", ex);
            }

            return result;
        }

        public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
        {
            var result = new Model_SaveResult();

            // Validate session
            var validation = _validation.ValidateSession(CurrentSession.Loads);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors = validation.Errors;
                return result;
            }

            try
            {
                int savedCount = await _mysqlReceiving.SaveReceivingLoadsAsync(CurrentSession.Loads);
                result.DatabaseSuccess = true;
                result.LoadsSaved = savedCount;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.DatabaseSuccess = false;
                result.Success = false;
                result.Errors.Add($"Database save failed: {ex.Message}");
                _logger.LogError("Database save failed", ex);
            }

            return result;
        }

        public async Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null)
        {
            _logger.LogInfo("Starting session save.");
            var result = new Model_SaveResult();

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
                var csvResult = await SaveToCSVOnlyAsync();

                result.LocalCSVSuccess = csvResult.LocalCSVSuccess;
                result.NetworkCSVSuccess = csvResult.NetworkCSVSuccess;
                result.LocalCSVPath = csvResult.LocalCSVPath;
                result.NetworkCSVPath = csvResult.NetworkCSVPath;
                result.Errors.AddRange(csvResult.Errors);
                result.Warnings.AddRange(csvResult.Warnings);

                _logger.LogInfo("Reporting progress: Saving to database...");
                messageProgress?.Report("Saving to database...");
                percentProgress?.Report(60);

                // Save to database
                var dbResult = await SaveToDatabaseOnlyAsync();

                result.DatabaseSuccess = dbResult.DatabaseSuccess;
                result.LoadsSaved = dbResult.LoadsSaved;
                if (!dbResult.Success)
                {
                    result.Errors.AddRange(dbResult.Errors);
                }

                _logger.LogInfo("Reporting progress: Finalizing...");
                messageProgress?.Report("Finalizing...");
                percentProgress?.Report(90);

                // Final success check
                // Success if Local CSV worked AND Database worked
                result.Success = result.LocalCSVSuccess && result.DatabaseSuccess;

                if (result.Success)
                {
                    _logger.LogInfo("Save completed successfully. Clearing session.");
                    // Clear session
                    await _sessionManager.ClearSessionAsync();
                    CurrentSession.Loads.Clear();

                    // Also clear CSV files since we saved successfully
                    await _csvWriter.ClearCSVFilesAsync();
                }
                else
                {
                    _logger.LogWarning($"Save completed with errors. Success: {result.Success}");
                }

                percentProgress?.Report(100);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error during save session", ex);
                result.Success = false;
                result.Errors.Add($"Unexpected error: {ex.Message}");
                return result;
            }
        }

        public async Task ResetWorkflowAsync()
        {
            CurrentSession = new Model_ReceivingSession();
            NumberOfLoads = 1;
            CurrentStep = Enum_ReceivingWorkflowStep.POEntry;
            CurrentPONumber = null;
            CurrentPart = null;
            IsNonPOItem = false;
            _currentBatchLoads.Clear();

            _viewModelRegistry.ClearAllInputs();

            await _sessionManager.ClearSessionAsync();
            StepChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Model_CSVDeleteResult> ResetCSVFilesAsync()
        {
            _logger.LogInfo("Resetting CSV files requested.");
            return await _csvWriter.ClearCSVFilesAsync();
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


