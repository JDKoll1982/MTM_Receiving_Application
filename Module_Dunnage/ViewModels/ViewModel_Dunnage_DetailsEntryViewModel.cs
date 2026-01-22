using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Details Entry
/// </summary>
public partial class ViewModel_Dunnage_DetailsEntry : ViewModel_Shared_Base
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Help _helpService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Dispatcher _dispatcher;

    public ViewModel_Dunnage_DetailsEntry(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Dispatcher dispatcher,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _helpService = helpService;
        _dunnageService = dunnageService;
        _dispatcher = dispatcher;

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
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
    private bool _hasTextSpecs = false;

    [ObservableProperty]
    private bool _hasNumberSpecs = false;

    [ObservableProperty]
    private bool _hasBooleanSpecs = false;

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
            StatusMessage = "Loading spec inputs...";

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
            if (!specsResult.IsSuccess || specsResult.Data == null)
            {
                _logger.LogWarning($"No specs found for type {selectedTypeId}: {specsResult.ErrorMessage}", "DetailsEntry");
                SpecInputs.Clear();
                return;
            }

            var specs = specsResult.Data;
            _logger.LogInfo($"Loaded {specs.Count} specs from database", "DetailsEntry");

            // Get the selected part's spec values for defaults
            var selectedPart = _workflowService.CurrentSession.SelectedPart;
            Dictionary<string, object>? partSpecValues = null;
            if (selectedPart != null && !string.IsNullOrWhiteSpace(selectedPart.SpecValues))
            {
                try
                {
                    partSpecValues = JsonSerializer.Deserialize<Dictionary<string, object>>(selectedPart.SpecValues);
                    _logger.LogInfo($"Loaded spec values from part: {selectedPart.SpecValues}", "DetailsEntry");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse part spec values: {ex.Message}", ex, "DetailsEntry");
                }
            }

            // Create spec inputs from database specs
            SpecInputs.Clear();
            TextSpecs.Clear();
            NumberSpecs.Clear();
            BooleanSpecs.Clear();

            foreach (var spec in specs)
            {
                // Parse spec_value JSON to get type, unit, required
                var specValueDict = ParseSpecValue(spec.SpecValue);

                // Get default value from part if available
                string? defaultValue = null;
                if (partSpecValues?.ContainsKey(spec.SpecKey) == true)
                {
                    defaultValue = partSpecValues[spec.SpecKey]?.ToString();
                }

                var specType = specValueDict.ContainsKey("type") ? specValueDict["type"]?.ToString()?.ToLowerInvariant() ?? "text" : "text";

                // Convert default value to appropriate type
                object? typedValue = defaultValue;
                if (!string.IsNullOrWhiteSpace(defaultValue))
                {
                    try
                    {
                        if (specType == "number")
                        {
                            typedValue = double.Parse(defaultValue);
                        }
                        else if (specType == "boolean")
                        {
                            typedValue = bool.Parse(defaultValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to convert value '{defaultValue}' to type '{specType}': {ex.Message}", "DetailsEntry");
                        typedValue = defaultValue; // Keep as string if conversion fails
                    }
                }

                var input = new Model_SpecInput
                {
                    SpecName = spec.SpecKey,
                    SpecType = specType,
                    Unit = specValueDict.ContainsKey("unit") ? specValueDict["unit"]?.ToString() : null,
                    IsRequired = specValueDict.ContainsKey("required") && bool.Parse(specValueDict["required"]?.ToString() ?? "false"),
                    Value = typedValue
                };

                SpecInputs.Add(input);

                var typeFromDb = specValueDict.ContainsKey("type") ? specValueDict["type"]?.ToString() ?? "null" : "not found";
                _logger.LogInfo($"Processing spec: {spec.SpecKey}, Type from DB: {typeFromDb}, Normalized: {specType}", "DetailsEntry");

                // Add to type-specific collection
                if (specType == "boolean")
                {
                    BooleanSpecs.Add(input);
                    _logger.LogInfo($"Added {spec.SpecKey} to BooleanSpecs", "DetailsEntry");
                }
                else if (specType == "number")
                {
                    NumberSpecs.Add(input);
                    _logger.LogInfo($"Added {spec.SpecKey} to NumberSpecs", "DetailsEntry");
                }
                else
                {
                    TextSpecs.Add(input);
                    _logger.LogInfo($"Added {spec.SpecKey} to TextSpecs", "DetailsEntry");
                }
            }

            // Update visibility flags
            HasTextSpecs = TextSpecs.Count > 0;
            HasNumberSpecs = NumberSpecs.Count > 0;
            HasBooleanSpecs = BooleanSpecs.Count > 0;

            _logger.LogInfo($"Created {SpecInputs.Count} spec input controls (Text: {TextSpecs.Count}, Number: {NumberSpecs.Count}, Boolean: {BooleanSpecs.Count})", "DetailsEntry");

            // Check if part is inventoried
            if (selectedPart != null)
            {
                var isInventoried = await _dunnageService.IsPartInventoriedAsync(selectedPart.PartId);
                if (isInventoried)
                {
                    IsInventoryNotificationVisible = true;
                    UpdateInventoryMessage();
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

    private Dictionary<string, object> ParseSpecValue(string specValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(specValue))
            {
                return new Dictionary<string, object>();
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(specValue)
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to parse spec value JSON: {ex.Message}", ex, "DetailsEntry");
            return new Dictionary<string, object>();
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnPoNumberChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            InventoryMethod = "Adjust In";
        }
        else
        {
            InventoryMethod = "Receive In";
        }

        UpdateInventoryMessage();
    }

    private void UpdateInventoryMessage()
    {
        if (IsInventoryNotificationVisible)
        {
            InventoryNotificationMessage = $"This part requires inventory adjustment in Visual. Method: {InventoryMethod}";
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
                StatusMessage = $"Required field missing: {spec.SpecName}";
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

    [RelayCommand]
    private async Task GoNextAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Validating...";

            if (!ValidateInputs())
            {
                return;
            }

            // Set details in workflow session
            _workflowService.CurrentSession.PONumber = PoNumber;
            _workflowService.CurrentSession.Location = Location;

            // Convert spec inputs to dictionary
            var specValues = SpecInputs.ToDictionary(
                s => s.SpecName,
                s => s.Value ?? string.Empty
            );

            _workflowService.CurrentSession.SpecValues = specValues;

            _logger.LogInfo("Details saved, navigating to Review", "DetailsEntry");

            // Navigate to Review
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.Review);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving details",
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

    /// <summary>\n    /// Shows contextual help for details entry\n    /// </summary>
    /// <param name="key"></param>\n    [RelayCommand]\n    private async Task ShowHelpAsync()\n    {\n        await _helpService.ShowHelpAsync(\"Dunnage.DetailsEntry\");\n    }

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
}


