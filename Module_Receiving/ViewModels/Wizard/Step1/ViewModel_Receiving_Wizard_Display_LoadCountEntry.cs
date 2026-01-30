using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

/// <summary>
/// Manages Load Count entry and load grid row generation for Wizard Step 1.
/// Validates input range (1-99) and generates initial load data structures.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_LoadCountEntry : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    /// <summary>
    /// Number of loads to receive (1-99).
    /// </summary>
    [ObservableProperty]
    private int _loadCount = 1;

    /// <summary>
    /// Minimum allowed load count.
    /// </summary>
    [ObservableProperty]
    private int _minLoadCount = 1;

    /// <summary>
    /// Maximum allowed load count.
    /// </summary>
    [ObservableProperty]
    private int _maxLoadCount = 99;

    /// <summary>
    /// Indicates if the load count is within valid range.
    /// </summary>
    [ObservableProperty]
    private bool _isLoadCountValid = false;

    /// <summary>
    /// Validation message for load count (empty if valid).
    /// </summary>
    [ObservableProperty]
    private string _loadCountValidationMessage = string.Empty;

    /// <summary>
    /// Collection of generated load rows (populated after count is set).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loads = new();

    /// <summary>
    /// PO Number to apply to all load rows.
    /// </summary>
    [ObservableProperty]
    private string _poNumber = string.Empty;

    /// <summary>
    /// Part Number to apply to all load rows.
    /// </summary>
    [ObservableProperty]
    private string _partNumber = string.Empty;

    /// <summary>
    /// Part Type name to apply to all load rows.
    /// </summary>
    [ObservableProperty]
    private string _partType = string.Empty;

    /// <summary>
    /// Default Package Type to apply to all load rows.
    /// </summary>
    [ObservableProperty]
    private string _defaultPackageType = "Skid";

    /// <summary>
    /// Default Receiving Location to apply to all load rows.
    /// </summary>
    [ObservableProperty]
    private string _defaultReceivingLocation = "RECV";

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Load Count Entry ViewModel.
    /// </summary>
    public ViewModel_Receiving_Wizard_Display_LoadCountEntry(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Load Count Entry ViewModel initialized");

        // Validate initial load count
        ValidateLoadCount();
    }

    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Validates load count whenever it changes.
    /// </summary>
    partial void OnLoadCountChanged(int value)
    {
        ValidateLoadCount();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Increments the load count by 1 (max 99).
    /// </summary>
    [RelayCommand]
    private void IncrementLoadCount()
    {
        if (LoadCount < MaxLoadCount)
        {
            LoadCount++;
            _logger.LogInfo($"Load count incremented to {LoadCount}");
        }
    }

    /// <summary>
    /// Decrements the load count by 1 (min 1).
    /// </summary>
    [RelayCommand]
    private void DecrementLoadCount()
    {
        if (LoadCount > MinLoadCount)
        {
            LoadCount--;
            _logger.LogInfo($"Load count decremented to {LoadCount}");
        }
    }

    /// <summary>
    /// Generates load grid rows based on the current load count and Step 1 data.
    /// </summary>
    [RelayCommand]
    private async Task GenerateLoadRowsAsync()
    {
        if (!IsLoadCountValid)
        {
            await _errorHandler.ShowUserErrorAsync(
                LoadCountValidationMessage,
                "Invalid Load Count",
                nameof(GenerateLoadRowsAsync));
            return;
        }

        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = $"Generating {LoadCount} load rows...";

            Loads.Clear();

            for (int i = 1; i <= LoadCount; i++)
            {
                var loadRow = new Model_Receiving_DataTransferObjects_LoadGridRow
                {
                    LoadNumber = i,
                    PONumber = PoNumber,
                    PartNumber = PartNumber,
                    PartType = PartType,
                    PackageType = DefaultPackageType,
                    ReceivingLocation = DefaultReceivingLocation,
                    
                    // Weight, Quantity, HeatLot, PackagesPerLoad remain null for user entry in Step 2
                    Weight = null,
                    Quantity = null,
                    HeatLot = string.Empty,
                    PackagesPerLoad = null,
                    WeightPerPackage = null,
                    
                    // Tracking fields
                    IsAutoFilled = false,
                    HasErrors = false,
                    ErrorMessage = string.Empty
                };

                Loads.Add(loadRow);
            }

            _logger.LogInfo($"Generated {LoadCount} load rows for PO: {PoNumber}, Part: {PartNumber}");
            StatusMessage = $"{LoadCount} loads ready for detail entry";

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(GenerateLoadRowsAsync),
                nameof(ViewModel_Receiving_Wizard_Display_LoadCountEntry));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validates the load count against min/max constraints.
    /// </summary>
    [RelayCommand]
    private void ValidateLoadCount()
    {
        IsLoadCountValid = LoadCount >= MinLoadCount && LoadCount <= MaxLoadCount;

        LoadCountValidationMessage = IsLoadCountValid
            ? string.Empty
            : $"Load count must be between {MinLoadCount} and {MaxLoadCount}";

        if (!IsLoadCountValid)
        {
            _logger.LogWarning($"Invalid load count: {LoadCount}. {LoadCountValidationMessage}");
        }
    }

    /// <summary>
    /// Resets load count to default value (1).
    /// </summary>
    [RelayCommand]
    private void ResetLoadCount()
    {
        LoadCount = MinLoadCount;
        Loads.Clear();
        _logger.LogInfo("Load count reset to minimum");
        StatusMessage = "Load count reset";
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets the context data from Step 1 (PO, Part, defaults) for load row generation.
    /// </summary>
    /// <param name="poNumber">PO Number from Step 1.</param>
    /// <param name="partNumber">Part Number from Step 1.</param>
    /// <param name="partType">Part Type from Step 1.</param>
    /// <param name="defaultPackageType">Default Package Type.</param>
    /// <param name="defaultReceivingLocation">Default Receiving Location.</param>
    public void SetContextData(
        string poNumber,
        string partNumber,
        string partType,
        string defaultPackageType,
        string defaultReceivingLocation)
    {
        PoNumber = poNumber ?? string.Empty;
        PartNumber = partNumber ?? string.Empty;
        PartType = partType ?? string.Empty;
        DefaultPackageType = defaultPackageType ?? "Skid";
        DefaultReceivingLocation = defaultReceivingLocation ?? "RECV";

        _logger.LogInfo($"Context data set: PO={PoNumber}, Part={PartNumber}, Type={PartType}");
    }

    #endregion
}
