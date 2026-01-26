using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step3;

/// <summary>
/// Manages the review summary display for Wizard Step 3.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_ReviewSummary : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private string _partNumber = string.Empty;

    [ObservableProperty]
    private string _partType = string.Empty;

    [ObservableProperty]
    private int _totalLoads = 0;

    [ObservableProperty]
    private decimal _totalWeight = 0;

    [ObservableProperty]
    private int _totalQuantity = 0;

    [ObservableProperty]
    private int _totalPackages = 0;

    [ObservableProperty]
    private bool _hasQualityHold = false;

    [ObservableProperty]
    private string _qualityHoldMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> _loadsReadOnly = new();

    [ObservableProperty]
    private string _summaryText = string.Empty;

    [ObservableProperty]
    private bool _qualityHoldAcknowledged = false;

    #endregion

    #region Constructor

    public ViewModel_Receiving_Wizard_Display_ReviewSummary(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Review Summary ViewModel initialized");
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void EnterEditMode()
    {
        _logger.LogInfo("User entered Edit Mode from Review Summary");
        StatusMessage = "Entering Edit Mode...";
    }

    [RelayCommand]
    private void RecalculateTotals()
    {
        try
        {
            TotalLoads = LoadsReadOnly.Count;
            TotalWeight = LoadsReadOnly.Sum(l => l.Weight ?? 0);
            TotalQuantity = LoadsReadOnly.Sum(l => l.Quantity ?? 0);
            TotalPackages = LoadsReadOnly.Sum(l => l.PackagesPerLoad ?? 0);

            SummaryText = $"{TotalLoads} loads | {TotalWeight:N0} lbs | {TotalQuantity:N0} units | {TotalPackages} packages";

            _logger.LogInfo($"Totals recalculated: {SummaryText}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recalculating totals: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AcknowledgeQualityHold()
    {
        QualityHoldAcknowledged = true;
        _logger.LogInfo($"Quality Hold acknowledged for part {PartNumber}");
        StatusMessage = "Quality Hold acknowledged";
    }

    [RelayCommand]
    private void ReturnToStep2()
    {
        _logger.LogInfo("User returning to Step 2 from Review Summary");
        StatusMessage = "Returning to Step 2...";
    }

    #endregion

    #region Public Methods

    public void SetTransactionData(
        string poNumber,
        string partNumber,
        string partType,
        ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow> loads)
    {
        _poNumber = poNumber ?? string.Empty;
        _partNumber = partNumber ?? string.Empty;
        _partType = partType ?? string.Empty;
        LoadsReadOnly = loads ?? new ObservableCollection<Model_Receiving_DataTransferObjects_LoadGridRow>();

        RecalculateTotals();
        CheckQualityHold();

        _logger.LogInfo($"Transaction data set: PO={_poNumber}, Part={_partNumber}, Loads={TotalLoads}");
    }

    public bool ValidateBeforeSave()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_poNumber))
            errors.Add("PO Number is missing");

        if (string.IsNullOrWhiteSpace(_partNumber))
            errors.Add("Part Number is missing");

        if (TotalLoads == 0)
            errors.Add("No loads to save");

        if (LoadsReadOnly.Any(l => l.HasErrors))
            errors.Add("Some loads have validation errors");

        if (HasQualityHold && !QualityHoldAcknowledged)
            errors.Add("Quality Hold warning must be acknowledged");

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("\n", errors);
            _logger.LogWarning($"Validation failed before save: {errorMessage}");
            return false;
        }

        _logger.LogInfo("Pre-save validation passed");
        return true;
    }

    #endregion

    #region Helper Methods

    private void CheckQualityHold()
    {
        var qualityHoldRequiredTypes = new[] { "CAST", "FORGE", "RAW", "BAR" };
        HasQualityHold = qualityHoldRequiredTypes.Contains(PartType?.ToUpperInvariant() ?? string.Empty);

        if (HasQualityHold)
        {
            QualityHoldMessage = $"⚠️ This part type ({PartType}) requires Quality Hold inspection before use.";
            _logger.LogWarning($"Quality Hold required for part {PartNumber} (Type: {PartType})");
        }
        else
        {
            QualityHoldMessage = string.Empty;
            QualityHoldAcknowledged = true;
        }
    }

    #endregion
}
