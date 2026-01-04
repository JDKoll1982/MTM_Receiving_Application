using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// Workflow coordinator ViewModel for the routing module.
/// Manages navigation between Label Entry, Review, Print, and History steps.
/// </summary>
public partial class ViewModel_Routing_Workflow : ViewModel_Shared_Base
{
    private readonly IService_Routing _routingService;
    private readonly IService_Routing_History _historyService;

    [ObservableProperty]
    private Enum_Routing_WorkflowStep _currentStep = Enum_Routing_WorkflowStep.LabelEntry;

    [ObservableProperty]
    private ViewModel_Routing_LabelEntry? _labelEntryViewModel;

    [ObservableProperty]
    private ViewModel_Routing_History? _historyViewModel;

    [ObservableProperty]
    private int _labelCount;

    [ObservableProperty]
    private bool _canPrint;

    public ViewModel_Routing_Workflow(
        IService_Routing routingService,
        IService_Routing_History historyService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
    }

    /// <summary>
    /// Initializes the workflow - defaults to Label Entry step.
    /// </summary>
    public async Task InitializeAsync()
    {
        await NavigateToLabelEntryAsync();
    }

    [RelayCommand]
    private async Task NavigateToLabelEntryAsync()
    {
        CurrentStep = Enum_Routing_WorkflowStep.LabelEntry;
        StatusMessage = "Label Entry - Add routing labels to queue";
        _logger.LogInfo("Navigated to Label Entry step");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NavigateToHistoryAsync()
    {
        CurrentStep = Enum_Routing_WorkflowStep.History;
        StatusMessage = "History - View archived routing labels";
        _logger.LogInfo("Navigated to History step");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task PrintLabelsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Exporting labels to CSV...";
            _logger.LogInfo("Starting label print/export workflow");

            // Get today's labels
            var todayLabelsResult = await _routingService.GetTodayLabelsAsync();
            if (!todayLabelsResult.IsSuccess || todayLabelsResult.Data.Count == 0)
            {
                await _errorHandler.HandleErrorAsync("No labels to print", Core.Models.Enums.Enum_ErrorSeverity.Warning, null, true);
                StatusMessage = "No labels to print";
                return;
            }

            // Export to CSV
            var exportResult = await _routingService.ExportToCSVAsync(todayLabelsResult.Data);
            if (!exportResult.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(exportResult.Message, Core.Models.Enums.Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error exporting CSV";
                return;
            }

            _logger.LogInfo($"CSV exported to: {exportResult.Data}");

            // Archive labels
            var labelIds = todayLabelsResult.Data.Select(l => l.Id).ToList();
            var archiveResult = await _historyService.ArchiveLabelsAsync(labelIds);
            if (archiveResult.IsSuccess)
            {
                StatusMessage = $"Exported {archiveResult.Data} labels to CSV - Ready for printing";
                _logger.LogInfo($"Archived {archiveResult.Data} labels to history");
                
                await _errorHandler.HandleErrorAsync(
                    $"Exported {archiveResult.Data} labels to:\n{exportResult.Data}\n\nLabels archived to history.",
                    Core.Models.Enums.Enum_ErrorSeverity.Info,
                    null,
                    true);

                // Refresh label entry to clear today's queue
                if (LabelEntryViewModel != null)
                {
                    await LabelEntryViewModel.LoadTodayLabelsCommand.ExecuteAsync(null);
                }
            }
            else
            {
                await _errorHandler.HandleErrorAsync(archiveResult.Message, Core.Models.Enums.Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error archiving labels";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error printing labels", Core.Models.Enums.Enum_ErrorSeverity.Error, ex, true);
            StatusMessage = "Error printing labels";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateLabelCountAsync()
    {
        try
        {
            var result = await _routingService.GetTodayLabelsAsync();
            if (result.IsSuccess)
            {
                LabelCount = result.Data.Count;
                CanPrint = LabelCount > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating label count: {ex.Message}", ex);
        }
    }
}
