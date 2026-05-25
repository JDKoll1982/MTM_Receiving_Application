using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Events;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for orchestrating the receiving workflow state machine.
    /// Manages step transitions, validation gates, and session state.
    /// </summary>
    public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
    {
        private static readonly Regex MockPoPattern = new(
            @"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[A-Za-z]?)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        private readonly IService_SessionManager _sessionManager;
        private readonly IService_ReceivingLabelData _labelDataService;
        private readonly IService_MySQL_Receiving _mysqlReceiving;
        private readonly IService_ReceivingValidation _validation;
        private readonly IService_QualityHoldWarning _qualityHoldWarning;
        private readonly IService_ReceivingSettings _receivingSettings;
        private readonly IService_AppSettings _appSettings;
        private readonly IService_InforVisualMockDataCatalog _mockDataCatalog;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_UserSessionManager _userSessionManager;
        private readonly IService_UserPrivileges _userPrivileges;
        private readonly List<Model_ReceivingLoad> _currentBatchLoads = new();
        private readonly WeakEventSource _stepChanged = new();
        private readonly WeakEventSource<string> _statusMessageRaised = new();

        public event EventHandler? StepChanged
        {
            add => _stepChanged.Subscribe(value);
            remove => _stepChanged.Unsubscribe(value);
        }

        public event EventHandler<string>? StatusMessageRaised
        {
            add => _statusMessageRaised.Subscribe(value);
            remove => _statusMessageRaised.Unsubscribe(value);
        }

        public void RaiseStatusMessage(string message)
        {
            _statusMessageRaised.Raise(this, message);
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
                    _stepChanged.Raise(this, EventArgs.Empty);
                    _logger.LogInfo($"Step changed to {value} (event fired)");
                }
            }
        }
        public Model_ReceivingSession CurrentSession { get; private set; } = new();
        public string? CurrentPONumber { get; set; }
        public Model_InforVisualPart? CurrentPart { get; set; }
        public bool IsNonPOItem { get; set; }
        public int NumberOfLoads { get; set; } = 1;
        public string CurrentLocation { get; set; } = string.Empty;

        // PO Header Data (from Model_InforVisualPO)
        public string? CurrentPOVendor { get; set; }
        public string? CurrentPOStatus { get; set; }
        public DateTime? CurrentPODueDate { get; set; }
        public Enum_DataSourceType RequestedEditDataSource { get; set; } =
            Enum_DataSourceType.Memory;

        public Service_ReceivingWorkflow(
            IService_SessionManager sessionManager,
            IService_ReceivingLabelData labelDataService,
            IService_MySQL_Receiving mysqlReceiving,
            IService_ReceivingValidation validation,
            IService_QualityHoldWarning qualityHoldWarning,
            IService_ReceivingSettings receivingSettings,
            IService_AppSettings appSettings,
            IService_InforVisualMockDataCatalog mockDataCatalog,
            IService_LoggingUtility logger,
            IService_ViewModelRegistry viewModelRegistry,
            IService_UserSessionManager userSessionManager,
            IService_UserPrivileges userPrivileges
        )
        {
            _sessionManager =
                sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _labelDataService =
                labelDataService ?? throw new ArgumentNullException(nameof(labelDataService));
            _mysqlReceiving =
                mysqlReceiving ?? throw new ArgumentNullException(nameof(mysqlReceiving));
            _validation = validation ?? throw new ArgumentNullException(nameof(validation));
            _qualityHoldWarning =
                qualityHoldWarning ?? throw new ArgumentNullException(nameof(qualityHoldWarning));
            _receivingSettings =
                receivingSettings ?? throw new ArgumentNullException(nameof(receivingSettings));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _mockDataCatalog =
                mockDataCatalog ?? throw new ArgumentNullException(nameof(mockDataCatalog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _viewModelRegistry =
                viewModelRegistry ?? throw new ArgumentNullException(nameof(viewModelRegistry));
            _userSessionManager =
                userSessionManager ?? throw new ArgumentNullException(nameof(userSessionManager));
            _userPrivileges =
                userPrivileges ?? throw new ArgumentNullException(nameof(userPrivileges));
        }

        public async Task<bool> StartWorkflowAsync()
        {
            _logger.LogInfo("Starting receiving workflow.");
            var currentUser = _userSessionManager?.CurrentSession?.User;

            // Receiving drafts are intentionally not restored across app launches.
            // If a stale session file exists, clear it so reopening always starts clean.
            if (_sessionManager.SessionExists())
            {
                _logger.LogInfo("Discarding stale receiving session on workflow start.");
                await _sessionManager.ClearSessionAsync();
            }

            // Start fresh
            CurrentSession = new Model_ReceivingSession { User = currentUser };
            NumberOfLoads = 1;
            CurrentLocation = string.Empty;
            RequestedEditDataSource = Enum_DataSourceType.Memory;

            // Check if user has a default receiving mode set
            var currentUserId = currentUser?.EmployeeNumber;

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
                        RequestedEditDataSource = Enum_DataSourceType.CurrentLabels;
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
                var configuredDefaultMode = (
                    await _receivingSettings.GetStringAsync(
                        MTM_Receiving_Application
                            .Module_Receiving
                            .Settings
                            .ReceivingSettingsKeys
                            .BusinessRules
                            .DefaultModeOnStartup,
                        currentUserId
                    )
                ).Trim();
                var normalizedDefaultMode = configuredDefaultMode.ToLowerInvariant();

                if (normalizedDefaultMode == "guided")
                {
                    CurrentStep = Enum_ReceivingWorkflowStep.POEntry;
                    _logger.LogInfo("Starting in Guided mode (settings default)");
                }
                else if (
                    normalizedDefaultMode == "manualentry"
                    || normalizedDefaultMode == "manual"
                )
                {
                    CurrentStep = Enum_ReceivingWorkflowStep.ManualEntry;
                    _logger.LogInfo("Starting in Manual Entry mode (settings default)");
                }
                else if (normalizedDefaultMode == "editmode" || normalizedDefaultMode == "edit")
                {
                    CurrentStep = Enum_ReceivingWorkflowStep.EditMode;
                    RequestedEditDataSource = Enum_DataSourceType.CurrentLabels;
                    _logger.LogInfo("Starting in Edit mode (settings default)");
                }
                else
                {
                    CurrentStep = Enum_ReceivingWorkflowStep.ModeSelection;
                    _logger.LogInfo("No default mode set, showing mode selection");
                }
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
                    // Manual entry goes directly to saving.
                    // Validation happens on save; check for unacknowledged quality holds first.
                    var loadsWithHolds = CurrentSession
                        .Loads.Where(l => l.IsQualityHoldRequired && !l.IsQualityHoldAcknowledged)
                        .ToList();
                    if (loadsWithHolds.Count > 0)
                    {
                        validationErrors.Add(
                            $"Quality hold acknowledgment required for {loadsWithHolds.Count} load(s) before proceeding."
                        );
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }
                    CurrentStep = Enum_ReceivingWorkflowStep.Saving;
                    break; // Fall through to PersistSessionAsync so session is saved before the save cycle begins

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
                    CurrentSession.PoNumber = string.IsNullOrWhiteSpace(CurrentPONumber)
                        ? null
                        : CurrentPONumber;

                    CurrentStep = Enum_ReceivingWorkflowStep.LoadEntry;
                    break;

                case Enum_ReceivingWorkflowStep.LoadEntry:
                    if (NumberOfLoads < 1)
                    {
                        validationErrors.Add("Number of loads must be at least 1.");
                        return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
                    }

                    if (string.IsNullOrWhiteSpace(CurrentLocation))
                    {
                        var defaultLocation = (
                            await _receivingSettings.GetStringAsync(
                                MTM_Receiving_Application
                                    .Module_Receiving
                                    .Settings
                                    .ReceivingSettingsKeys
                                    .Defaults
                                    .DefaultLocation
                            )
                        ).Trim();
                        if (string.IsNullOrWhiteSpace(defaultLocation) is false)
                        {
                            CurrentLocation = defaultLocation;
                        }
                    }

                    var locationValidation = await _validation.ValidateLocationAsync(
                        CurrentLocation
                    );
                    if (!locationValidation.IsValid)
                    {
                        validationErrors.Add(locationValidation.Message);
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
                            validationErrors.Add(
                                $"Load {load.LoadNumber}: Package Type is required."
                            );
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

            return Model_ReceivingWorkflowStepResult.SuccessResult(
                CurrentStep,
                $"Advanced to {CurrentStep}"
            );
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
                // Get current logged-in user from UserSessionManager
                var currentUser = _userSessionManager?.CurrentSession?.User;

                var load = new Model_ReceivingLoad
                {
                    PartID = CurrentPart?.PartID ?? string.Empty,
                    // PartType will be auto-set by OnPartIDChanged logic (MMC=Coil, MMF=Sheet)
                    PoNumber = string.IsNullOrWhiteSpace(CurrentPONumber) ? null : CurrentPONumber,
                    PoLineNumber = CurrentPart?.POLineNumber ?? string.Empty,
                    LoadNumber = CurrentSession.Loads.Count + 1, // Increment load number globally
                    IsNonPOItem = IsNonPOItem,
                    EmployeeNumber = currentUser?.EmployeeNumber ?? 0,
                    UserId = currentUser?.WindowsUsername ?? string.Empty,
                    // Additional Infor Visual / PO data
                    PartDescription = CurrentPart?.Description ?? string.Empty,
                    UnitOfMeasure = CurrentPart?.UnitOfMeasure ?? "EA",
                    QtyOrdered = CurrentPart?.QtyOrdered ?? 0,
                    RemainingQuantity = CurrentPart?.RemainingQuantity ?? 0,
                    InitialLocation = CurrentLocation?.Trim() ?? string.Empty,
                    // PO Header Data
                    PoVendor = CurrentPOVendor ?? string.Empty,
                    PoStatus = CurrentPOStatus ?? string.Empty,
                    PoDueDate = CurrentPODueDate,
                    IsQualityHoldRequired = _appSettings.GetUseInforVisualMockData()
                        ? CurrentPart?.RequiresQualityHold == true
                        : CurrentPart?.RequiresQualityHold == true
                            || (
                                CurrentPart?.PartID?.Contains(
                                    "MMFSR",
                                    StringComparison.OrdinalIgnoreCase
                                ) == true
                            )
                            || (
                                CurrentPart?.PartID?.Contains(
                                    "MMCSR",
                                    StringComparison.OrdinalIgnoreCase
                                ) == true
                            ),
                    QualityHoldRestrictionType =
                        CurrentPart?.RequiresQualityHold == true
                            ? CurrentPart.QualityHoldRestrictionType?.Trim() ?? string.Empty
                        : CurrentPart?.PartID?.Contains("MMFSR", StringComparison.OrdinalIgnoreCase)
                        == true
                            ? "Sheet Material - Quality Hold Required"
                        : CurrentPart?.PartID?.Contains("MMCSR", StringComparison.OrdinalIgnoreCase)
                        == true
                            ? "Coil Material - Quality Hold Required"
                        : string.Empty,
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
                    return Model_ReceivingWorkflowStepResult.ErrorResult(
                        new List<string> { $"Cannot go back from step {CurrentStep}" }
                    );
            }

            return Model_ReceivingWorkflowStepResult.SuccessResult(
                CurrentStep,
                $"Returned to {CurrentStep}"
            );
        }

        public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step)
        {
            CurrentStep = step;
            return Model_ReceivingWorkflowStepResult.SuccessResult(
                CurrentStep,
                $"Navigated to {CurrentStep}"
            );
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
            CurrentLocation = string.Empty;
            RequestedEditDataSource = Enum_DataSourceType.Memory;

            CurrentStep = Enum_ReceivingWorkflowStep.POEntry;
        }

        public void ClearUIInputs()
        {
            _viewModelRegistry.ClearAllInputs();
        }

        public async Task<Model_SaveResult> SaveToLabelDataOnlyAsync()
        {
            _logger.LogInfo($"{nameof(SaveToLabelDataOnlyAsync)} redirected to label queue save.");
            var result = await SaveToDatabaseOnlyAsync();

            result.LabelQueueSuccess = result.DatabaseSuccess;
            result.ArchiveQueueSuccess = result.DatabaseSuccess;
            result.LabelQueuePath = "MySQL.receiving_label_data";
            result.ArchiveQueuePath = "MySQL.receiving_history (archive on clear)";

            return result;
        }

        public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
        {
            var result = new Model_SaveResult();
            var loadsToSave = CurrentSession.Loads.ToList();

            // Validate session
            var validation = _validation.ValidateSession(loadsToSave);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors = validation.Errors;
                return result;
            }

            try
            {
                int savedCount = await _mysqlReceiving.SaveReceivingLoadsAsync(loadsToSave);

                if (_appSettings.GetUseInforVisualMockData())
                {
                    var mockTransactions = BuildMockReceivingTransactions(loadsToSave);
                    var mockAppendResult = await _mockDataCatalog.AppendReceivingTransactionsAsync(
                        mockTransactions
                    );

                    if (!mockAppendResult.IsSuccess)
                    {
                        result.Warnings.Add(
                            $"Mock data update failed: {mockAppendResult.ErrorMessage}"
                        );
                        _logger.LogError(
                            $"Mock data update failed after receiving save: {mockAppendResult.ErrorMessage}"
                        );
                    }
                }

                result.DatabaseSuccess = true;
                result.LoadsSaved = savedCount;
                if (savedCount < loadsToSave.Count)
                {
                    result.Warnings.Add(
                        $"{loadsToSave.Count - savedCount} load(s) were already in the database and were skipped."
                    );
                }
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

        public async Task<Model_SaveResult> SaveSessionAsync(
            IProgress<string>? messageProgress = null,
            IProgress<int>? percentProgress = null
        )
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
                _logger.LogWarning(
                    $"Session validation failed: {string.Join(", ", validation.Errors)}"
                );
                result.Success = false;
                result.Errors = validation.Errors;
                return result;
            }

            try
            {
                var loadsNeedingFinalAcknowledgment = CurrentSession
                    .Loads.Where(load =>
                        load.IsQualityHoldRequired && !load.IsQualityHoldAcknowledged
                    )
                    .ToList();

                if (loadsNeedingFinalAcknowledgment.Count > 0)
                {
                    _logger.LogInfo(
                        $"Requesting final quality hold acknowledgment for {loadsNeedingFinalAcknowledgment.Count} load(s) before queue save."
                    );

                    var acknowledged = await _qualityHoldWarning.ConfirmBeforeSaveAsync(
                        loadsNeedingFinalAcknowledgment
                    );

                    if (!acknowledged)
                    {
                        result.Success = false;
                        result.Errors.Add(
                            "Quality hold acknowledgment required before saving to receiving_label_data."
                        );
                        result.LabelDataErrorMessage =
                            "Save cancelled because final quality hold acknowledgment was not confirmed.";
                        result.DatabaseErrorMessage = result.LabelDataErrorMessage;
                        return result;
                    }

                    foreach (var load in loadsNeedingFinalAcknowledgment)
                    {
                        load.IsQualityHoldAcknowledged = true;
                    }
                }

                _logger.LogInfo("Reporting progress: Preparing label queue save...");
                messageProgress?.Report("Preparing label queue save...");
                percentProgress?.Report(30);

                _logger.LogInfo("Reporting progress: Saving to label queue...");
                messageProgress?.Report("Saving to label queue...");
                percentProgress?.Report(60);

                // Save to database
                var dbResult = await SaveToDatabaseOnlyAsync();

                result.DatabaseSuccess = dbResult.DatabaseSuccess;
                result.LoadsSaved = dbResult.LoadsSaved;
                result.LabelQueueSuccess = dbResult.DatabaseSuccess;
                result.ArchiveQueueSuccess = dbResult.DatabaseSuccess;
                result.LabelQueuePath = "MySQL.receiving_label_data";
                result.ArchiveQueuePath = "MySQL.receiving_history (archive on clear)";
                if (!dbResult.Success)
                {
                    result.Errors.AddRange(dbResult.Errors);
                    // Populate Database error message if database save failed
                    if (dbResult.Errors.Count > 0)
                    {
                        result.DatabaseErrorMessage =
                            "Database save failed: " + string.Join("; ", dbResult.Errors);
                    }
                }

                _logger.LogInfo("Reporting progress: Finalizing...");
                messageProgress?.Report("Finalizing...");
                percentProgress?.Report(90);

                // Final success check
                result.Success = result.DatabaseSuccess;

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
                result.LabelDataErrorMessage = "Unexpected error during save: " + ex.Message;
                result.DatabaseErrorMessage = "Unexpected error during save: " + ex.Message;
                return result;
            }
        }

        public async Task ResetWorkflowAsync()
        {
            CurrentSession = new Model_ReceivingSession
            {
                User = _userSessionManager.CurrentSession?.User,
            };
            NumberOfLoads = 1;
            CurrentStep = Enum_ReceivingWorkflowStep.ModeSelection;
            CurrentPONumber = null;
            CurrentPart = null;
            IsNonPOItem = false;
            CurrentLocation = string.Empty;
            RequestedEditDataSource = Enum_DataSourceType.Memory;
            _currentBatchLoads.Clear();

            _viewModelRegistry.ClearAllInputs();

            await _sessionManager.ClearSessionAsync();
            _stepChanged.Raise(this, EventArgs.Empty);
        }

        public async Task<Model_LabelDataClearResult> ResetLabelDataAsync(bool clearAllRows = false)
        {
            var currentUser = _userSessionManager.CurrentSession?.User;
            var employeeNumber = currentUser?.EmployeeNumber ?? 0;
            if (!clearAllRows && employeeNumber <= 0)
            {
                return new Model_LabelDataClearResult
                {
                    LabelQueueCleared = false,
                    ArchiveQueueCleared = false,
                    ArchiveQueueError =
                        "A valid 4-digit employee number is required to clear your receiving label rows.",
                };
            }

            if (clearAllRows)
            {
                var authorizationResult = await EnsureAdminOrDeveloperAsync(employeeNumber);
                if (!authorizationResult.IsSuccess)
                {
                    return new Model_LabelDataClearResult
                    {
                        LabelQueueCleared = false,
                        ArchiveQueueCleared = false,
                        ArchiveQueueError =
                            authorizationResult.ErrorMessage
                            ?? "Only Admin or Developer users can clear all receiving label rows.",
                    };
                }
            }

            var archivedBy = currentUser?.WindowsUsername ?? Environment.UserName;
            _logger.LogInfo($"Clear Label Data requested by {archivedBy}.");

            var clearResult = await _mysqlReceiving.ClearLabelDataToHistoryAsync(
                archivedBy,
                employeeNumber,
                clearAllRows
            );
            if (clearResult.IsSuccess)
            {
                _logger.LogInfo($"Clear Label Data succeeded. Rows moved: {clearResult.Data}");
                return new Model_LabelDataClearResult
                {
                    LabelQueueCleared = true,
                    ArchiveQueueCleared = true,
                };
            }

            _logger.LogWarning($"Clear Label Data failed: {clearResult.ErrorMessage}");
            return new Model_LabelDataClearResult
            {
                LabelQueueCleared = false,
                ArchiveQueueCleared = false,
                ArchiveQueueError = clearResult.ErrorMessage,
            };
        }

        private async Task<Model_Dao_Result> EnsureAdminOrDeveloperAsync(int employeeNumber)
        {
            if (employeeNumber <= 0)
            {
                return Model_Dao_Result_Factory.Failure(
                    "Only Admin or Developer users can clear all receiving label rows."
                );
            }

            if (!_userPrivileges.IsInitialized || _userPrivileges.CurrentUserId != employeeNumber)
            {
                var initializeResult = await _userPrivileges.InitializeAsync(employeeNumber);
                if (!initializeResult.IsSuccess)
                {
                    return initializeResult;
                }
            }

            return _userPrivileges.HasAnyRole("Admin", "Developer")
                ? Model_Dao_Result_Factory.Success()
                : Model_Dao_Result_Factory.Failure(
                    "Only Admin or Developer users can clear all receiving label rows."
                );
        }

        public async Task<bool> HasActiveLabelDataAsync()
        {
            return await _mysqlReceiving.HasActiveLabelDataAsync();
        }

        public async Task PersistSessionAsync()
        {
            if (_sessionManager.SessionExists())
            {
                await _sessionManager.ClearSessionAsync();
            }
        }

        private static string NormalizeLocation(string? location)
        {
            return string.IsNullOrWhiteSpace(location)
                ? "RECV"
                : location.Trim().ToUpperInvariant();
        }

        private List<Model_InforVisualMockReceivingTransaction> BuildMockReceivingTransactions(
            IReadOnlyList<Model_ReceivingLoad> loads
        )
        {
            var transactions = new List<Model_InforVisualMockReceivingTransaction>();

            foreach (
                var receiptGroup in loads.GroupBy(load => new
                {
                    PONumber = NormalizeMockPoNumber(load.PoNumber),
                    PartId = load.PartID?.Trim().ToUpperInvariant() ?? string.Empty,
                    POLineNumber = load.PoLineNumber?.Trim() ?? string.Empty,
                    ReceivedDate = load.ReceivedDate.Date,
                })
            )
            {
                var orderedLoads = receiptGroup
                    .OrderByDescending(load => load.WeightQuantity)
                    .ThenBy(load => load.LoadNumber)
                    .ThenBy(load => load.LoadID)
                    .ToList();
                var assignedLocations = ResolveMockCurrentLocations(orderedLoads);

                transactions.AddRange(
                    orderedLoads.Select(load => new Model_InforVisualMockReceivingTransaction
                    {
                        SourceLoadId = load.LoadID.ToString(),
                        PONumber = NormalizeMockPoNumber(load.PoNumber),
                        PartID = load.PartID,
                        POLineNumber = load.PoLineNumber,
                        Quantity = load.WeightQuantity,
                        UnitOfMeasure = load.UnitOfMeasure,
                        ReceivedDate = load.ReceivedDate,
                        TransactionDate = load.ReceivedDate,
                        ReceiptWarehouseId = "002",
                        ReceiptLocationId = NormalizeLocation(load.InitialLocation),
                        CurrentWarehouseId = "002",
                        CurrentLocationId = assignedLocations[load.LoadID],
                        EmployeeNumber = load.EmployeeNumber,
                        UserId = load.UserId ?? string.Empty,
                    })
                );
            }

            return transactions;
        }

        private Dictionary<Guid, string> ResolveMockCurrentLocations(
            IReadOnlyList<Model_ReceivingLoad> loads
        )
        {
            var assignedLocations = new Dictionary<Guid, string>();
            if (loads.Count == 0)
            {
                return assignedLocations;
            }

            if (loads.Count == 1)
            {
                assignedLocations[loads[0].LoadID] = ResolveMockCurrentLocation(loads[0]);
                return assignedLocations;
            }

            var primaryLocation = ResolveMockCurrentLocation(loads[0]);
            var receiptLocation = NormalizeLocation(loads[0].InitialLocation);
            var catalogLocations = _mockDataCatalog
                .GetLocations()
                .Select(NormalizeLocation)
                .Where(location =>
                    !string.IsNullOrWhiteSpace(location)
                    && !string.Equals(location, receiptLocation, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(location, primaryLocation, StringComparison.OrdinalIgnoreCase)
                )
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var candidateLocations = new List<string> { receiptLocation, primaryLocation };
            candidateLocations.AddRange(catalogLocations);

            var randomSeed = HashCode.Combine(
                NormalizeMockPoNumber(loads[0].PoNumber),
                loads[0].PartID?.Trim().ToUpperInvariant() ?? string.Empty,
                loads[0].PoLineNumber?.Trim() ?? string.Empty,
                loads[0].ReceivedDate.Date,
                loads.Count
            );
            var random = new Random(randomSeed);
            var shuffledLocations = candidateLocations
                .Where(location => !string.IsNullOrWhiteSpace(location))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(_ => random.Next())
                .ToList();

            for (var index = 0; index < loads.Count; index++)
            {
                var destinationLocation =
                    index < shuffledLocations.Count
                        ? shuffledLocations[index]
                        : $"V-C0-{index + 1:00}";

                assignedLocations[loads[index].LoadID] = destinationLocation;
            }

            return assignedLocations;
        }

        private string ResolveMockCurrentLocation(Model_ReceivingLoad load)
        {
            var catalog = _mockDataCatalog.GetCatalog();
            var normalizedPoNumber = NormalizeMockPoNumber(load.PoNumber);
            var normalizedPartId = load.PartID?.Trim().ToUpperInvariant() ?? string.Empty;
            var normalizedLineNumber = load.PoLineNumber?.Trim() ?? string.Empty;

            var matchedPurchaseOrderPart = catalog
                .PurchaseOrders.Where(po =>
                    string.Equals(
                        po.PONumber,
                        normalizedPoNumber,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .SelectMany(po => po.Parts)
                .FirstOrDefault(part =>
                    string.Equals(part.PartID, normalizedPartId, StringComparison.OrdinalIgnoreCase)
                    && (
                        string.IsNullOrWhiteSpace(normalizedLineNumber)
                        || string.Equals(
                            part.POLineNumber,
                            normalizedLineNumber,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );

            if (!string.IsNullOrWhiteSpace(matchedPurchaseOrderPart?.DefaultLocationId))
            {
                return NormalizeLocation(matchedPurchaseOrderPart.DefaultLocationId);
            }

            var matchedPart = catalog.Parts.FirstOrDefault(part =>
                string.Equals(part.PartID, normalizedPartId, StringComparison.OrdinalIgnoreCase)
            );

            return !string.IsNullOrWhiteSpace(matchedPart?.DefaultLocationId)
                ? NormalizeLocation(matchedPart.DefaultLocationId)
                : NormalizeLocation(load.InitialLocation);
        }

        private static string NormalizeMockPoNumber(string? poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
            {
                return string.Empty;
            }

            var trimmed = poNumber.Trim().ToUpperInvariant();
            var match = MockPoPattern.Match(trimmed);
            if (!match.Success)
            {
                return trimmed;
            }

            var digits = match.Groups["digits"].Value;
            var suffix = match.Groups["suffix"].Value.ToUpperInvariant();
            return $"PO-{digits.PadLeft(6, '0')}{suffix}";
        }
    }
}
