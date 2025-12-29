using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Details Entry
/// </summary>
public partial class Dunnage_DetailsEntryViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Dispatcher _dispatcher;

    public Dunnage_DetailsEntryViewModel(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Dispatcher dispatcher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
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

            var selectedType = _workflowService.CurrentSession.SelectedType;
            if (selectedType == null)
            {
                _logger.LogError("No type selected in workflow session", null, "DetailsEntry");
                return;
            }

            // Deserialize SpecsJson
            var specs = DeserializeSpecsJson(selectedType.SpecsJson);

            // Create spec inputs
            SpecInputs.Clear();
            foreach (var spec in specs)
            {
                var input = new Model_SpecInput
                {
                    SpecName = spec.Key,
                    SpecType = DetermineSpecType(spec.Value),
                    Unit = ExtractUnit(spec.Value),
                    IsRequired = IsSpecRequired(spec.Value),
                    Value = null
                };

                SpecInputs.Add(input);
            }

            _logger.LogInfo($"Loaded {SpecInputs.Count} spec inputs", "DetailsEntry");

            // Check if part is inventoried
            var selectedPart = _workflowService.CurrentSession.SelectedPart;
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

    private Dictionary<string, object> DeserializeSpecsJson(string specsJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(specsJson))
            {
                return new Dictionary<string, object>();
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(specsJson)
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to deserialize specs JSON: {ex.Message}", ex, "DetailsEntry");
            return new Dictionary<string, object>();
        }
    }

    private string DetermineSpecType(object specValue)
    {
        if (specValue == null)
        {
            return "text";
        }

        var specStr = specValue.ToString()?.ToLowerInvariant() ?? string.Empty;

        // Check for common patterns
        if (specStr.Contains("number") || specStr.Contains("numeric") || specStr.Contains("mm") || specStr.Contains("inches"))
        {
            return "number";
        }

        if (specStr.Contains("bool") || specStr.Contains("yes/no") || specStr.Contains("true/false"))
        {
            return "boolean";
        }

        return "text";
    }

    private string? ExtractUnit(object specValue)
    {
        if (specValue == null)
        {
            return null;
        }

        var specStr = specValue.ToString() ?? string.Empty;

        // Extract units like mm, inches, etc.
        if (specStr.Contains("mm", StringComparison.OrdinalIgnoreCase))
        {
            return "mm";
        }

        if (specStr.Contains("inch", StringComparison.OrdinalIgnoreCase))
        {
            return "inches";
        }

        if (specStr.Contains("cm", StringComparison.OrdinalIgnoreCase))
        {
            return "cm";
        }

        return null;
    }

    private bool IsSpecRequired(object specValue)
    {
        if (specValue == null)
        {
            return false;
        }

        var specStr = specValue.ToString()?.ToLowerInvariant() ?? string.Empty;
        return specStr.Contains("required");
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
            InventoryNotificationMessage = $"⚠️ This part requires inventory adjustment in Visual. Method: {InventoryMethod}";
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

    #endregion
}
