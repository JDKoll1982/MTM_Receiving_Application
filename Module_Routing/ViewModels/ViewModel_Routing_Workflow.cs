using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
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
    private readonly IService_RoutingWorkflow _workflowService;

    [ObservableProperty]
    private Enum_Routing_WorkflowStep _currentStep = Enum_Routing_WorkflowStep.ModeSelection;

    [ObservableProperty]
    private bool _isModeSelectionVisible;

    [ObservableProperty]
    private bool _isLabelEntryVisible;

    [ObservableProperty]
    private bool _isHistoryVisible;

    [ObservableProperty]
    private bool _isWizardVisible;

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
        IService_RoutingWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));

        _workflowService.StepChanged += OnWorkflowStepChanged;
    }

    /// <summary>
    /// Initializes the workflow - defaults to Mode Selection step.
    /// </summary>
    public async Task InitializeAsync()
    {
        _workflowService.ResetWorkflow();

        // Ensure visibility is updated even if step didn't change
        CurrentStep = _workflowService.CurrentStep;
        UpdateVisibility();
        UpdateStatusMessage();

        await Task.CompletedTask;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        CurrentStep = _workflowService.CurrentStep;
        UpdateVisibility();
        UpdateStatusMessage();
    }

    private void UpdateVisibility()
    {
        IsModeSelectionVisible = CurrentStep == Enum_Routing_WorkflowStep.ModeSelection;
        IsLabelEntryVisible = CurrentStep == Enum_Routing_WorkflowStep.LabelEntry;
        IsHistoryVisible = CurrentStep == Enum_Routing_WorkflowStep.History;
        IsWizardVisible = CurrentStep == Enum_Routing_WorkflowStep.Wizard;
    }

    private void UpdateStatusMessage()
    {
        switch (CurrentStep)
        {
            case Enum_Routing_WorkflowStep.ModeSelection:
                StatusMessage = "Select a mode to begin";
                break;
            case Enum_Routing_WorkflowStep.LabelEntry:
                StatusMessage = "Label Entry - Add routing labels to queue";
                break;
            case Enum_Routing_WorkflowStep.History:
                StatusMessage = "History - View archived routing labels";
                break;
            case Enum_Routing_WorkflowStep.Wizard:
                StatusMessage = "Wizard Mode - Step-by-step label creation";
                break;
        }
    }

    [RelayCommand]
    private async Task NavigateToLabelEntryAsync()
    {
        _workflowService.GoToStep(Enum_Routing_WorkflowStep.LabelEntry);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NavigateToHistoryAsync()
    {
        _workflowService.GoToStep(Enum_Routing_WorkflowStep.History);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NavigateToModeSelectionAsync()
    {
        _workflowService.GoToStep(Enum_Routing_WorkflowStep.ModeSelection);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task PrintLabelsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Exporting labels to CSV...";
            _logger.LogInfo("Starting label print/export workflow");

            // Get today's labels
            var todayLabelsResult = await _routingService.GetTodayLabelsAsync();
            if (!todayLabelsResult.IsSuccess || todayLabelsResult.Data == null || todayLabelsResult.Data.Count == 0)
            {
                await _errorHandler.HandleErrorAsync("No labels to print", Enum_ErrorSeverity.Warning, null, true);
                StatusMessage = "No labels to print";
                return;
            }

            // Export to CSV
            var exportResult = await _routingService.ExportToCSVAsync(todayLabelsResult.Data);
            if (!exportResult.IsSuccess)
            {
                await _errorHandler.HandleErrorAsync(exportResult.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error exporting CSV";
                return;
            }

            _logger.LogInfo($"CSV exported to: {exportResult.Data}");

            // Archive labels
            var archiveResult = await _historyService.ArchiveTodayToHistoryAsync();
            if (archiveResult.IsSuccess)
            {
                StatusMessage = $"Exported {archiveResult.Data} labels to CSV - Ready for printing";
                _logger.LogInfo($"Archived {archiveResult.Data} labels to history");

                await _errorHandler.HandleErrorAsync(
                    $"Exported {archiveResult.Data} labels to:\n{exportResult.Data}\n\nLabels archived to history.",
                    Enum_ErrorSeverity.Info,
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
                await _errorHandler.HandleErrorAsync(archiveResult.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error archiving labels";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error printing labels", Enum_ErrorSeverity.Error, ex, true);
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
            if (result.IsSuccess && result.Data != null)
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
