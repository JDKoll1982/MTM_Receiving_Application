using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Models.Enums;
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
    private readonly ILoggingService _logger;

    // Reference to parent container
    private readonly RoutingWizardContainerViewModel _containerViewModel;

    #region Constructor
    public RoutingWizardStep1ViewModel(
        IRoutingInforVisualService inforVisualService,
        IRoutingService routingService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger,
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
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(PONumber))
        {
            StatusMessage = "Please enter a PO number";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Validating PO...";

            // Validate PO exists in Infor Visual
            var validationResult = await _inforVisualService.ValidatePOAsync(PONumber);

            if (validationResult.IsSuccess && validationResult.Data)
            {
                // PO is valid - fetch lines
                var linesResult = await _inforVisualService.GetPOLinesAsync(PONumber);

                if (linesResult.IsSuccess && linesResult.Data != null)
                {
                    POLines.Clear();
                    foreach (var line in linesResult.Data)
                    {
                        POLines.Add(line);
                    }

                    StatusMessage = $"Found {POLines.Count} line(s) for PO {PONumber}";
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

            // Load OTHER reasons
            var reasonsResult = await _routingService.GetOtherReasonsAsync();

            if (reasonsResult.IsSuccess && reasonsResult.Data != null)
            {
                OtherReasons.Clear();
                foreach (var reason in reasonsResult.Data)
                {
                    OtherReasons.Add(reason);
                }

                IsOtherMode = true;
                POLines.Clear();
                StatusMessage = "Select a reason for non-PO package";
            }
            else
            {
                _errorHandler.ShowUserError(
                    "Failed to load OTHER reasons",
                    "Data Load Error",
                    nameof(SwitchToOtherModeAsync));
            }
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
        var dialog = new ContentDialog
        {
            Title = "PO Not Found",
            Content = $"PO '{PONumber}' was not found in Infor Visual.\n\nWould you like to treat this as an OTHER (non-PO) package?",
            PrimaryButtonText = "Yes, treat as OTHER",
            CloseButtonText = "No, try again",
            DefaultButton = ContentDialogButton.Close
        };

        // Set XamlRoot for dialog
        dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // User chose to treat as OTHER
            await SwitchToOtherModeAsync();
        }
        else
        {
            // User chose to retry
            PONumber = string.Empty;
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
