using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step2;

/// <summary>
/// Manages the editable DataGrid for load details entry in Wizard Step 2.
/// Handles row-level validation, cell edits, and auto-calculation of Weight Per Package.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_LoadDetailsGrid : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loads = new();

    [ObservableProperty]
    private Model_Receiving_DataTransferObjects_LoadGridRow? _selectedLoad;

    [ObservableProperty]
    private int _validLoadCount = 0;

    [ObservableProperty]
    private int _invalidLoadCount = 0;

    [ObservableProperty]
    private Dictionary<int, List<string>> _loadErrors = new();

    [ObservableProperty]
    private bool _allLoadsValid = false;

    [ObservableProperty]
    private string _validationSummary = string.Empty;

    [ObservableProperty]
    private string _partType = string.Empty;

    #endregion

    #region Constructor

    public ViewModel_Receiving_Wizard_Display_LoadDetailsGrid(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Load Details Grid ViewModel initialized");
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ValidateAllLoads()
    {
        try
        {
            LoadErrors.Clear();
            ValidLoadCount = 0;
            InvalidLoadCount = 0;

            foreach (var load in Loads)
            {
                ValidateLoad(load);
                
                if (load.HasErrors)
                    InvalidLoadCount++;
                else
                    ValidLoadCount++;
            }

            AllLoadsValid = InvalidLoadCount == 0 && ValidLoadCount > 0;
            
            ValidationSummary = AllLoadsValid
                ? $"All {ValidLoadCount} loads are valid"
                : $"{ValidLoadCount} valid, {InvalidLoadCount} invalid";

            _logger.LogInfo($"Load validation complete: {ValidationSummary}");
            StatusMessage = ValidationSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating loads: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ValidateLoad(Model_Receiving_DataTransferObjects_LoadGridRow load)
    {
        if (load == null) return;

        var errors = new List<string>();

        if (!load.Weight.HasValue || load.Weight <= 0)
            errors.Add("Weight is required and must be positive");

        if (!load.Quantity.HasValue || load.Quantity <= 0)
            errors.Add("Quantity is required and must be positive");

        if (string.IsNullOrWhiteSpace(load.HeatLot) && RequiresHeatLot(PartType))
            errors.Add("Heat/Lot is required for this part type");

        if (string.IsNullOrWhiteSpace(load.PackageType))
            errors.Add("Package Type is required");

        if (!load.PackagesPerLoad.HasValue || load.PackagesPerLoad <= 0)
            errors.Add("Packages Per Load is required and must be positive");

        if (string.IsNullOrWhiteSpace(load.ReceivingLocation))
            errors.Add("Receiving Location is required");

        LoadErrors[load.LoadNumber] = errors;
        load.HasErrors = errors.Count > 0;
        load.ErrorMessage = string.Join("; ", errors);

        if (load.HasErrors)
        {
            _logger.LogWarning($"Load {load.LoadNumber} validation failed: {load.ErrorMessage}");
        }
    }

    #endregion

    #region Public Methods

    public void OnCellEditEnded(Model_Receiving_DataTransferObjects_LoadGridRow load, string propertyName)
    {
        if (load == null) return;

        try
        {
            if (propertyName == nameof(load.Weight) || propertyName == nameof(load.PackagesPerLoad))
            {
                if (load.Weight.HasValue && load.PackagesPerLoad.HasValue && load.PackagesPerLoad > 0)
                {
                    load.WeightPerPackage = Math.Round(load.Weight.Value / load.PackagesPerLoad.Value, 2);
                    _logger.LogInfo($"Load {load.LoadNumber}: Weight Per Package = {load.WeightPerPackage}");
                }
            }

            ValidateLoad(load);
            ValidateAllLoads();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling cell edit: {ex.Message}");
        }
    }

    public void SetLoads(ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> loads)
    {
        Loads = loads;
        ValidateAllLoads();
        _logger.LogInfo($"Load collection set: {Loads.Count} loads");
    }

    public void SetPartType(string partType)
    {
        PartType = partType ?? string.Empty;
        _logger.LogInfo($"Part type set: {PartType}");
    }

    #endregion

    #region Helper Methods

    private static bool RequiresHeatLot(string partType)
    {
        var heatLotRequiredTypes = new[] { "RAW", "CAST", "FORGE", "BAR", "PLATE" };
        return heatLotRequiredTypes.Contains(partType?.ToUpperInvariant() ?? string.Empty);
    }

    #endregion
}
