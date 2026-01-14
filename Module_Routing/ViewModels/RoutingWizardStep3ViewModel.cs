using System;
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
/// ViewModel for Wizard Step 3: Review and Confirm
/// </summary>
public partial class RoutingWizardStep3ViewModel : ObservableObject
{
    private readonly IRoutingService _routingService;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_LoggingUtility _logger;
    private readonly RoutingWizardContainerViewModel _containerViewModel;

    #region Constructor
    public RoutingWizardStep3ViewModel(
        IRoutingService routingService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        RoutingWizardContainerViewModel containerViewModel)
    {
        _routingService = routingService;
        _errorHandler = errorHandler;
        _logger = logger;
        _containerViewModel = containerViewModel;

        // Register with container so it can trigger LoadReviewData
        _containerViewModel.RegisterStep3ViewModel(this);
        _logger.LogInfo("Step3ViewModel initialized and registered");
    }
    #endregion

    #region Observable Properties
    /// <summary>
    /// Summary of label data for review
    /// </summary>
    [ObservableProperty]
    private string _poNumber = string.Empty;

    [ObservableProperty]
    private int _poLine;

    [ObservableProperty]
    private string _partID = string.Empty;

    [ObservableProperty]
    private string _partDescription = string.Empty;

    [ObservableProperty]
    private string _recipientName = string.Empty;

    [ObservableProperty]
    private string _recipientLocation = string.Empty;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private string _otherReason = string.Empty;

    [ObservableProperty]
    private bool _isOtherWorkflow;

    /// <summary>
    /// Busy indicator
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Review label details";
    #endregion

    #region Initialization
    /// <summary>
    /// Load review data from container ViewModel
    /// </summary>
    public void LoadReviewData()
    {
        _logger.LogInfo("LoadReviewData called");
        _logger.LogInfo($"Container state - SelectedPOLine: {_containerViewModel.SelectedPOLine?.PartID ?? "null"}, SelectedRecipient: {_containerViewModel.SelectedRecipient?.Name ?? "null"}, FinalQuantity: {_containerViewModel.FinalQuantity}");

        // Load from container state
        if (_containerViewModel.SelectedPOLine != null)
        {
            // PO workflow
            IsOtherWorkflow = false;
            PoNumber = _containerViewModel.SelectedPOLine.PONumber;
            PoLine = int.TryParse(_containerViewModel.SelectedPOLine.LineNumber, out var lineNum) ? lineNum : 0;
            PartID = _containerViewModel.SelectedPOLine.PartID;
            PartDescription = _containerViewModel.SelectedPOLine.Description;
            _logger.LogInfo($"Loaded PO data - PO: {PoNumber}, Line: {PoLine}, Part: {PartID}");
        }
        else if (_containerViewModel.SelectedOtherReason != null)
        {
            // OTHER workflow
            IsOtherWorkflow = true;
            PoNumber = "OTHER";
            PoLine = 0;
            PartID = $"OTHER-{_containerViewModel.SelectedOtherReason.ReasonCode}";
            PartDescription = _containerViewModel.SelectedOtherReason.Description;
            OtherReason = _containerViewModel.SelectedOtherReason.Description;
            _logger.LogInfo($"Loaded OTHER data - Reason: {OtherReason}");
        }
        else
        {
            _logger.LogWarning("LoadReviewData: No PO line or OTHER reason in container!");
        }

        // Load recipient
        if (_containerViewModel.SelectedRecipient != null)
        {
            RecipientName = _containerViewModel.SelectedRecipient.Name;
            RecipientLocation = _containerViewModel.SelectedRecipient.Location ?? string.Empty;
            _logger.LogInfo($"Loaded recipient - Name: {RecipientName}, Location: {RecipientLocation}");
        }
        else
        {
            _logger.LogWarning("LoadReviewData: No recipient in container!");
        }

        // Load quantity
        Quantity = _containerViewModel.FinalQuantity;
        _logger.LogInfo($"Final quantity: {Quantity}");

        StatusMessage = "Review and confirm label details";
    }
    #endregion

    #region Edit Commands
    /// <summary>
    /// Edit PO/Line selection (go back to Step 1)
    /// </summary>
    [RelayCommand]
    private void EditPOSelection()
    {
        _logger.LogInfo("EditPOSelection - Navigating to Step 1 in edit mode");
        _containerViewModel.NavigateToStep1ForEditCommand.Execute(null);
    }

    /// <summary>
    /// Edit recipient selection (go back to Step 2)
    /// </summary>
    [RelayCommand]
    private void EditRecipientSelection()
    {
        _logger.LogInfo("EditRecipientSelection - Navigating to Step 2 in edit mode");
        _containerViewModel.NavigateToStep2ForEditCommand.Execute(null);
    }

    /// <summary>
    /// Navigate back to Step 2 (standard back navigation)
    /// </summary>
    [RelayCommand]
    private void NavigateBack()
    {
        _logger.LogInfo("NavigateBack - Navigating back to Step 2");
        _containerViewModel.NavigateBackToStep2Command.Execute(null);
    }
    #endregion

    #region Create Label Command
    /// <summary>
    /// Create the routing label with duplicate check
    /// </summary>
    [RelayCommand]
    private async Task CreateLabelAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Checking for duplicates...";

            // Check for duplicate labels (24-hour window)
            var duplicateCheckResult = await _routingService.CheckDuplicateLabelAsync(
                PoNumber,
                PoLine.ToString(),
                _containerViewModel.SelectedRecipient?.Id ?? 0,
                DateTime.Now);

            if (duplicateCheckResult.IsSuccess && duplicateCheckResult.Data.Exists)
            {
                // Duplicate found - show confirmation
                var continueAnyway = await ShowDuplicateConfirmationAsync();
                if (!continueAnyway)
                {
                    StatusMessage = "Label creation cancelled";
                    return;
                }
            }

            // Proceed with label creation via container ViewModel
            StatusMessage = "Creating label...";
            await _containerViewModel.CreateLabelCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(CreateLabelAsync),
                nameof(RoutingWizardStep3ViewModel));
            StatusMessage = "Error creating label";
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Show duplicate confirmation dialog
    /// </summary>
    private async Task<bool> ShowDuplicateConfirmationAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Duplicate Label Detected",
            Content = $"A similar label was created within the last 24 hours.\n\n" +
                     $"PO: {PoNumber} Line: {PoLine}\n" +
                     $"Part: {PartID}\n" +
                     $"Recipient: {RecipientName}\n\n" +
                     $"Do you want to create this label anyway?",
            PrimaryButtonText = "Yes, create anyway",
            CloseButtonText = "No, cancel",
            DefaultButton = ContentDialogButton.Close
        };

        // Set XamlRoot for dialog
        dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
    #endregion
}
