using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Main workflow container ViewModel for the ShipRec Tools module.
/// Controls which view (tool selection or a specific tool) is currently displayed.
/// Mirrors the pattern of ViewModel_Receiving_Workflow.
/// </summary>
public partial class ViewModel_ShipRecTools_Main
    : ViewModel_Shared_Base,
        IViewModel_HeaderTitleProvider
{
    private readonly IService_ShipRecTools_Navigation _navigationService;
    private readonly IService_HeaderBackNavigation _headerBackNavigation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]
    private string _currentToolTitle = "Ship/Rec Tools";

    public string CurrentHeaderTitle => CurrentToolTitle;

    [ObservableProperty]
    private bool _isToolSelectionVisible = true;

    [ObservableProperty]
    private bool _isOutsideServiceHistoryVisible;

    [ObservableProperty]
    private bool _isMaterialAvailabilityBoardVisible;

    public ViewModel_ShipRecTools_Main(
        IService_ShipRecTools_Navigation navigationService,
        IService_HeaderBackNavigation headerBackNavigation,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(headerBackNavigation);
        _navigationService = navigationService;
        _headerBackNavigation = headerBackNavigation;
    }

    /// <summary>
    /// Navigates to a tool by its registered key.
    /// Called when the user clicks a tool card on the selection screen.
    /// </summary>
    /// <param name="toolKey">The unique key of the tool to activate.</param>
    public void NavigateToTool(string toolKey)
    {
        try
        {
            _logger.LogInfo($"Navigating to tool: {toolKey}");

            HideAllViews();

            var tool = _navigationService.GetToolByKey(toolKey);
            var toolTitle = tool?.Title ?? toolKey;

            switch (toolKey)
            {
                case "OutsideServiceHistory":
                    IsOutsideServiceHistoryVisible = true;
                    CurrentToolTitle = toolTitle;
                    ShowHeaderBackButton();
                    break;

                case "MaterialAvailabilityBoard":
                    IsMaterialAvailabilityBoardVisible = true;
                    CurrentToolTitle = toolTitle;
                    ShowHeaderBackButton();
                    break;

                default:
                    _logger.LogInfo($"Unknown tool key '{toolKey}' - returning to tool selection.");
                    ShowToolSelection();
                    break;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(NavigateToTool),
                nameof(ViewModel_ShipRecTools_Main)
            );
        }
    }

    /// <summary>
    /// Returns the user to the tool selection screen.
    /// </summary>
    [RelayCommand]
    private void ShowToolSelection()
    {
        HideAllViews();
        IsToolSelectionVisible = true;
        CurrentToolTitle = "Ship/Rec Tools";
        _headerBackNavigation.ClearBackAction();
        _logger.LogInfo("Returned to ShipRec tool selection.");
    }

    private void ShowHeaderBackButton()
    {
        _headerBackNavigation.RegisterBackAction(
            () =>
            {
                ShowToolSelection();
                return Task.CompletedTask;
            },
            "Back to Tools"
        );
    }

    private void HideAllViews()
    {
        IsToolSelectionVisible = false;
        IsOutsideServiceHistoryVisible = false;
        IsMaterialAvailabilityBoardVisible = false;
    }
}
