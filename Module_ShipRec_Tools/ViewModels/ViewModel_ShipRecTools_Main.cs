using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Main workflow container ViewModel for the ShipRec Tools module.
/// Controls which view (tool selection or a specific tool) is currently displayed.
/// Mirrors the pattern of ViewModel_Receiving_Workflow.
/// </summary>
public partial class ViewModel_ShipRecTools_Main : ViewModel_Shared_Base
{
    private readonly IService_ShipRecTools_Navigation _navigationService;

    [ObservableProperty]
    private string _currentToolTitle = "Ship/Rec Tools";

    [ObservableProperty]
    private bool _isToolSelectionVisible = true;

    [ObservableProperty]
    private bool _isOutsideServiceHistoryVisible;

    public ViewModel_ShipRecTools_Main(
        IService_ShipRecTools_Navigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        _navigationService = navigationService;
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
                    break;

                default:
                    _logger.LogInfo($"Unknown tool key '{toolKey}' - returning to tool selection.");
                    ShowToolSelection();
                    break;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(NavigateToTool), nameof(ViewModel_ShipRecTools_Main));
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
        _logger.LogInfo("Returned to ShipRec tool selection.");
    }

    private void HideAllViews()
    {
        IsToolSelectionVisible = false;
        IsOutsideServiceHistoryVisible = false;
    }
}
