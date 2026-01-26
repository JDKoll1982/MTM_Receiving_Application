using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Hub;

/// <summary>
/// Manages the mode selection display in the Receiving hub.
/// Provides mode descriptions and handles mode navigation.
/// </summary>
public partial class ViewModel_Receiving_Hub_Display_ModeSelection : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    /// <summary>
    /// Description of Wizard Mode workflow.
    /// </summary>
    [ObservableProperty]
    private string _wizardModeDescription = "Guided 3-step workflow for streamlined receiving";

    /// <summary>
    /// Description of Manual Entry Mode workflow.
    /// </summary>
    [ObservableProperty]
    private string _manualModeDescription = "Direct data grid entry for experienced users";

    /// <summary>
    /// Description of Edit Mode workflow.
    /// </summary>
    [ObservableProperty]
    private string _editModeDescription = "View and edit historical transactions";

    /// <summary>
    /// Indicates if Non-PO receiving mode is available.
    /// </summary>
    [ObservableProperty]
    private bool _isNonPOEnabled = true;

    /// <summary>
    /// Currently selected mode for visual highlighting.
    /// </summary>
    [ObservableProperty]
    private Enum_Receiving_Mode_WorkflowMode? _selectedMode = null;

    /// <summary>
    /// Indicates if mode selection is enabled (not during active workflow).
    /// </summary>
    [ObservableProperty]
    private bool _isModeSelectionEnabled = true;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the mode selection display ViewModel.
    /// </summary>
    public ViewModel_Receiving_Hub_Display_ModeSelection(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Mode Selection Display ViewModel initialized");
    }

    #endregion

    #region Commands

    /// <summary>
    /// Selects a workflow mode and triggers navigation event.
    /// </summary>
    /// <param name="mode">The workflow mode to select.</param>
    [RelayCommand]
    private void SelectMode(Enum_Receiving_Mode_WorkflowMode mode)
    {
        try
        {
            SelectedMode = mode;
            _logger.LogInfo($"Mode selected: {mode}");
            
            StatusMessage = mode switch
            {
                Enum_Receiving_Mode_WorkflowMode.Wizard => "Starting Wizard Mode...",
                Enum_Receiving_Mode_WorkflowMode.Manual => "Starting Manual Entry Mode...",
                Enum_Receiving_Mode_WorkflowMode.Edit => "Starting Edit Mode...",
                _ => "Unknown mode selected"
            };

            // Mode navigation will be handled by parent orchestration ViewModel
            // This ViewModel only manages the display state
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(SelectMode),
                nameof(ViewModel_Receiving_Hub_Display_ModeSelection));
        }
    }

    /// <summary>
    /// Displays help information for a specific mode.
    /// </summary>
    /// <param name="mode">The workflow mode to show help for.</param>
    [RelayCommand]
    private void ShowModeHelp(Enum_Receiving_Mode_WorkflowMode mode)
    {
        try
        {
            var helpMessage = mode switch
            {
                Enum_Receiving_Mode_WorkflowMode.Wizard => GetWizardModeHelp(),
                Enum_Receiving_Mode_WorkflowMode.Manual => GetManualModeHelp(),
                Enum_Receiving_Mode_WorkflowMode.Edit => GetEditModeHelp(),
                _ => "Help not available for this mode."
            };

            _logger.LogInfo($"Displaying help for mode: {mode}");
            
            // Show help dialog (implementation will use ContentDialog in View)
            StatusMessage = $"Showing help for {mode} mode";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(ShowModeHelp),
                nameof(ViewModel_Receiving_Hub_Display_ModeSelection));
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets detailed help text for Wizard Mode.
    /// </summary>
    private static string GetWizardModeHelp()
    {
        return @"Wizard Mode - Guided 3-Step Workflow

Step 1: Order & Part Selection
- Enter PO Number (or enable Non-PO mode)
- Select Part Number with auto-padding
- Specify number of loads (1-99)

Step 2: Load Details Entry
- Enter Weight and Quantity for each load
- Enter Heat/Lot numbers
- Select Package Type and count
- Bulk copy feature available

Step 3: Review & Save
- Review all load details
- Save to database and CSV
- Print receiving labels

Best for: Standard receiving operations with multiple loads requiring validation.";
    }

    /// <summary>
    /// Gets detailed help text for Manual Entry Mode.
    /// </summary>
    private static string GetManualModeHelp()
    {
        return @"Manual Entry Mode - Direct Data Grid Entry

Features:
- Single-screen workflow
- Quick entry form at top
- Live transaction grid below
- Add loads one at a time
- Edit/delete loads before saving

Best for: Experienced users who prefer direct data entry without step-by-step guidance.

Note: All validation still applies. Quality Hold warnings will appear as needed.";
    }

    /// <summary>
    /// Gets detailed help text for Edit Mode.
    /// </summary>
    private static string GetEditModeHelp()
    {
        return @"Edit Mode - Transaction History Management

Features:
- Search completed transactions
- Filter by date range, PO, Part
- View transaction details
- Edit load information
- Reprint labels

Best for: Correcting mistakes in completed transactions or reprinting labels.

Warning: Editing completed transactions creates audit trail entries.";
    }

    #endregion
}
