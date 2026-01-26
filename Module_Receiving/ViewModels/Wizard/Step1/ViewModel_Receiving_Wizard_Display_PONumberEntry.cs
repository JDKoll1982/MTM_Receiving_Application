using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

/// <summary>
/// Manages PO Number entry and validation for Wizard Step 1.
/// Provides real-time validation with auto-standardization.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_PONumberEntry : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _validationTimer;

    #region Observable Properties

    /// <summary>
    /// PO Number entered by user (auto-standardized to uppercase/trimmed).
    /// </summary>
    [ObservableProperty]
    private string _poNumber = string.Empty;

    /// <summary>
    /// Indicates if the entered PO Number is valid.
    /// </summary>
    [ObservableProperty]
    private bool _isPoValid = false;

    /// <summary>
    /// Validation message for PO Number (empty if valid).
    /// </summary>
    [ObservableProperty]
    private string _poValidationMessage = string.Empty;

    /// <summary>
    /// Indicates if Non-PO receiving mode is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isNonPo = false;

    /// <summary>
    /// Indicates if validation is currently in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isValidating = false;

    /// <summary>
    /// Placeholder text for PO Number entry field.
    /// </summary>
    [ObservableProperty]
    private string _poNumberPlaceholder = "Enter PO Number (e.g., PO-123456)";

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the PO Number entry ViewModel.
    /// </summary>
    public ViewModel_Receiving_Wizard_Display_PONumberEntry(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _logger.LogInfo("PO Number Entry ViewModel initialized");
    }

    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Handles PO Number changes with auto-standardization and debounced validation.
    /// </summary>
    partial void OnPoNumberChanged(string value)
    {
        // Auto-standardize: uppercase and trim
        var standardized = value?.ToUpperInvariant().Trim() ?? string.Empty;
        if (PoNumber != standardized)
        {
            PoNumber = standardized;
            return; // Will trigger this handler again with standardized value
        }

        // Cancel previous validation timer
        _validationTimer?.Stop();

        // Don't validate empty input immediately
        if (string.IsNullOrWhiteSpace(PoNumber))
        {
            IsPoValid = false;
            PoValidationMessage = string.Empty;
            return;
        }

        // Debounced validation (500ms delay)
        _validationTimer = _dispatcherQueue.CreateTimer();
        _validationTimer.Interval = TimeSpan.FromMilliseconds(500);
        _validationTimer.Tick += async (s, e) =>
        {
            _validationTimer.Stop();
            await ValidatePoNumberAsync();
        };
        _validationTimer.Start();
    }

    /// <summary>
    /// Handles Non-PO mode toggle - disables PO validation when enabled.
    /// </summary>
    partial void OnIsNonPoChanged(bool value)
    {
        if (value)
        {
            // Non-PO mode: skip validation
            IsPoValid = true;
            PoValidationMessage = string.Empty;
            PoNumberPlaceholder = "Non-PO receiving (PO not required)";
            _logger.LogInfo("Non-PO mode enabled");
        }
        else
        {
            // Regular PO mode: revalidate
            PoNumberPlaceholder = "Enter PO Number (e.g., PO-123456)";
            _ = ValidatePoNumberAsync();
            _logger.LogInfo("Non-PO mode disabled");
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Validates the entered PO Number against business rules.
    /// </summary>
    [RelayCommand]
    private async Task ValidatePoNumberAsync()
    {
        // Skip validation if Non-PO mode is enabled
        if (IsNonPo)
        {
            IsPoValid = true;
            PoValidationMessage = string.Empty;
            return;
        }

        // Validate required field
        if (string.IsNullOrWhiteSpace(PoNumber))
        {
            IsPoValid = false;
            PoValidationMessage = "PO Number is required";
            return;
        }

        IsValidating = true;
        try
        {
            var query = new QueryRequest_Receiving_Shared_Validate_PONumber { PONumber = PoNumber };
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                IsPoValid = result.Data.IsValid;
                PoValidationMessage = result.Data.ErrorMessage ?? string.Empty;

                // Apply normalized PO Number if different
                if (!string.IsNullOrEmpty(result.Data.FormattedPONumber) &&
                    result.Data.FormattedPONumber != PoNumber)
                {
                    _logger.LogInfo($"PO Number normalized: {PoNumber} -> {result.Data.FormattedPONumber}");
                    PoNumber = result.Data.FormattedPONumber;
                }

                if (IsPoValid)
                {
                    _logger.LogInfo($"PO Number validated successfully: {PoNumber}");
                    StatusMessage = "PO Number valid";
                }
                else
                {
                    _logger.LogWarning($"PO Number validation failed: {PoValidationMessage}");
                }
            }
            else
            {
                IsPoValid = false;
                PoValidationMessage = result.ErrorMessage ?? "Validation failed";
                _logger.LogError($"PO validation error: {PoValidationMessage}");
            }
        }
        catch (Exception ex)
        {
            IsPoValid = false;
            PoValidationMessage = "Validation error occurred";
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ValidatePoNumberAsync),
                nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
        }
        finally
        {
            IsValidating = false;
        }
    }

    /// <summary>
    /// Clears the PO Number and resets validation state.
    /// </summary>
    [RelayCommand]
    private void ClearPoNumber()
    {
        PoNumber = string.Empty;
        IsPoValid = false;
        PoValidationMessage = string.Empty;
        _logger.LogInfo("PO Number cleared");
        StatusMessage = "PO Number cleared";
    }

    /// <summary>
    /// Opens PO search dialog for user to select from existing POs.
    /// </summary>
    [RelayCommand]
    private async Task SearchPoNumberAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Opening PO search...";

            // PO search dialog will be implemented in View layer
            // This command triggers the dialog open event
            await Task.CompletedTask;

            _logger.LogInfo("PO search opened");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(SearchPoNumberAsync),
                nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
