using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for Wizard Step 1: PO and Line Selection (or OTHER workflow)
/// </summary>
public partial class RoutingWizardStep1ViewModel : ObservableObject
{
    private readonly IRoutingInforVisualService _inforVisualService;
    private readonly IRoutingService _routingService;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_LoggingUtility _logger;

    // Issue #24: TODO - Replace with WeakReferenceMessenger for loose coupling
    // Current implementation uses direct parent reference for communication
    // Migration requires: Message classes, messenger registration, 3 step ViewModels update
    private readonly RoutingWizardContainerViewModel _containerViewModel;

    #region Constructor
    public RoutingWizardStep1ViewModel(
        IRoutingInforVisualService inforVisualService,
        IRoutingService routingService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        RoutingWizardContainerViewModel containerViewModel)
    {
        _inforVisualService = inforVisualService;
        _routingService = routingService;
        _errorHandler = errorHandler;
        _logger = logger;
        _containerViewModel = containerViewModel;
    }
    #endregion

    #region Observable Properties
    /// <summary>
    /// PO Number entered by user
    /// </summary>
    [ObservableProperty]
    private string _poNumber = string.Empty;

    /// <summary>
    /// List of PO lines retrieved from Infor Visual
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_InforVisualPOLine> _poLines = new();

    /// <summary>
    /// Selected PO line from DataGrid
    /// </summary>
    [ObservableProperty]
    private Model_InforVisualPOLine? _selectedPOLine;

    /// <summary>
    /// Indicates whether we're in OTHER mode (no PO validation)
    /// </summary>
    [ObservableProperty]
    private bool _isOtherMode;

    /// <summary>
    /// Available OTHER reasons (RETURNED, SAMPLE, etc.)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_RoutingOtherReason> _otherReasons = new();

    /// <summary>
    /// Selected OTHER reason from dropdown
    /// </summary>
    [ObservableProperty]
    private Model_RoutingOtherReason? _selectedOtherReason;

    /// <summary>
    /// Manual quantity for OTHER workflow
    /// </summary>
    [ObservableProperty]
    private int _otherQuantity = 1;

    /// <summary>
    /// Busy indicator
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Enter PO number or select OTHER";
    #endregion

    #region Commands
    /// <summary>
    /// Validate PO number and fetch lines
    /// </summary>
    [RelayCommand]
    private async Task ValidatePOAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(PoNumber))
        {
            StatusMessage = "Please enter a PO number";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Validating PO...";

            // Validate PO exists in Infor Visual
            var validationResult = await _inforVisualService.ValidatePoNumberAsync(PoNumber);

            if (validationResult.IsSuccess && validationResult.Data)
            {
                // Issue #30: Better progress feedback
                StatusMessage = "PO valid - fetching line items...";

                // PO is valid - fetch lines
                var linesResult = await _inforVisualService.GetPoLinesAsync(PoNumber);

                if (linesResult.IsSuccess && linesResult.Data != null)
                {
                    PoLines.Clear();
                    foreach (var line in linesResult.Data)
                    {
                        PoLines.Add(line);
                    }

                    // Issue #27: Check for empty lines result
                    if (PoLines.Count == 0)
                    {
                        StatusMessage = $"PO {PoNumber} is valid but has no open lines available";
                        await _errorHandler.ShowUserErrorAsync(
                            $"PO {PoNumber} exists but has no open line items to route.",
                            "No Lines Available",
                            nameof(ValidatePOAsync)
                        );
                    }
                    else
                    {
                        StatusMessage = $"Found {PoLines.Count} line(s) for PO {PoNumber}";
                    }
                }
                else
                {
                    StatusMessage = "No lines found for this PO";
                }
            }
            else
            {
                // PO not found or Infor Visual offline
                await ShowPONotFoundDialogAsync();
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ValidatePOAsync),
                nameof(RoutingWizardStep1ViewModel));
            StatusMessage = "Error validating PO";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Switch to OTHER mode (manual entry without PO)
    /// </summary>
    [RelayCommand]
    private async Task SwitchToOtherModeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading OTHER reasons...";

            // Issue #13: GetOtherReasonsAsync not implemented in IRoutingService
            // Feature requires database table: routing_other_reasons (id, reason_code, description, is_active)
            // Stored procedure: sp_routing_other_reasons_get_active
            // Priority: LOW - Wizard mode (PO-based) is primary workflow
            // var reasonsResult = await _routingService.GetOtherReasonsAsync();
            // if (reasonsResult.IsSuccess && reasonsResult.Data != null)
            // {
            //     OtherReasons.Clear();
            //     foreach (var reason in reasonsResult.Data)
            //     {
            //         OtherReasons.Add(reason);
            //     }
            //     IsOtherMode = true;
            //     PoLines.Clear();
            //     StatusMessage = "Select a reason for non-PO package";
            // }
            // else
            // {
            //     await _errorHandler.ShowUserErrorAsync(
            //         "Failed to load OTHER reasons",
            //         "Data Load Error",
            //         nameof(SwitchToOtherModeAsync));
            // }

            await Task.CompletedTask;
            IsOtherMode = true;
            PoLines.Clear();
            StatusMessage = "OTHER reasons loading not yet implemented";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SwitchToOtherModeAsync),
                nameof(RoutingWizardStep1ViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Proceed to Step 2 (Recipient Selection)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanProceedToStep2))]
    private void ProceedToStep2()
    {
        // Update container with selected data
        if (IsOtherMode && SelectedOtherReason != null)
        {
            _containerViewModel.SelectedOtherReason = SelectedOtherReason;
            _containerViewModel.OtherQuantity = OtherQuantity;
            _containerViewModel.SelectedPOLine = null;
        }
        else if (SelectedPOLine != null)
        {
            _containerViewModel.SelectedPOLine = SelectedPOLine;
            _containerViewModel.SelectedOtherReason = null;
        }

        // Navigate to Step 2
        _containerViewModel.NavigateToStep2Command.Execute(null);
    }

    /// <summary>
    /// Can proceed to Step 2 if either PO line or OTHER reason is selected
    /// </summary>
    private bool CanProceedToStep2()
    {
        return (IsOtherMode && SelectedOtherReason != null) ||
               (!IsOtherMode && SelectedPOLine != null);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Show "PO not found" confirmation dialog
    /// </summary>
    private async Task ShowPONotFoundDialogAsync()
    {
        // Issue #29: Null guard for XamlRoot
        if (App.MainWindow?.Content?.XamlRoot == null)
        {
            await _errorHandler.ShowUserErrorAsync(
                "Unable to display dialog - window not initialized",
                "UI Error",
                nameof(ShowPONotFoundDialogAsync)
            );
            StatusMessage = "Error: UI not ready";
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "PO Not Found",
            Content = $"PO '{PoNumber}' was not found in Infor Visual.\n\nWould you like to treat this as an OTHER (non-PO) package?",
            PrimaryButtonText = "Yes, treat as OTHER",
            CloseButtonText = "No, try again",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // User chose to treat as OTHER
            await SwitchToOtherModeAsync();
        }
        else
        {
            // User chose to retry
            PoNumber = string.Empty;
            StatusMessage = "Enter a valid PO number";
        }
    }

    /// <summary>
    /// Notify command can execute changed when selection changes
    /// </summary>
    partial void OnSelectedPOLineChanged(Model_InforVisualPOLine? value)
    {
        ProceedToStep2Command.NotifyCanExecuteChanged();
    }

    partial void OnSelectedOtherReasonChanged(Model_RoutingOtherReason? value)
    {
        ProceedToStep2Command.NotifyCanExecuteChanged();
    }
    #endregion
}
