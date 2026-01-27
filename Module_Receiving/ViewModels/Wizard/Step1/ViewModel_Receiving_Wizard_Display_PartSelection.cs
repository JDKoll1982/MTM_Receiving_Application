using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

/// <summary>
/// Manages Part Number selection and validation for Wizard Step 1.
/// Provides auto-padding, part type detection, and default value population.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_PartSelection : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _validationTimer;

    #region Observable Properties

    /// <summary>
    /// Part Number entered by user (auto-padded to 6 digits).
    /// </summary>
    [ObservableProperty]
    private string _partNumber = string.Empty;

    /// <summary>
    /// Indicates if the entered Part Number is valid.
    /// </summary>
    [ObservableProperty]
    private bool _isPartValid = false;

    /// <summary>
    /// Validation message for Part Number (empty if valid).
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
    /// Default package type for this part.
    /// </summary>
    [ObservableProperty]
    private string _defaultPackageType = string.Empty;

    /// <summary>
    /// Indicates if part details are currently being loaded.
    /// </summary>
    [ObservableProperty]
    private bool _isLoadingPartDetails = false;

    /// <summary>
    /// Placeholder text for Part Number entry field.
    /// </summary>
    [ObservableProperty]
    private string _partNumberPlaceholder = "Enter Part Number (e.g., MMC0001000)";

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Part Selection ViewModel.
    /// </summary>
    public ViewModel_Receiving_Wizard_Display_PartSelection(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _logger.LogInfo("Part Selection ViewModel initialized");

        // Load available part types on initialization
        _ = LoadPartTypesAsync();
    }

    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Handles Part Number changes with auto-padding and debounced validation.
    /// </summary>
    partial void OnPartNumberChanged(string value)
    {
        // Auto-pad the part number
        var padded = AutoPadPartNumber(value);
        if (PartNumber != padded)
        {
            PartNumber = padded;
            return; // Will trigger this handler again with padded value
        }

        // Cancel previous validation timer
        _validationTimer?.Stop();

        // Don't validate empty input
        if (string.IsNullOrWhiteSpace(PartNumber))
        {
            IsPartValid = false;
            PartValidationMessage = string.Empty;
            ClearPartDetails();
            return;
        }

        // Debounced validation and part details loading (500ms delay)
        _validationTimer = _dispatcherQueue.CreateTimer();
        _validationTimer.Interval = TimeSpan.FromMilliseconds(500);
        _validationTimer.Tick += async (s, e) =>
        {
            _validationTimer.Stop();
            await ValidatePartNumberAsync();
            if (IsPartValid)
            {
                await LoadPartDetailsAsync();
            }
        };
        _validationTimer.Start();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Validates the entered Part Number.
    /// </summary>
    [RelayCommand]
    private async Task ValidatePartNumberAsync()
    {
        if (string.IsNullOrWhiteSpace(PartNumber))
        {
            IsPartValid = false;
            PartValidationMessage = "Part Number is required";
            return;
        }

        try
        {
            // Basic format validation (can be enhanced with actual validation logic)
            var regex = new Regex(@"^[A-Z]{3}\d{6}$");
            if (!regex.IsMatch(PartNumber))
            {
                IsPartValid = false;
                PartValidationMessage = "Part Number must be 3 letters followed by 6 digits";
                _logger.LogWarning($"Invalid part number format: {PartNumber}");
                return;
            }

            IsPartValid = true;
            PartValidationMessage = string.Empty;
            _logger.LogInfo($"Part Number validated: {PartNumber}");
            StatusMessage = "Part Number valid";
        }
        catch (Exception ex)
        {
            IsPartValid = false;
            PartValidationMessage = "Validation error occurred";
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(ValidatePartNumberAsync),
                nameof(ViewModel_Receiving_Wizard_Display_PartSelection));
        }
    }

    /// <summary>
    /// Loads part details (description, defaults) from part master query.
    /// </summary>
    [RelayCommand]
    private async Task LoadPartDetailsAsync()
    {
        if (!IsPartValid || string.IsNullOrWhiteSpace(PartNumber)) return;

        IsLoadingPartDetails = true;
        try
        {
            var query = new QueryRequest_Receiving_Shared_Get_PartDetails { PartNumber = PartNumber };
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                SelectedPartType = result.Data.PartType;
                DefaultReceivingLocation = result.Data.DefaultLocation ?? "RECV";
                DefaultPackageType = result.Data.DefaultPackageType ?? "Skid";
                PartDescription = result.Data.Description ?? string.Empty;

                _logger.LogInfo($"Part details loaded for {PartNumber}: Type={SelectedPartType?.PartTypeName}, Location={DefaultReceivingLocation}");
                StatusMessage = "Part details loaded";
            }
            else
            {
                // Part not found in master - use defaults
                ClearPartDetails();
                _logger.LogWarning($"Part details not found for {PartNumber}");
                StatusMessage = "Part not found in master - using defaults";
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

    #endregion

    #region Helper Methods

    /// <summary>
    /// Auto-pads the part number to standard format (3 letters + 6 digits).
    /// Examples: MMC1 → MMC000001, MMC123 → MMC000123, MMC123456 → MMC123456
    /// </summary>
    private static string AutoPadPartNumber(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber)) return string.Empty;

        // Standardize to uppercase
        partNumber = partNumber.ToUpperInvariant().Trim();

        // Match pattern: 3 letters followed by digits
        var match = Regex.Match(partNumber, @"^([A-Z]{3})(\d+)$");
        if (match.Success)
        {
            var prefix = match.Groups[1].Value;
            var number = match.Groups[2].Value.PadLeft(6, '0');
            return $"{prefix}{number}";
        }

        // Return as-is if doesn't match expected pattern
        return partNumber;
    }

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
        DefaultPackageType = "Skid";
        PartDescription = string.Empty;
    }

    #endregion
}
