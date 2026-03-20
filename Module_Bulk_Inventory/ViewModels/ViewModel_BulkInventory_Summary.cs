using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.ViewModels;

/// <summary>
/// ViewModel for the Bulk Inventory Summary view.
/// Displays outcome counts (Success / Failed / Skipped), lists failed rows,
/// and supports re-pushing selected failed rows without re-consolidating.
/// </summary>
public partial class ViewModel_BulkInventory_Summary : ViewModel_Shared_Base
{
    private readonly IService_VisualInventoryAutomation _automation;
    private readonly IService_MySQL_BulkInventory _bulkService;

    /// <summary>Raised by <see cref="DoneCommand"/> so the Host can navigate back to DataEntry.</summary>
    public Action? RequestNavigateToDataEntry { get; set; }

    // ── Counts ────────────────────────────────────────────────────────────────

    [ObservableProperty]
    private int _successCount;

    [ObservableProperty]
    private int _failedCount;

    [ObservableProperty]
    private int _skippedCount;

    // ── Failed rows (shown in checklist for re-push) ──────────────────────────

    [ObservableProperty]
    private ObservableCollection<Model_BulkInventoryTransaction> _failedRows = [];

    public ViewModel_BulkInventory_Summary(
        IService_VisualInventoryAutomation automation,
        IService_MySQL_BulkInventory bulkService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _automation = automation;
        _bulkService = bulkService;
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>
    /// Populates the summary from the completed rows list.
    /// Called by the Host immediately after navigation to this view.
    /// </summary>
    /// <param name="completedRows">The final row states after the automation loop has finished.</param>
    public void LoadResults(IReadOnlyList<Model_BulkInventoryTransaction> completedRows)
    {
        SuccessCount = completedRows.Count(r => r.Status == Enum_BulkInventoryStatus.Success);
        FailedCount = completedRows.Count(r => r.Status == Enum_BulkInventoryStatus.Failed);
        SkippedCount = completedRows.Count(r =>
            r.Status is Enum_BulkInventoryStatus.Skipped or Enum_BulkInventoryStatus.Consolidated
        );

        FailedRows.Clear();
        foreach (var row in completedRows.Where(r => r.Status == Enum_BulkInventoryStatus.Failed))
            FailedRows.Add(row);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Re-pushes the selected (IsSelectedForRepush) failed rows through Visual automation
    /// without consolidation — rows are pushed as-is.
    /// </summary>
    [RelayCommand]
    private async Task RePushSelectedAsync()
    {
        if (IsBusy)
            return;

        var selected = FailedRows.Where(r => r.IsSelectedForRepush).ToList();
        if (selected.Count == 0)
        {
            ShowStatus("No rows selected for re-push.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus($"Re-pushing {selected.Count} row(s)…", InfoBarSeverity.Informational);

            foreach (var row in selected)
            {
                row.Status = Enum_BulkInventoryStatus.InProgress;
                row.ErrorMessage = null;

                if (row.Id > 0)
                    await _bulkService.WriteAuditAsync(row.Id);

                try
                {
                    using var cts = new System.Threading.CancellationTokenSource();
                    if (row.TransactionType == Enum_BulkInventoryTransactionType.Transfer)
                        await _automation.ExecuteTransferAsync(row, cts.Token);
                    else
                        await _automation.ExecuteNewTransactionAsync(row, cts.Token);
                }
                catch (Exception ex)
                {
                    row.Status = Enum_BulkInventoryStatus.Failed;
                    row.ErrorMessage = ex.Message;
                    _logger.LogError($"Re-push failed for row {row.Id}: {ex.Message}");
                }

                if (row.Id > 0)
                    await _bulkService.CompleteRowAsync(row.Id, row.Status, row.ErrorMessage);
            }

            // Refresh counts.
            SuccessCount += selected.Count(r => r.Status == Enum_BulkInventoryStatus.Success);
            var stillFailed = selected
                .Where(r => r.Status == Enum_BulkInventoryStatus.Failed)
                .ToList();
            FailedCount = FailedRows.Count(r => r.Status == Enum_BulkInventoryStatus.Failed);

            // Remove successfully re-pushed rows from the failed list.
            foreach (var row in selected.Where(r => r.Status == Enum_BulkInventoryStatus.Success))
                FailedRows.Remove(row);

            ShowStatus(
                stillFailed.Count == 0
                    ? "All selected rows re-pushed successfully."
                    : $"{stillFailed.Count} row(s) still failed.",
                stillFailed.Count == 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RePushSelectedAsync),
                nameof(ViewModel_BulkInventory_Summary)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Navigates back to the Data Entry view for a new batch.</summary>
    [RelayCommand]
    private void Done()
    {
        RequestNavigateToDataEntry?.Invoke();
    }
}
