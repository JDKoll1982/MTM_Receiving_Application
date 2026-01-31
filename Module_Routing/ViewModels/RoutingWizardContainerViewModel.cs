using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// Container ViewModel managing wizard state and navigation between steps
/// </summary>
public partial class RoutingWizardContainerViewModel : ObservableObject
{
    private readonly IRoutingService _routingService;
    private readonly IRoutingInforVisualService _inforVisualService;
    private readonly IRoutingUsageTrackingService _usageTrackingService;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_UserSessionManager _sessionManager;

    // Lazy-loaded Step3 ViewModel for triggering data refresh
    private RoutingWizardStep3ViewModel? _step3ViewModel;

    #region Constructor
    public RoutingWizardContainerViewModel(
        IRoutingService routingService,
        IRoutingInforVisualService inforVisualService,
        IRoutingUsageTrackingService usageTrackingService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_UserSessionManager sessionManager)
    {
        _routingService = routingService;
        _inforVisualService = inforVisualService;
        _usageTrackingService = usageTrackingService;
        _errorHandler = errorHandler;
        _logger = logger;
        _sessionManager = sessionManager;
    }

    #endregion

    #region Observable Properties
    /// <summary>
    /// Current wizard step (1, 2, or 3)
    /// </summary>
    [ObservableProperty]
    private int _currentStep = 1;

    /// <summary>
    /// Busy indicator for async operations
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Status message for user feedback
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Selected PO line from Step 1 (null if OTHER workflow)
    /// </summary>
    [ObservableProperty]
    private Model_InforVisualPOLine? _selectedPOLine;

    /// <summary>
    /// Selected OTHER reason from Step 1 (null if PO workflow)
    /// </summary>
    [ObservableProperty]
    private Model_RoutingOtherReason? _selectedOtherReason;

    /// <summary>
    /// Manual quantity for OTHER workflow
    /// </summary>
    [ObservableProperty]
    private int _otherQuantity = 1;

    /// <summary>
    /// Selected recipient from Step 2
    /// </summary>
    [ObservableProperty]
    private Model_RoutingRecipient? _selectedRecipient;

    /// <summary>
    /// Final label quantity (from PO line or manual entry)
    /// </summary>
    [ObservableProperty]
    private int _finalQuantity;

    /// <summary>
    /// Indicates if we're in edit mode from Step 3 review
    /// </summary>
    [ObservableProperty]
    private bool _isEditingFromReview;
    #endregion

    #region Navigation Commands
    /// <summary>
    /// Navigate to Step 2 (Recipient Selection)
    /// </summary>
    [RelayCommand]
    private void NavigateToStep2()
    {
        _logger.LogInfo($"NavigateToStep2 called - CurrentStep: {CurrentStep}, SelectedPOLine: {SelectedPOLine?.PartID ?? "null"}, SelectedOtherReason: {SelectedOtherReason?.Description ?? "null"}");

        // Validation: Must have either PO line or OTHER reason
        if (SelectedPOLine == null && SelectedOtherReason == null)
        {
            _logger.LogWarning("NavigateToStep2: No PO line or OTHER reason selected");
            StatusMessage = "Please select a PO line or OTHER reason";
            return;
        }

        // Set quantity from PO line if applicable
        if (SelectedPOLine != null)
        {
            FinalQuantity = (int)SelectedPOLine.QuantityOrdered;
            _logger.LogInfo($"Set FinalQuantity from PO line: {FinalQuantity}");
        }
        else
        {
            FinalQuantity = OtherQuantity;
            _logger.LogInfo($"Set FinalQuantity from OTHER: {FinalQuantity}");
        }

        _logger.LogInfo($"Changing CurrentStep from {CurrentStep} to 2");
        CurrentStep = 2;
        StatusMessage = "Select recipient";
        _logger.LogInfo($"CurrentStep is now: {CurrentStep}");
    }

    /// <summary>
    /// Navigate to Step 3 (Review)
    /// </summary>
    [RelayCommand]
    private void NavigateToStep3()
    {
        _logger.LogInfo($"NavigateToStep3 called - SelectedRecipient: {SelectedRecipient?.Name ?? "null"}");

        // Validation: Must have recipient
        if (SelectedRecipient == null)
        {
            _logger.LogWarning("NavigateToStep3: No recipient selected");
            StatusMessage = "Please select a recipient";
            return;
        }

        _logger.LogInfo($"Navigating to Step 3 - Recipient: {SelectedRecipient.Name}, CurrentStep changing from {CurrentStep} to 3");
        CurrentStep = 3;
        StatusMessage = "Review label details";
        _logger.LogInfo($"CurrentStep is now: {CurrentStep}");

        // Trigger Step3 to reload data
        if (_step3ViewModel != null)
        {
            _logger.LogInfo("Calling Step3ViewModel.LoadReviewData()");
            _step3ViewModel.LoadReviewData();
        }
        else
        {
            _logger.LogWarning("Step3ViewModel not yet initialized");
        }
    }

    /// <summary>
    /// Register Step 3 ViewModel for data refresh coordination
    /// </summary>
    /// <param name="step3ViewModel"></param>
    public void RegisterStep3ViewModel(RoutingWizardStep3ViewModel step3ViewModel)
    {
        _step3ViewModel = step3ViewModel;
        _logger.LogInfo("Step3ViewModel registered with Container");
    }

    /// <summary>
    /// Navigate back to Step 1
    /// </summary>
    [RelayCommand]
    private void NavigateToStep1()
    {
        IsEditingFromReview = false;
        CurrentStep = 1;
        StatusMessage = "Enter PO or select OTHER";
    }

    /// <summary>
    /// Navigate to Step 1 for editing (from Step 3)
    /// </summary>
    [RelayCommand]
    private void NavigateToStep1ForEdit()
    {
        _logger.LogInfo("NavigateToStep1ForEdit called - Setting edit mode");
        IsEditingFromReview = true;
        CurrentStep = 1;
        StatusMessage = "Edit PO selection";
    }

    /// <summary>
    /// Navigate back to Step 2
    /// </summary>
    [RelayCommand]
    private void NavigateBackToStep2()
    {
        IsEditingFromReview = false;
        CurrentStep = 2;
        StatusMessage = "Select recipient";
    }

    /// <summary>
    /// Navigate to Step 2 for editing (from Step 3)
    /// </summary>
    [RelayCommand]
    private void NavigateToStep2ForEdit()
    {
        _logger.LogInfo("NavigateToStep2ForEdit called - Setting edit mode");
        IsEditingFromReview = true;
        CurrentStep = 2;
        StatusMessage = "Edit recipient selection";
    }
    #endregion

    #region Create Label Command
    /// <summary>
    /// Create the routing label (final step)
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
            StatusMessage = "Creating label...";

            // Build label model
            var label = new Model_RoutingLabel
            {
                PONumber = SelectedPOLine?.PONumber ?? "OTHER",
                LineNumber = SelectedPOLine?.LineNumber.ToString() ?? "0",
                Description = SelectedPOLine?.Description ?? SelectedOtherReason?.Description ?? string.Empty,
                RecipientId = SelectedRecipient?.Id ?? 0,
                Quantity = FinalQuantity,
                OtherReasonId = SelectedOtherReason?.Id,
                CreatedBy = GetCurrentEmployeeNumber(),
                CreatedDate = DateTime.Now,
                WorkOrder = SelectedPOLine?.WorkOrder ?? string.Empty,
                CustomerOrder = SelectedPOLine?.CustomerOrder ?? string.Empty
            };

            // Create label via service
            var result = await _routingService.CreateLabelAsync(label);

            if (result.IsSuccess)
            {
                // Increment usage tracking for personalization
                await _usageTrackingService.IncrementUsageCountAsync(
                    label.CreatedBy,
                    label.RecipientId);

                StatusMessage = "Label created successfully!";

                // Log success
                await _logger.LogInfoAsync(
                    $"Routing label created: PO={label.PONumber}, Recipient={SelectedRecipient?.Name}",
                    context: nameof(CreateLabelAsync));

                // Issue #13: Navigate back to Mode Selection after successful creation
                // Requires: INavigationService.NavigateTo(typeof(RoutingModeSelectionViewModel))
                // Or: Raise NavigationRequested event handled by parent window
                // Priority: LOW - User can manually navigate, auto-return is convenience feature
                // Navigation flow not yet implemented
                ResetWizard();
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage,
                    "Create Label Failed",
                    nameof(CreateLabelAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(CreateLabelAsync),
                nameof(RoutingWizardContainerViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Cancel Command
    /// <summary>
    /// Cancel wizard with confirmation
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        // Show confirmation dialog
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Cancel Wizard",
            Content = "Are you sure you want to cancel? All progress will be lost.",
            PrimaryButtonText = "Yes, cancel",
            CloseButtonText = "No, continue",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close
        };

        // Set XamlRoot for dialog
        dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            // User confirmed cancel
            ResetWizard();
            // Navigate back to Mode Selection after cancel
            // Requires: INavigationService integration or parent window event handling
            // Priority: LOW - Manual navigation available
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Reset wizard to initial state
    /// </summary>
    private void ResetWizard()
    {
        CurrentStep = 1;
        SelectedPOLine = null;
        SelectedOtherReason = null;
        SelectedRecipient = null;
        OtherQuantity = 1;
        FinalQuantity = 0;
        StatusMessage = "Enter PO or select OTHER";
    }

    /// <summary>
    /// Get current employee number from session
    /// Issue #7: Implemented using IService_UserSessionManager
    /// </summary>
    private int GetCurrentEmployeeNumber()
    {
        return _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
    }
    #endregion
}
