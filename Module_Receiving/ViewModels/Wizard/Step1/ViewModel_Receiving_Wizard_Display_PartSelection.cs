using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

/// <summary>
/// Manages Part selection from PO for Wizard Step 1.
/// User SELECTS part from dropdown list (not manual entry).
/// Provides package type auto-detection and default value population.
/// ENHANCED: Quality Hold detection with first acknowledgment dialog (P0 CRITICAL)
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_PartSelection : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_Receiving_QualityHoldDetection _qualityHoldService;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _validationTimer;

    #region Observable Properties

    /// <summary>
    /// Parts available on the PO (passed from PONumberEntry ViewModel).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_InforVisualPO> _availablePartsOnPo = new();

    /// <summary>
    /// Part selected by user from the dropdown list.
    /// </summary>
    [ObservableProperty]
    private Model_InforVisualPO? _selectedPartFromPo;

    /// <summary>
    /// Indicates if part selection is valid.
    /// </summary>
    [ObservableProperty]
    private bool _isPartValid = false;

    /// <summary>
    /// Validation message for Part selection (empty if valid).
    /// </summary>
    [ObservableProperty]
    private string _partValidationMessage = string.Empty;

    /// <summary>
    /// Available part types from reference data.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_Receiving_TableEntitys_PartType> _availablePartTypes = new();

    /// <summary>
    /// Auto-detected or user-selected part type.
    /// </summary>
    [ObservableProperty]
    private Model_Receiving_TableEntitys_PartType? _selectedPartType;

    /// <summary>
    /// Description of the selected part from part master.
    /// </summary>
    [ObservableProperty]
    private string _partDescription = string.Empty;

    /// <summary>
    /// Default receiving location for this part.
    /// </summary>
    [ObservableProperty]
    private string _defaultReceivingLocation = string.Empty;

    /// <summary>
    /// Default package type for this part (auto-detected from Part ID prefix).
    /// MMC* = Coils, MMF* = Sheets, others = Skids
    /// </summary>
    [ObservableProperty]
    private string _defaultPackageType = "Skids";

    /// <summary>
    /// Indicates if part details are currently being loaded.
    /// </summary>
    [ObservableProperty]
    private bool _isLoadingPartDetails = false;

    /// <summary>
    /// Placeholder text for Part selection.
    /// </summary>
    [ObservableProperty]
    private string _partSelectionPlaceholder = "Select part from PO...";

    /// <summary>
    /// Available package types for user selection.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _availablePackageTypes = new() { "Coils", "Sheets", "Skids", "Custom" };

    /// <summary>
    /// Currently selected package type (auto-detected or manually overridden).
    /// </summary>
    [ObservableProperty]
    private string _selectedPackageType = "Skids";

    /// <summary>
    /// Custom package type name (when user selects "Custom").
    /// </summary>
    [ObservableProperty]
    private string _customPackageTypeName = string.Empty;

    /// <summary>
    /// Whether custom package type name field is visible.
    /// </summary>
    [ObservableProperty]
    private bool _isCustomPackageTypeVisible = false;

    /// <summary>
    /// Whether this part requires quality hold (detected from configurable patterns)
    /// </summary>
    [ObservableProperty]
    private bool _isQualityHoldRequired = false;

    /// <summary>
    /// Type of quality hold restriction (e.g., "Weight Sensitive", "Quality Control")
    /// </summary>
    [ObservableProperty]
    private string? _qualityHoldRestrictionType;

    /// <summary>
    /// Matched pattern that triggered quality hold
    /// </summary>
    [ObservableProperty]
    private string? _qualityHoldMatchedPattern;

    /// <summary>
    /// Whether user acknowledged quality hold (Step 1 of 2)
    /// </summary>
    [ObservableProperty]
    private bool _userAcknowledgedQualityHold = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Part Selection ViewModel.
    /// </summary>
    public ViewModel_Receiving_Wizard_Display_PartSelection(
        IMediator mediator,
        IService_Receiving_QualityHoldDetection qualityHoldService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _qualityHoldService = qualityHoldService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _logger.LogInfo("Part Selection ViewModel initialized");

        // Load available part types on initialization
        _ = LoadPartTypesAsync();
    }

    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Handles part selection from dropdown list.
    /// Auto-detects package type and triggers quality hold check.
    /// </summary>
    partial void OnSelectedPartFromPoChanged(Model_InforVisualPO? value)
    {
        if (value is null)
        {
            IsPartValid = false;
            PartValidationMessage = string.Empty;
            ClearPartDetails();
            return;
        }

        // Part selected - mark as valid
        IsPartValid = true;
        PartValidationMessage = string.Empty;
        PartDescription = value.PartDescription;

        // Auto-detect package type from Part Number prefix
        var upperPart = value.PartNumber.ToUpperInvariant().Trim();
        if (upperPart.StartsWith("MMC"))
        {
            SelectedPackageType = "Coils";
            DefaultPackageType = "Coils";
            _logger.LogInfo($"Part {value.PartNumber}: Auto-detected package type = Coils");
        }
        else if (upperPart.StartsWith("MMF"))
        {
            SelectedPackageType = "Sheets";
            DefaultPackageType = "Sheets";
            _logger.LogInfo($"Part {value.PartNumber}: Auto-detected package type = Sheets");
        }
        else
        {
            SelectedPackageType = "Skids";
            DefaultPackageType = "Skids";
            _logger.LogInfo($"Part {value.PartNumber}: Auto-detected package type = Skids (default)");
        }

        StatusMessage = $"Part selected: {value.PartNumber} - {value.PartDescription}";
        _logger.LogInfo($"Part selected from PO: {value.PartNumber}");

        // Load additional part details and check quality hold
        _ = LoadPartDetailsAsync(value.PartNumber);
    }

    /// <summary>
    /// Handles manual package type selection - shows/hides custom name field.
    /// </summary>
    partial void OnSelectedPackageTypeChanged(string value)
    {
        IsCustomPackageTypeVisible = value == "Custom";
        _logger.LogInfo($"Package type changed to: {value}");
    }

    #endregion

    #region Commands

    /// <summary>
    /// Loads part details (part type, receiving location, package preferences) from part master.
    /// Also triggers quality hold detection for restricted parts.
    /// </summary>
    private async Task LoadPartDetailsAsync(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber)) return;

        IsLoadingPartDetails = true;
        try
        {
            var query = new QueryRequest_Receiving_Shared_Get_PartDetails { PartNumber = partNumber };
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                SelectedPartType = result.Data.PartType;
                DefaultReceivingLocation = result.Data.DefaultLocation ?? "RECV";
                // Note: Package type already set by OnSelectedPartFromPoChanged, don't override
                
                _logger.LogInfo($"Part details loaded for {partNumber}: Type={SelectedPartType?.PartTypeName}, Location={DefaultReceivingLocation}");
                StatusMessage = "Part details loaded";
                
                // Check for quality hold after part details loaded
                await DetectQualityHoldAsync(partNumber);
            }
            else
            {
                // Part not found in master - use defaults
                DefaultReceivingLocation = "RECV";
                _logger.LogWarning($"Part details not found for {partNumber}");
                StatusMessage = "Part not found in master - using defaults";
                
                // Still check quality hold even if not in master
                await DetectQualityHoldAsync(partNumber);
            }
        }
        catch (Exception ex)
        {
            ClearPartDetails();
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(LoadPartDetailsAsync),
                nameof(ViewModel_Receiving_Wizard_Display_PartSelection));
        }
        finally
        {
            IsLoadingPartDetails = false;
        }
    }

    /// <summary>
    /// Opens Part Number search dialog.
    /// </summary>
    [RelayCommand]
    private async Task SearchPartNumberAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Opening part search...";

            // Part search dialog will be implemented in View layer
            await Task.CompletedTask;

            _logger.LogInfo("Part search opened");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(SearchPartNumberAsync),
                nameof(ViewModel_Receiving_Wizard_Display_PartSelection));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Manually selects a part type (overrides auto-detection).
    /// </summary>
    /// <param name="partType">The part type to select.</param>
    [RelayCommand]
    private void SelectPartType(Model_Receiving_TableEntitys_PartType partType)
    {
        SelectedPartType = partType;
        _logger.LogInfo($"Part type manually selected: {partType.PartTypeName}");
        StatusMessage = $"Part type: {partType.PartTypeName}";
    }

    /// <summary>
    /// Receives parts list from PONumberEntry ViewModel.
    /// Called by orchestration after PO is loaded.
    /// </summary>
    public void SetAvailablePartsFromPo(ObservableCollection<Model_InforVisualPO> partsFromPo)
    {
        ArgumentNullException.ThrowIfNull(partsFromPo);
        
        AvailablePartsOnPo.Clear();
        foreach (var part in partsFromPo)
        {
            AvailablePartsOnPo.Add(part);
        }
        
        _logger.LogInfo($"Received {AvailablePartsOnPo.Count} parts from PO for selection");
        StatusMessage = AvailablePartsOnPo.Count > 0 
            ? $"{AvailablePartsOnPo.Count} parts available - select one" 
            : "No parts found on PO";
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Loads available part types from reference data.
    /// </summary>
    private async Task LoadPartTypesAsync()
    {
        try
        {
            var query = new QueryRequest_Receiving_Shared_Get_ReferenceData();
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                AvailablePartTypes.Clear();
                foreach (var partType in result.Data.PartTypes)
                {
                    AvailablePartTypes.Add(partType);
                }
                _logger.LogInfo($"Loaded {AvailablePartTypes.Count} part types");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load part types: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears loaded part details.
    /// </summary>
    private void ClearPartDetails()
    {
        SelectedPartType = null;
        DefaultReceivingLocation = "RECV";
        DefaultPackageType = "Skids";
        PartDescription = string.Empty;
        
        // Clear quality hold flags
        IsQualityHoldRequired = false;
        QualityHoldRestrictionType = null;
        QualityHoldMatchedPattern = null;
        UserAcknowledgedQualityHold = false;
    }

    /// <summary>
    /// Detects if part requires quality hold and shows first acknowledgment dialog
    /// STEP 1 OF 2 - User Acknowledgment on Part Selection
    /// User Requirement: Configurable patterns, two-step acknowledgment
    /// </summary>
    private async Task DetectQualityHoldAsync(string partNumber)
    {
        try
        {
            var (isRestricted, userAcknowledged, matchedPattern, restrictionType) = 
                await _qualityHoldService.DetectAndShowFirstWarningAsync(partNumber);

            if (isRestricted)
            {
                IsQualityHoldRequired = true;
                QualityHoldMatchedPattern = matchedPattern;
                QualityHoldRestrictionType = restrictionType;
                UserAcknowledgedQualityHold = userAcknowledged;

                if (userAcknowledged)
                {
                    _logger.LogInfo($"Quality hold acknowledged (Step 1 of 2) for part {partNumber}");
                    StatusMessage = $"⚠️ Quality Hold: {restrictionType} - Step 1 acknowledged";
                }
                else
                {
                    _logger.LogWarning($"Quality hold cancelled for part {partNumber}");
                    StatusMessage = "Quality hold cancelled - part selection cancelled";
                    
                    // Clear part selection if user cancels acknowledgment
                    SelectedPartFromPo = null;
                    ClearPartDetails();
                }
            }
            else
            {
                // No quality hold required
                IsQualityHoldRequired = false;
                QualityHoldRestrictionType = null;
                QualityHoldMatchedPattern = null;
                UserAcknowledgedQualityHold = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error detecting quality hold: {ex.Message}", ex);
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(DetectQualityHoldAsync),
                nameof(ViewModel_Receiving_Wizard_Display_PartSelection));
        }
    }

    #endregion
}
