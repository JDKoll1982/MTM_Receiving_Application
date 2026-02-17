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
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
    {
        private readonly IService_SessionManager _sessionManager;
        private readonly IService_XLSWriter _xlsWriter;
        private readonly IService_MySQL_Receiving _mysqlReceiving;
        private readonly IService_ReceivingValidation _validation;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_UserSessionManager _userSessionManager;
        private readonly List<Model_ReceivingLoad> _currentBatchLoads = new();

        public event EventHandler? StepChanged;
        public event EventHandler<string>? StatusMessageRaised;

        public void RaiseStatusMessage(string message)
        {
            StatusMessageRaised?.Invoke(this, message);
        }

        private Enum_ReceivingWorkflowStep _currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
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
        public Model_ReceivingSession CurrentSession { get; private set; } = new();
        public string? CurrentPONumber { get; set; }
        public Model_InforVisualPart? CurrentPart { get; set; }
        public bool IsNonPOItem { get; set; }
        public int NumberOfLoads { get; set; } = 1;

        public Service_ReceivingWorkflow(
            IService_SessionManager sessionManager,
            IService_XLSWriter xlsWriter,
            IService_MySQL_Receiving mysqlReceiving,
            IService_ReceivingValidation validation,
            IService_LoggingUtility logger,
            IService_ViewModelRegistry viewModelRegistry,
            IService_UserSessionManager userSessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _xlsWriter = xlsWriter ?? throw new ArgumentNullException(nameof(xlsWriter));
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
                    // Set "Nothing Entered" for blank heat/lot fields before validation
                    foreach (var load in CurrentSession.Loads)
                    {
                        if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                        {
                            load.HeatLotNumber = "Nothing Entered";
                        }
                    }
                    
                    // Validate heat/lot numbers (only checks max length now)
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
                    IsNonPOItem = IsNonPOItem,
                    EmployeeNumber = CurrentSession.User?.EmployeeNumber ?? 0,
                    UserId = CurrentSession.User?.WindowsUsername ?? string.Empty
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

        public async Task<Model_SaveResult> SaveToXLSOnlyAsync()
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
                var xlsResult = await _xlsWriter.WriteToXLSAsync(CurrentSession.Loads);

                result.LocalXLSSuccess = true;
                result.NetworkXLSSuccess = xlsResult.NetworkSuccess;
                result.NetworkXLSPath = await _xlsWriter.GetNetworkXLSPathAsync();

                if (!xlsResult.NetworkSuccess)
                {
                    result.Errors.Add($"Network XLS write failed: {xlsResult.NetworkError}");
                }

                result.Success = result.NetworkXLSSuccess;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"XLS save failed: {ex.Message}");
                _logger.LogError("XLS save failed", ex);
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
                _logger.LogInfo("Reporting progress: Saving to local XLS...");
                messageProgress?.Report("Saving to local XLS...");
                percentProgress?.Report(30);

                // Save to XLS
                var xlsResult = await SaveToXLSOnlyAsync();

                result.LocalXLSSuccess = xlsResult.LocalXLSSuccess;
                result.NetworkXLSSuccess = xlsResult.NetworkXLSSuccess;
                result.LocalXLSPath = xlsResult.LocalXLSPath;
                result.NetworkXLSPath = xlsResult.NetworkXLSPath;
                result.Errors.AddRange(xlsResult.Errors);
                result.Warnings.AddRange(xlsResult.Warnings);

                // Generate error message if CSV save failed
                if (!result.LocalXLSSuccess && xlsResult.Errors.Count > 0)
                {
                    result.XLSFileErrorMessage = "XLS save failed: " + string.Join("; ", xlsResult.Errors);
                }

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
                    // Populate Database error message if database save failed
                    if (dbResult.Errors.Count > 0)
                    {
                        result.DatabaseErrorMessage = "Database save failed: " + string.Join("; ", dbResult.Errors);
                    }
                }

                _logger.LogInfo("Reporting progress: Finalizing...");
                messageProgress?.Report("Finalizing...");
                percentProgress?.Report(90);

                // Final success check
                // Success if Local XLS worked AND Database worked
                result.Success = result.LocalXLSSuccess && result.DatabaseSuccess;

                if (result.Success)
                {
                    _logger.LogInfo("Save completed successfully. Clearing session.");
                    // Clear session
                    await _sessionManager.ClearSessionAsync();
                    CurrentSession.Loads.Clear();
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
                result.XLSFileErrorMessage = "Unexpected error during save: " + ex.Message;
                result.DatabaseErrorMessage = "Unexpected error during save: " + ex.Message;
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

        public async Task<Model_XLSDeleteResult> ResetXLSFilesAsync()
        {
            _logger.LogInfo("Resetting XLS files requested.");
            return await _xlsWriter.ClearXLSFilesAsync();
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


