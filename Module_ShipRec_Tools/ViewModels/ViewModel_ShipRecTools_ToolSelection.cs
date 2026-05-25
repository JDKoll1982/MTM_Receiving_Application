using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the tool selection screen.
/// Loads the available tool cards from the navigation service.
/// </summary>
public partial class ViewModel_ShipRecTools_ToolSelection : ViewModel_Shared_Base
{
    private readonly IService_ShipRecTools_Navigation _navigationService;

    [ObservableProperty]
    private ObservableCollection<Model_ToolDefinition> _allTools = new();

    /// <summary>
    /// Raised when the user selects a tool. The string value is the ToolKey.
    /// </summary>
    public event Action<string>? ToolSelected;

    public ViewModel_ShipRecTools_ToolSelection(
        IService_ShipRecTools_Navigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        _navigationService = navigationService;
    }

    /// <summary>
    /// Shows the shared guidance message for the Ship/Rec tool selection screen.
    /// </summary>
    public void ActivateView()
    {
        ShowStatus("Choose a Ship/Rec tool to continue.", InfoBarSeverity.Informational);
    }

    /// <summary>
    /// Populates tool cards from the registry. Called when the screen becomes visible.
    /// </summary>
    public void LoadTools()
    {
        try
        {
            AllTools = new ObservableCollection<Model_ToolDefinition>(
                _navigationService.GetAllTools()
            );

            _logger.LogInfo($"Tool selection loaded: {AllTools.Count} available tools.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadTools),
                nameof(ViewModel_ShipRecTools_ToolSelection)
            );
        }
    }

    [RelayCommand]
    private void SelectTool(string toolKey)
    {
        if (string.IsNullOrWhiteSpace(toolKey))
        {
            return;
        }

        _logger.LogInfo($"Tool selected: {toolKey}");
        ToolSelected?.Invoke(toolKey);
    }
}
