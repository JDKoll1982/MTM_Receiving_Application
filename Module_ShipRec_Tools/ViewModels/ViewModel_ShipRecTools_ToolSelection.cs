using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the tool selection screen.
/// Loads tool cards grouped by category from the navigation service.
/// </summary>
public partial class ViewModel_ShipRecTools_ToolSelection : ViewModel_Shared_Base
{
    private readonly IService_ShipRecTools_Navigation _navigationService;

    [ObservableProperty]
    private ObservableCollection<Model_ToolDefinition> _lookupTools = new();

    [ObservableProperty]
    private ObservableCollection<Model_ToolDefinition> _analysisTools = new();

    [ObservableProperty]
    private ObservableCollection<Model_ToolDefinition> _utilityTools = new();

    [ObservableProperty]
    private bool _hasLookupTools;

    [ObservableProperty]
    private bool _hasAnalysisTools;

    [ObservableProperty]
    private bool _hasUtilityTools;

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
    /// Populates tool collections from the registry. Called when the screen becomes visible.
    /// </summary>
    public void LoadTools()
    {
        try
        {
            LookupTools.Clear();
            AnalysisTools.Clear();
            UtilityTools.Clear();

            foreach (var tool in _navigationService.GetToolsByCategory(Enum_ToolCategory.Lookup))
            {
                LookupTools.Add(tool);
            }

            foreach (var tool in _navigationService.GetToolsByCategory(Enum_ToolCategory.Analysis))
            {
                AnalysisTools.Add(tool);
            }

            foreach (var tool in _navigationService.GetToolsByCategory(Enum_ToolCategory.Utilities))
            {
                UtilityTools.Add(tool);
            }

            HasLookupTools = LookupTools.Count > 0;
            HasAnalysisTools = AnalysisTools.Count > 0;
            HasUtilityTools = UtilityTools.Count > 0;

            _logger.LogInfo(
                $"Tool selection loaded: {LookupTools.Count} lookup, {AnalysisTools.Count} analysis, {UtilityTools.Count} utility tools."
            );
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
