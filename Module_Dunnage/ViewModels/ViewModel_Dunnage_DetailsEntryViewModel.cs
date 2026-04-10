using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Details Entry
/// </summary>
public partial class ViewModel_Dunnage_DetailsEntry : ViewModel_Shared_Base, IResettableViewModel
{
    private const string SettingsCategory = "Dunnage";
    private const string WarehouseCode = "002";
    private const string FallbackDefaultLocation = "RECV";

    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Help _helpService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Dispatcher _dispatcher;
    private readonly IService_ReceivingValidation _receivingValidation;
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_ViewModelRegistry _viewModelRegistry;

    public ViewModel_Dunnage_DetailsEntry(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Dispatcher dispatcher,
        IService_Help helpService,
        IService_ReceivingValidation receivingValidation,
        IService_InforVisual inforVisualService,
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager,
        IService_ViewModelRegistry viewModelRegistry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _helpService = helpService;
        _dunnageService = dunnageService;
        _dispatcher = dispatcher;
        _receivingValidation = receivingValidation;
        _inforVisualService = inforVisualService;
        _settingsCore = settingsCore;
        _sessionManager = sessionManager;
        _viewModelRegistry = viewModelRegistry;

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _viewModelRegistry.Register(this);
    }

    public void ResetToDefaults()
    {
        PoNumber = string.Empty;
        Location = string.Empty;
        SpecInputs = new ObservableCollection<Model_SpecInput>();
        TextSpecs = new ObservableCollection<Model_SpecInput>();
        NumberSpecs = new ObservableCollection<Model_SpecInput>();
        BooleanSpecs = new ObservableCollection<Model_SpecInput>();
        ChoiceSpecs = new ObservableCollection<Model_SpecInput>();
        HasTextSpecs = false;
        HasNumberSpecs = false;
        HasBooleanSpecs = false;
        HasChoiceSpecs = false;
        IsInventoryNotificationVisible = false;
        InventoryNotificationMessage = string.Empty;
        InventoryMethod = "Adjust In";
        StatusMessage = string.Empty;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.DetailsEntry)
        {
            _dispatcher.TryEnqueue(async () =>
            {
                await LoadSpecsForSelectedPartAsync();
            });
        }
    }

    #region Observable Properties

    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_SpecInput> _specInputs = new();

    [ObservableProperty]
    private ObservableCollection<Model_SpecInput> _textSpecs = new();

    [ObservableProperty]
    private ObservableCollection<Model_SpecInput> _numberSpecs = new();

    [ObservableProperty]
    private ObservableCollection<Model_SpecInput> _booleanSpecs = new();

    [ObservableProperty]
    private ObservableCollection<Model_SpecInput> _choiceSpecs = new();

    [ObservableProperty]
    private bool _hasTextSpecs = false;

    [ObservableProperty]
    private bool _hasNumberSpecs = false;

    [ObservableProperty]
    private bool _hasBooleanSpecs = false;

    [ObservableProperty]
    private bool _hasChoiceSpecs = false;

    [ObservableProperty]
    private bool _isInventoryNotificationVisible = false;

    [ObservableProperty]
    private string _inventoryNotificationMessage = string.Empty;

    [ObservableProperty]
    private string _inventoryMethod = "Adjust In";

    #endregion

    #region Initialization

    /// <summary>
    /// Load spec inputs for the selected part's type
    /// </summary>
    public async Task LoadSpecsForSelectedPartAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading part details...";

            await InitializeStepStateAsync();

            var selectedTypeId = _workflowService.CurrentSession.SelectedTypeId;
            if (selectedTypeId <= 0)
            {
                _logger.LogError("No type selected in workflow session", null, "DetailsEntry");
                SpecInputs.Clear();
                return;
            }

            _logger.LogInfo($"Loading specs for type ID: {selectedTypeId}", "DetailsEntry");

            // Fetch specs from dunnage_specs table (NOT from SpecsJson field which doesn't exist)
            var specsResult = await _dunnageService.GetSpecsForTypeAsync(selectedTypeId);
            var specs =
                specsResult.IsSuccess && specsResult.Data != null
                    ? specsResult.Data
                    : new List<Model_DunnageSpec>();
            _logger.LogInfo(
                $"Loaded {specs.Count} configured type specs from database",
                "DetailsEntry"
            );

            // Get the selected part's spec values for defaults
            var selectedPart = _workflowService.CurrentSession.SelectedPart;
            var partSpecValues = selectedPart?.SpecValuesDict;
            var partSpecificDefinitions =
                selectedPart?.PartSpecificSpecDefinitions
                ?? new Dictionary<string, SpecDefinition>();

            // Create spec inputs from database specs
            var specInputs = new List<Model_SpecInput>();
            var textSpecs = new List<Model_SpecInput>();
            var numberSpecs = new List<Model_SpecInput>();
            var booleanSpecs = new List<Model_SpecInput>();
            var choiceSpecs = new List<Model_SpecInput>();

            var createdSpecNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var spec in specs)
            {
                var definition = ParseSpecDefinition(spec.SpecValue);
                object? defaultValue = null;
                partSpecValues?.TryGetValue(spec.SpecKey, out defaultValue);

                var input = CreateSpecInput(spec.SpecKey, definition, defaultValue);

                specInputs.Add(input);
                createdSpecNames.Add(spec.SpecKey);

                _logger.LogInfo(
                    $"Processing configured spec: {spec.SpecKey}, Type: {input.SpecType}",
                    "DetailsEntry"
                );

                AddInputToCollection(input, textSpecs, numberSpecs, booleanSpecs, choiceSpecs);
            }

            foreach (var definition in partSpecificDefinitions.OrderBy(item => item.Key))
            {
                if (!createdSpecNames.Add(definition.Key))
                {
                    continue;
                }

                var input = CreateSpecInput(
                    definition.Key,
                    definition.Value,
                    Helper_Dunnage_PartSpecs.GetDefaultRuntimeValue(definition.Value)
                );

                specInputs.Add(input);
                _logger.LogInfo(
                    $"Processing part-specific runtime spec: {definition.Key}, Type: {input.SpecType}",
                    "DetailsEntry"
                );
                AddInputToCollection(input, textSpecs, numberSpecs, booleanSpecs, choiceSpecs);
            }

            SpecInputs = new ObservableCollection<Model_SpecInput>(specInputs);
            TextSpecs = new ObservableCollection<Model_SpecInput>(textSpecs);
            NumberSpecs = new ObservableCollection<Model_SpecInput>(numberSpecs);
            BooleanSpecs = new ObservableCollection<Model_SpecInput>(booleanSpecs);
            ChoiceSpecs = new ObservableCollection<Model_SpecInput>(choiceSpecs);

            // Update visibility flags
            HasTextSpecs = TextSpecs.Count > 0;
            HasNumberSpecs = NumberSpecs.Count > 0;
            HasBooleanSpecs = BooleanSpecs.Count > 0;
            HasChoiceSpecs = ChoiceSpecs.Count > 0;

            _logger.LogInfo(
                $"Created {SpecInputs.Count} spec input controls (Text: {TextSpecs.Count}, Number: {NumberSpecs.Count}, Boolean: {BooleanSpecs.Count}, Choices: {ChoiceSpecs.Count})",
                "DetailsEntry"
            );

            // Check if part is inventoried
            if (selectedPart != null)
            {
                var isInventoried = await _dunnageService.IsPartInventoriedAsync(
                    selectedPart.PartId
                );
                if (isInventoried)
                {
                    IsInventoryNotificationVisible = true;

                    var inventoryDetails = await _dunnageService.GetInventoryDetailsAsync(
                        selectedPart.PartId
                    );
                    if (
                        inventoryDetails.IsSuccess
                        && inventoryDetails.Data is not null
                        && string.IsNullOrWhiteSpace(inventoryDetails.Data.InventoryMethod) is false
                    )
                    {
                        InventoryMethod = inventoryDetails.Data.InventoryMethod;
                        _workflowService.CurrentSession.InventoryMethod = InventoryMethod;
                    }

                    UpdateInventoryMessage();
                }
                else
                {
                    _workflowService.CurrentSession.InventoryMethod = "Not Inventoried";
                    InventoryMethod = "Not Inventoried";
                }
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading specs",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Spec Parsing Helpers

    private static SpecDefinition ParseSpecDefinition(string specValue)
    {
        try
        {
            return JsonSerializer.Deserialize<SpecDefinition>(specValue) ?? new SpecDefinition();
        }
        catch (JsonException)
        {
            return new SpecDefinition();
        }
    }

    private static Model_SpecInput CreateSpecInput(
        string specName,
        SpecDefinition definition,
        object? defaultValue
    )
    {
        var normalizedType = NormalizeSpecType(definition.DataType);

        return new Model_SpecInput
        {
            SpecName = specName,
            SpecType = normalizedType,
            Unit = string.IsNullOrWhiteSpace(definition.Unit) ? null : definition.Unit,
            IsRequired = definition.Required,
            Value = defaultValue,
            Choices = definition.Choices?.ToList() ?? new List<string>(),
        };
    }

    private static void AddInputToCollection(
        Model_SpecInput input,
        List<Model_SpecInput> textSpecs,
        List<Model_SpecInput> numberSpecs,
        List<Model_SpecInput> booleanSpecs,
        List<Model_SpecInput> choiceSpecs
    )
    {
        switch (input.SpecType)
        {
            case "boolean":
                booleanSpecs.Add(input);
                break;
            case "number":
                numberSpecs.Add(input);
                break;
            case "choices":
                choiceSpecs.Add(input);
                break;
            default:
                textSpecs.Add(input);
                break;
        }
    }

    private static string NormalizeSpecType(string? specType)
    {
        return specType?.Trim().ToLowerInvariant() switch
        {
            "number" => "number",
            "boolean" => "boolean",
            "choices" => "choices",
            _ => "text",
        };
    }

    #endregion

    #region Property Change Handlers

    partial void OnPoNumberChanged(string value)
    {
        // Keep the workflow session in sync so AdvanceToNextStepAsync always
        // reads the value the user has typed, not a stale empty string.
        _workflowService.CurrentSession.PONumber = value;

        if (!string.IsNullOrWhiteSpace(_workflowService.CurrentSession.InventoryMethod))
        {
            InventoryMethod = _workflowService.CurrentSession.InventoryMethod;
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
            InventoryMethod = "Adjust In";
        }
        else
        {
            InventoryMethod = "Receive In";
        }

        UpdateInventoryMessage();
    }

    partial void OnLocationChanged(string value)
    {
        _workflowService.CurrentSession.Location = value;
    }

    public async Task<Model_ReceivingValidationResult> ValidateLocationAsync()
    {
        var locationToValidate = string.IsNullOrWhiteSpace(Location)
            ? await GetDefaultLocationAsync()
            : Location.Trim();

        var validation = await _receivingValidation.ValidateLocationAsync(
            locationToValidate,
            WarehouseCode
        );
        if (validation.IsValid)
        {
            Location = locationToValidate;
        }

        return validation;
    }

    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetLocationSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(Location))
        {
            return Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>());
        }

        if (_receivingValidation.UseMockLocationList)
        {
            var normalizedSearch = NormalizeLocationForMatch(Location);
            var locationSuggestions = _receivingValidation
                .PresetLocations.Where(presetLocation =>
                {
                    var normalizedPreset = NormalizeLocationForMatch(presetLocation);
                    return normalizedPreset.Contains(
                        normalizedSearch,
                        StringComparison.OrdinalIgnoreCase
                    );
                })
                .OrderByDescending(presetLocation =>
                    presetLocation.StartsWith(Location.Trim(), StringComparison.OrdinalIgnoreCase)
                )
                .ThenBy(presetLocation => presetLocation, StringComparer.OrdinalIgnoreCase)
                .Select(presetLocation => new Model_FuzzySearchResult
                {
                    Key = presetLocation,
                    Label = presetLocation,
                    Detail = "Preset mock location",
                })
                .ToList();

            return Model_Dao_Result_Factory.Success(locationSuggestions);
        }

        var fuzzyResult = await _inforVisualService.FuzzySearchLocationsAsync(
            Location.Trim(),
            WarehouseCode
        );
        if (!fuzzyResult.IsSuccess || fuzzyResult.Data is null)
        {
            return fuzzyResult;
        }

        var suggestions = fuzzyResult
            .Data.Where(static result => string.IsNullOrWhiteSpace(result.Label) is false)
            .GroupBy(static result => result.Label.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToList();

        return Model_Dao_Result_Factory.Success(suggestions);
    }

    private void UpdateInventoryMessage()
    {
        if (IsInventoryNotificationVisible)
        {
            InventoryNotificationMessage =
                $"This part updates Visual inventory using '{InventoryMethod}'.";
        }
    }

    #endregion

    #region Validation

    private bool ValidateInputs()
    {
        // Check required specs
        foreach (var spec in SpecInputs.Where(s => s.IsRequired))
        {
            if (spec.Value == null || string.IsNullOrWhiteSpace(spec.Value.ToString()))
            {
                StatusMessage = $"Enter a value for {spec.SpecName}.";
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInfo("Navigating back to Quantity Entry", "DetailsEntry");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
    }

    public async Task<Model_WorkflowStepResult> SaveAndAdvanceAsync()
    {
        if (IsBusy)
        {
            return new Model_WorkflowStepResult
            {
                IsSuccess = false,
                ErrorMessage = "Please wait for the current step to finish.",
            };
        }

        try
        {
            IsBusy = true;
            _workflowService.SetNavigationLock(true);
            StatusMessage = "Saving your entry...";

            if (!ValidateInputs())
            {
                return new Model_WorkflowStepResult
                {
                    IsSuccess = false,
                    ErrorMessage = StatusMessage,
                };
            }

            // Set details in workflow session
            _workflowService.CurrentSession.PONumber = PoNumber;
            _workflowService.CurrentSession.Location = Location;

            // Convert spec inputs to dictionary
            var specValues = SpecInputs.ToDictionary(s => s.SpecName, s => s.Value ?? string.Empty);

            _workflowService.CurrentSession.SpecValues = specValues;

            var advanceResult = await _workflowService.AdvanceToNextStepAsync();
            if (!advanceResult.IsSuccess)
            {
                StatusMessage = advanceResult.ErrorMessage;
                return advanceResult;
            }

            _logger.LogInfo("Details saved, navigating to Review", "DetailsEntry");
            return advanceResult;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "There was a problem saving this entry.",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );

            return new Model_WorkflowStepResult
            {
                IsSuccess = false,
                ErrorMessage = "There was a problem saving this entry.",
            };
        }
        finally
        {
            _workflowService.SetNavigationLock(false);
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoNextAsync()
    {
        await SaveAndAdvanceAsync();
    }

    /// <summary>
    /// Shows contextual help for details entry.
    /// </summary>
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.DetailsEntry");
    }

    #endregion

    #region Help Content Helpers

    /// <summary>
    /// Gets a tooltip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    /// <summary>
    /// Gets a placeholder by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

    /// <summary>
    /// Gets a tip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTip(string key) => _helpService.GetTip(key);

    #endregion

    private async Task InitializeStepStateAsync()
    {
        PoNumber = _workflowService.CurrentSession.PONumber?.Trim() ?? string.Empty;

        var currentLocation = _workflowService.CurrentSession.Location?.Trim();
        Location = string.IsNullOrWhiteSpace(currentLocation)
            ? await GetDefaultLocationAsync()
            : currentLocation;

        if (!string.IsNullOrWhiteSpace(_workflowService.CurrentSession.InventoryMethod))
        {
            InventoryMethod = _workflowService.CurrentSession.InventoryMethod;
        }
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
                "DetailsEntry"
            );
        }

        return FallbackDefaultLocation;
    }

    private static string NormalizeLocationForMatch(string? location)
    {
        return new string(
            (location ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray()
        );
    }
}
