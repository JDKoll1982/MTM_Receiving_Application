using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Events;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    public class Service_DunnageWorkflow : IService_DunnageWorkflow, IDisposable
    {
        private const string SettingsCategory = "Dunnage";
        private const string WarehouseCode = "002";
        private const string FallbackDefaultLocation = "RECV";

        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_SettingsCoreFacade _settingsCore;
        private readonly IService_ReceivingValidation _receivingValidation;
        private readonly List<Model_DunnageLoad> _currentEntryLoads = new();
        private readonly WeakEventSource _stepChanged = new();
        private readonly WeakEventSource<string> _statusMessageRaised = new();
        private readonly WeakEventSource _labelDataCleared = new();

        public Enum_DunnageWorkflowStep CurrentStep { get; private set; }
        public Model_DunnageSession CurrentSession { get; private set; } = new();

        public int NumberOfLoads
        {
            get => CurrentSession.NumberOfLoads <= 0 ? 1 : CurrentSession.NumberOfLoads;
            set => CurrentSession.NumberOfLoads = Math.Max(1, value);
        }

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

        public event EventHandler? LabelDataCleared
        {
            add => _labelDataCleared.Subscribe(value);
            remove => _labelDataCleared.Unsubscribe(value);
        }

        public Service_DunnageWorkflow(
            IService_MySQL_Dunnage dunnageService,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_ErrorHandler errorHandler,
            IService_ViewModelRegistry viewModelRegistry,
            IService_SettingsCoreFacade settingsCore,
            IService_ReceivingValidation receivingValidation
        )
        {
            _dunnageService = dunnageService;
            _sessionManager = sessionManager;
            _logger = logger;
            _errorHandler = errorHandler;
            _viewModelRegistry = viewModelRegistry;
            _settingsCore = settingsCore;
            _receivingValidation = receivingValidation;

            _sessionManager.SessionTimedOut += OnSessionTimedOut;
        }

        private void OnSessionTimedOut(object? sender, object e)
        {
            ClearSession();
        }

        public Task<bool> StartWorkflowAsync()
        {
            ClearSession();

            var currentUser = _sessionManager.CurrentSession?.User;
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultDunnageMode))
            {
                switch (currentUser.DefaultDunnageMode.ToLower())
                {
                    case "guided":
                        GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
                        _statusMessageRaised.Raise(this, "Starting Guided Wizard mode");
                        break;
                    case "manual":
                        GoToStep(Enum_DunnageWorkflowStep.ManualEntry);
                        _statusMessageRaised.Raise(this, "Starting Manual Entry mode");
                        break;
                    case "edit":
                        GoToStep(Enum_DunnageWorkflowStep.EditMode);
                        _statusMessageRaised.Raise(this, "Starting Edit mode");
                        break;
                    default:
                        GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
                        _statusMessageRaised.Raise(this, "Workflow started");
                        break;
                }
            }
            else
            {
                GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
                _statusMessageRaised.Raise(this, "Workflow started");
            }

            return Task.FromResult(true);
        }

        public async Task<Model_WorkflowStepResult> AdvanceToNextStepAsync()
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
                            return new Model_WorkflowStepResult
                            {
                                IsSuccess = false,
                                ErrorMessage = "Please select a dunnage type.",
                            };
                        }

                        GoToStep(Enum_DunnageWorkflowStep.PartSelection);
                        break;

                    case Enum_DunnageWorkflowStep.PartSelection:
                        if (CurrentSession.SelectedPart == null)
                        {
                            return new Model_WorkflowStepResult
                            {
                                IsSuccess = false,
                                ErrorMessage = "Please select a part.",
                            };
                        }

                        GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
                        break;

                    case Enum_DunnageWorkflowStep.QuantityEntry:
                        EnsureLoadQuantitySlots();
                        if (NumberOfLoads < 1)
                        {
                            return new Model_WorkflowStepResult
                            {
                                IsSuccess = false,
                                ErrorMessage = "Number of loads must be at least 1.",
                            };
                        }

                        for (var index = 0; index < CurrentSession.LoadQuantities.Count; index++)
                        {
                            if (CurrentSession.LoadQuantities[index] <= 0)
                            {
                                return new Model_WorkflowStepResult
                                {
                                    IsSuccess = false,
                                    ErrorMessage =
                                        $"Load {index + 1}: Quantity must be greater than zero.",
                                };
                            }
                        }

                        GenerateCurrentEntryLoads();
                        GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
                        break;

                    case Enum_DunnageWorkflowStep.DetailsEntry:
                        var locationValidation = await EnsureDefaultAndValidatedLocationAsync();
                        if (!locationValidation.IsValid)
                        {
                            return new Model_WorkflowStepResult
                            {
                                IsSuccess = false,
                                ErrorMessage = locationValidation.Message,
                            };
                        }

                        ApplySessionDetailsToCurrentEntryLoads();
                        GoToStep(Enum_DunnageWorkflowStep.Review);
                        break;

                    case Enum_DunnageWorkflowStep.Review:
                        return new Model_WorkflowStepResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Already at Review step. Use Save to finish.",
                        };
                }

                return new Model_WorkflowStepResult { IsSuccess = true, TargetStep = CurrentStep };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync(
                    "Error advancing step",
                    Enum_ErrorSeverity.Error,
                    ex,
                    true
                );
                return new Model_WorkflowStepResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                };
            }
        }

        public void GoToStep(Enum_DunnageWorkflowStep step)
        {
            if (
                step == Enum_DunnageWorkflowStep.TypeSelection
                && CurrentStep == Enum_DunnageWorkflowStep.Review
            )
            {
                _currentEntryLoads.Clear();
            }

            if (
                step == Enum_DunnageWorkflowStep.TypeSelection
                && CurrentStep != Enum_DunnageWorkflowStep.ModeSelection
                && CurrentStep != Enum_DunnageWorkflowStep.PartSelection
                && CurrentStep != Enum_DunnageWorkflowStep.QuantityEntry
                && CurrentStep != Enum_DunnageWorkflowStep.DetailsEntry
            )
            {
                ClearSession();
            }

            CurrentStep = step;
            _stepChanged.Raise(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _sessionManager.SessionTimedOut -= OnSessionTimedOut;
        }

        public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
        {
            try
            {
                if (CurrentSession.Loads.Count == 0)
                {
                    GenerateCurrentEntryLoads();
                    ApplySessionDetailsToCurrentEntryLoads();
                }

                var loads = new List<Model_DunnageLoad>(CurrentSession.Loads);
                if (loads.Count == 0)
                {
                    return new Model_SaveResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "No data to save.",
                    };
                }

                var dbResult = await _dunnageService.SaveLoadsAsync(loads);

                return new Model_SaveResult
                {
                    IsSuccess = dbResult.IsSuccess,
                    ErrorMessage = dbResult.ErrorMessage,
                    RecordsSaved = dbResult.IsSuccess ? loads.Count : 0,
                };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync(
                    "Error saving to database",
                    Enum_ErrorSeverity.Error,
                    ex,
                    true
                );
                return new Model_SaveResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<Model_SaveResult> SaveSessionAsync()
        {
            return await SaveToDatabaseOnlyAsync();
        }

        public void ClearSession()
        {
            _currentEntryLoads.Clear();
            CurrentSession = new Model_DunnageSession();
            NumberOfLoads = 1;
            _viewModelRegistry.ClearAllInputs();
            _statusMessageRaised.Raise(this, "Session cleared");
        }

        public async Task<Model_Dao_Result<int>> ClearLabelDataAsync()
        {
            try
            {
                var result = await _dunnageService.ClearLabelDataAsync();
                if (result.IsSuccess)
                {
                    _statusMessageRaised.Raise(
                        this,
                        $"Label data cleared — {result.Data} row(s) archived"
                    );
                    _labelDataCleared.Raise(this, EventArgs.Empty);
                }
                else
                {
                    _statusMessageRaised.Raise(
                        this,
                        $"Clear Label Data failed: {result.ErrorMessage}"
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync(
                    "Error clearing label data",
                    Enum_ErrorSeverity.Error,
                    ex,
                    true
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error clearing label data: {ex.Message}"
                );
            }
        }

        public void AddCurrentLoadToSession()
        {
            try
            {
                GenerateCurrentEntryLoads();
                ApplySessionDetailsToCurrentEntryLoads();

                if (_currentEntryLoads.Count > 0)
                {
                    _logger.LogInfo(
                        $"Added {_currentEntryLoads.Count} load(s) to session for part {CurrentSession.SelectedPart?.PartId}",
                        "DunnageWorkflow"
                    );
                    _statusMessageRaised.Raise(this, "Added loads to session");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleErrorAsync(
                    "Error adding load to session",
                    Enum_ErrorSeverity.Medium,
                    ex,
                    false
                );
            }
        }

        private void EnsureLoadQuantitySlots()
        {
            while (CurrentSession.LoadQuantities.Count < NumberOfLoads)
            {
                var defaultQuantity =
                    CurrentSession.LoadQuantities.Count == 0 && CurrentSession.Quantity > 0
                        ? CurrentSession.Quantity
                        : 0m;
                CurrentSession.LoadQuantities.Add(defaultQuantity);
            }

            while (CurrentSession.LoadQuantities.Count > NumberOfLoads)
            {
                CurrentSession.LoadQuantities.RemoveAt(CurrentSession.LoadQuantities.Count - 1);
            }

            CurrentSession.Quantity = CurrentSession.LoadQuantities.FirstOrDefault();
        }

        private void GenerateCurrentEntryLoads()
        {
            if (CurrentSession.SelectedPart is null || NumberOfLoads < 1)
            {
                return;
            }

            EnsureLoadQuantitySlots();

            foreach (var load in _currentEntryLoads)
            {
                CurrentSession.Loads.Remove(load);
            }

            _currentEntryLoads.Clear();

            var existingReviewedCount = CurrentSession.Loads.Count;
            for (var index = 0; index < NumberOfLoads; index++)
            {
                var load = CreateLoadFromCurrentSession(
                    existingReviewedCount + index + 1,
                    CurrentSession.LoadQuantities[index]
                );
                CurrentSession.Loads.Add(load);
                _currentEntryLoads.Add(load);
            }
        }

        private void ApplySessionDetailsToCurrentEntryLoads()
        {
            if (_currentEntryLoads.Count == 0)
            {
                GenerateCurrentEntryLoads();
            }

            var location = string.IsNullOrWhiteSpace(CurrentSession.Location)
                ? FallbackDefaultLocation
                : CurrentSession.Location.Trim();
            var poNumber = string.IsNullOrWhiteSpace(CurrentSession.PONumber)
                ? "Nothing Entered"
                : CurrentSession.PONumber;
            var specs = CurrentSession.SpecValues ?? new Dictionary<string, object>();
            var createdBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";
            var inventoryMethod = string.IsNullOrWhiteSpace(CurrentSession.InventoryMethod)
                ? string.IsNullOrWhiteSpace(CurrentSession.PONumber)
                    ? "Adjust In"
                    : "Receive In"
                : CurrentSession.InventoryMethod.Trim();

            foreach (var load in _currentEntryLoads)
            {
                load.PoNumber = poNumber;
                load.Location = location;
                load.HomeLocation = CurrentSession.SelectedPart?.HomeLocation;
                load.TypeName = CurrentSession.SelectedTypeName;
                load.TypeIcon = CurrentSession.SelectedType?.Icon ?? "Help";
                load.DunnageType = CurrentSession.SelectedTypeName;
                load.TypeId = CurrentSession.SelectedTypeId;
                load.Specs = new Dictionary<string, object>(specs);
                load.SpecValues = new Dictionary<string, object>(specs);
                load.InventoryMethod = inventoryMethod;
                load.CreatedBy = createdBy;
                load.ReceivedDate = DateTime.Now;
            }
        }

        private Model_DunnageLoad CreateLoadFromCurrentSession(int loadNumber, decimal quantity)
        {
            return new Model_DunnageLoad
            {
                LoadUuid = Guid.NewGuid(),
                PartId = CurrentSession.SelectedPart?.PartId ?? "Unknown",
                Quantity = quantity,
                PoNumber = CurrentSession.PONumber,
                Location = CurrentSession.Location,
                HomeLocation = CurrentSession.SelectedPart?.HomeLocation,
                DunnageType = CurrentSession.SelectedTypeName,
                TypeName = CurrentSession.SelectedTypeName,
                TypeIcon = CurrentSession.SelectedType?.Icon ?? "Help",
                TypeId = CurrentSession.SelectedTypeId,
                Specs = CurrentSession.SpecValues ?? new Dictionary<string, object>(),
                SpecValues = CurrentSession.SpecValues is null
                    ? null
                    : new Dictionary<string, object>(CurrentSession.SpecValues),
                ReceivedDate = DateTime.Now,
                CreatedBy = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown",
                LoadNumber = loadNumber,
            };
        }

        private async Task<Model_ReceivingValidationResult> EnsureDefaultAndValidatedLocationAsync()
        {
            var resolvedLocation = string.IsNullOrWhiteSpace(CurrentSession.Location)
                ? await GetDefaultLocationAsync()
                : CurrentSession.Location.Trim();

            var validation = await _receivingValidation.ValidateLocationAsync(
                resolvedLocation,
                WarehouseCode
            );
            if (!validation.IsValid)
            {
                return validation;
            }

            CurrentSession.Location = resolvedLocation;
            return Model_ReceivingValidationResult.Success();
        }

        private async Task<string> GetDefaultLocationAsync()
        {
            try
            {
                var result = await _settingsCore.GetSettingAsync(
                    SettingsCategory,
                    DunnageSettingsKeys.UserPreferences.DefaultLocation,
                    _sessionManager.CurrentSession?.User?.EmployeeNumber
                );

                if (result.IsSuccess && result.Data is not null)
                {
                    var configuredLocation = result.Data.Value?.Trim();
                    if (!string.IsNullOrWhiteSpace(configuredLocation))
                    {
                        return configuredLocation;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Failed to load default Dunnage location. Falling back to {FallbackDefaultLocation}. Error: {ex.Message}",
                    "DunnageWorkflow"
                );
            }

            return FallbackDefaultLocation;
        }
    }
}
