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
using Microsoft.Extensions.Configuration;

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
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

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
        RoutingWizardContainerViewModel containerViewModel,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _inforVisualService = inforVisualService;
        _routingService = routingService;
        _errorHandler = errorHandler;
        _logger = logger;
        _containerViewModel = containerViewModel;
        _configuration = configuration;

        // Subscribe to container property changes to update button text
        _containerViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_containerViewModel.IsEditingFromReview))
            {
                OnPropertyChanged(nameof(IsEditingFromReview));
                OnPropertyChanged(nameof(NavigationButtonText));
            }
        };

        // Auto-fill PO number if using mock data
        InitializeAsync();
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
    [NotifyPropertyChangedFor(nameof(IsLineSelected))]
    [NotifyPropertyChangedFor(nameof(SelectedPOLineTitle))]
    [NotifyPropertyChangedFor(nameof(HasReferenceInfo))]
    private Model_InforVisualPOLine? _selectedPOLine;

    /// <summary>
    /// Helper property for binding visibility of details panel
    /// </summary>
    public bool IsLineSelected => SelectedPOLine != null;

    /// <summary>
    /// Title for the details panel
    /// </summary>
    public string SelectedPOLineTitle => SelectedPOLine != null 
        ? $"Line {SelectedPOLine.LineNumber} Specifications" 
        : string.Empty;

    /// <summary>
    /// Helper to show/hide reference info field
    /// </summary>
    public bool HasReferenceInfo => SelectedPOLine != null && !string.IsNullOrEmpty(SelectedPOLine.ReferenceInfo);

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

    /// <summary>
    /// Is user editing from review (Step 3)
    /// </summary>
    public bool IsEditingFromReview => _containerViewModel.IsEditingFromReview;

    /// <summary>
    /// Button text for navigation (changes when editing from review)
    /// </summary>
    public string NavigationButtonText => IsEditingFromReview ? "Back to Review" : "Next: Select Recipient";

    /// <summary>
    /// Controls visibility of the Reference column (hidden if empty for all rows)
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReferenceColumnWidth))]
    private bool _isReferenceColumnVisible;

    public Microsoft.UI.Xaml.GridLength ReferenceColumnWidth => _isReferenceColumnVisible ? new Microsoft.UI.Xaml.GridLength(100) : new Microsoft.UI.Xaml.GridLength(0);

    /// <summary>
    /// Controls visibility of the Specs column (hidden if empty for all rows)
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpecsColumnWidth))]
    private bool _isSpecsColumnVisible;

    public Microsoft.UI.Xaml.GridLength SpecsColumnWidth => _isSpecsColumnVisible ? new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) : new Microsoft.UI.Xaml.GridLength(0);
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
                    bool hasRefs = false;
                    bool hasSpecs = false;

                    foreach (var line in linesResult.Data)
                    {
                        PoLines.Add(line);
                        if (!string.IsNullOrEmpty(line.ReferenceInfo)) hasRefs = true;
                        if (!string.IsNullOrEmpty(line.SpecificationsPreview)) hasSpecs = true;
                    }

                    IsReferenceColumnVisible = hasRefs;
                    IsSpecsColumnVisible = hasSpecs;

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
            // Feature requires database table: routing_po_alternatives (id, reason_code, description, is_active)
            // Stored procedure: sp_routing_po_alternatives_get_active
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
    [RelayCommand(CanExecute = nameof(CanProceeDataTransferObjectstep2))]
    private void ProceeDataTransferObjectstep2()
    {
        try
        {
            _logger.LogInfo($"ProceeDataTransferObjectstep2 called - IsOtherMode: {IsOtherMode}, SelectedPOLine: {SelectedPOLine?.PartID ?? "null"}, SelectedOtherReason: {SelectedOtherReason?.Description ?? "null"}");

            // Update container with selected data
            if (IsOtherMode && SelectedOtherReason != null)
            {
                _containerViewModel.SelectedOtherReason = SelectedOtherReason;
                _containerViewModel.OtherQuantity = OtherQuantity;
                _containerViewModel.SelectedPOLine = null;
                _logger.LogInfo($"Updated container with OTHER reason: {SelectedOtherReason.Description}, Quantity: {OtherQuantity}");
            }
            else if (SelectedPOLine != null)
            {
                _containerViewModel.SelectedPOLine = SelectedPOLine;
                _containerViewModel.SelectedOtherReason = null;
                _logger.LogInfo($"Updated container with PO Line: {SelectedPOLine.PartID}, PO: {SelectedPOLine.PONumber}");
            }
            else
            {
                _logger.LogWarning("ProceeDataTransferObjectstep2: Neither PO line nor OTHER reason selected");
                StatusMessage = "Please select a PO line or OTHER reason";
                return;
            }

            // Navigate based on edit mode
            if (IsEditingFromReview)
            {
                _logger.LogInfo("Returning to Step 3 (Review) after edit");
                _containerViewModel.NavigateToStep3Command.Execute(null);
            }
            else
            {
                _logger.LogInfo("Executing NavigateToStep2Command");
                _containerViewModel.NavigateToStep2Command.Execute(null);
                _logger.LogInfo("NavigateToStep2Command executed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ProceeDataTransferObjectstep2: {ex.Message}", ex);
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Error,
                nameof(ProceeDataTransferObjectstep2),
                nameof(RoutingWizardStep1ViewModel));
        }
    }

    /// <summary>
    /// Can proceed to Step 2 if either PO line or OTHER reason is selected
    /// </summary>
    private bool CanProceeDataTransferObjectstep2()
    {
        var canProceed = (IsOtherMode && SelectedOtherReason != null) ||
               (!IsOtherMode && SelectedPOLine != null);
        _logger.LogInfo($"CanProceeDataTransferObjectstep2: {canProceed} - IsOtherMode: {IsOtherMode}, SelectedPOLine: {SelectedPOLine?.PartID ?? "null"}, SelectedOtherReason: {SelectedOtherReason?.Description ?? "null"}");
        return canProceed;
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
        _logger.LogInfo($"SelectedPOLine changed to: {value?.PartID ?? "null"} (PO: {value?.PONumber ?? "null"})");
        ProceeDataTransferObjectstep2Command.NotifyCanExecuteChanged();

        if (value != null)
        {
            StatusMessage = $"Selected: {value.PartID} - Click 'Next' to continue";
        }
    }

    partial void OnSelectedOtherReasonChanged(Model_RoutingOtherReason? value)
    {
        _logger.LogInfo($"SelectedOtherReason changed to: {value?.Description ?? "null"}");
        ProceeDataTransferObjectstep2Command.NotifyCanExecuteChanged();

        if (value != null)
        {
            StatusMessage = $"Selected: {value.Description} - Click 'Next' to continue";
        }
    }

    /// <summary>
    /// Initialize component - autofill PO if mock data enabled
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            var useMockData = _configuration.GetValue<bool>("AppSettings:UseInforVisualMockData");

            if (useMockData)
            {
                var defaultPO = _configuration.GetValue<string>("AppSettings:DefaultMockPONumber") ?? "PO-066868";
                await _logger.LogInfoAsync($"[MOCK DATA MODE] Auto-filling PO number: {defaultPO}");

                PoNumber = defaultPO;
                StatusMessage = $"[MOCK DATA] Auto-filled PO: {defaultPO}";

                // Auto-validate and load mock data
                await Task.Delay(500); // Small delay for UI update
                await ValidatePOAsync();
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error during initialization: {ex.Message}", ex);
        }
    }
    #endregion
}
