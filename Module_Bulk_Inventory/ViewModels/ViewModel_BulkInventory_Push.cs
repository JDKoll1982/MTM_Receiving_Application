using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
/// ViewModel for the full-screen automation overlay (Push view).
/// Drives the row-by-row Visual UI Automation loop and exposes progress state
/// for the overlay display.
/// </summary>
public partial class ViewModel_BulkInventory_Push : ViewModel_Shared_Base
{
    private readonly IService_VisualInventoryAutomation _automation;
    private readonly IService_MySQL_BulkInventory _bulkService;

    private CancellationTokenSource? _cts;

    // ── F6 skip flag (set by View's keyboard handler) ─────────────────────────
    private volatile bool _skipCurrentRow;

    /// <summary>
    /// Called by the View when the user presses F6 to skip the current row.
    /// </summary>
    public void RequestSkipCurrentRow() => _skipCurrentRow = true;

    // ── Raised when the automation loop finishes — tells Host to navigate to Summary ──
    /// <summary>
    /// Raised on completion (success or cancellation) so the Host can navigate to the
    /// Summary view. The payload is the completed rows collection.
    /// </summary>
    public Action<
        IReadOnlyList<Model_BulkInventoryTransaction>
    >? RequestNavigateToSummary { get; set; }

    // ── Observable state ──────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isAutomationRunning;

    [ObservableProperty]
    private string _overlayStatusMessage = "Preparing batch…";

    [ObservableProperty]
    private int _processedCount;

    [ObservableProperty]
    private int _totalCount;

    public ViewModel_BulkInventory_Push(
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

    // ── Commands ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Begins the consolidated push loop for <paramref name="rows"/>.
    /// Consolidates rows by (PartId, FromLocation, ToLocation, TransactionType), marks
    /// originals as <c>Consolidated</c>, then processes each consolidated row through Visual.
    /// </summary>
    /// <param name="rows">The current data-entry rows to consolidate and push.</param>
    [RelayCommand]
    private async Task StartPushAsync(ObservableCollection<Model_BulkInventoryTransaction> rows)
    {
        if (IsAutomationRunning)
            return;

        _logger.LogInfo($"Push: StartPushAsync called with {rows.Count} total rows.");

        // ── Consolidation ─────────────────────────────────────────────────────
        var consolidated = rows.Where(r => r.Status == Enum_BulkInventoryStatus.Pending)
            .GroupBy(r => (r.PartId, r.FromLocation, r.ToLocation, r.TransactionType))
            .Select(g =>
            {
                var first = g.First();
                // Mark non-first rows as consolidated so the Summary can report them.
                foreach (var dup in g.Skip(1))
                    dup.Status = Enum_BulkInventoryStatus.Consolidated;

                return new Model_BulkInventoryTransaction
                {
                    // Consolidated rows inherit the Id of the first source row so
                    // WriteAuditAsync / CompleteRowAsync can address the correct DB record.
                    Id = first.Id,
                    PartId = first.PartId,
                    FromLocation = first.FromLocation,
                    ToLocation = first.ToLocation,
                    Quantity = g.Sum(r => r.Quantity),
                    WorkOrder = first.WorkOrder,
                    TransactionType = first.TransactionType,
                    Status = Enum_BulkInventoryStatus.Pending,
                    CreatedByUser = first.CreatedByUser,
                    CreatedAt = first.CreatedAt,
                };
            })
            .ToList();

        if (consolidated.Count == 0)
        {
            _logger.LogInfo("Push: No Pending rows to process after consolidation.");
            ShowStatus("Nothing to push.", InfoBarSeverity.Informational);
            return;
        }

        _logger.LogInfo(
            $"Push: Consolidated to {consolidated.Count} unique row(s). Starting automation loop."
        );

        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        IsAutomationRunning = true;
        TotalCount = consolidated.Count;
        ProcessedCount = 0;

        // Capture the UI dispatcher before leaving the UI thread.
        var uiDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // ── Row loop ──────────────────────────────────────────────────────────
        // Do NOT pass _cts.Token to Task.Run — that would throw TaskCanceledException
        // if the token is already cancelled before the task starts.
        // Cancellation is checked inside the loop instead.
        await Task.Run(async () =>
        {
            try
            {
                foreach (var row in consolidated)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        row.Status = Enum_BulkInventoryStatus.Skipped;
                        continue;
                    }

                    // Audit the row as InProgress before sending any keystrokes.
                    if (row.Id > 0)
                        await _bulkService.WriteAuditAsync(row.Id);

                    // F6 skip check before executing.
                    if (_skipCurrentRow)
                    {
                        _skipCurrentRow = false;
                        row.Status = Enum_BulkInventoryStatus.Skipped;
                        if (row.Id > 0)
                            await _bulkService.CompleteRowAsync(
                                row.Id,
                                Enum_BulkInventoryStatus.Skipped
                            );
                        ProcessedCount++;
                        UpdateOverlayMessage(row, "Skipped (F6)");
                        continue;
                    }

                    UpdateOverlayMessage(row, "Processing…");

                    try
                    {
                        if (row.TransactionType == Enum_BulkInventoryTransactionType.Transfer)
                            await _automation.ExecuteTransferAsync(row, _cts.Token);
                        else
                            await _automation.ExecuteNewTransactionAsync(row, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        row.Status = Enum_BulkInventoryStatus.Skipped;
                    }
                    catch (Exception ex)
                    {
                        row.Status = Enum_BulkInventoryStatus.Failed;
                        row.ErrorMessage = ex.Message;
                        _logger.LogError($"Unhandled error on row: {ex.Message}");
                    }

                    if (row.Id > 0)
                        await _bulkService.CompleteRowAsync(row.Id, row.Status, row.ErrorMessage);

                    ProcessedCount++;
                    UpdateOverlayMessage(row, row.Status.ToString());

                    if (_cts.Token.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Push loop failed unexpectedly: {ex.Message}");
            }

            // Navigate to Summary on the UI thread — ContentFrame.Content must be set there.
            var completedRows = rows.ToList();
            uiDispatcher?.TryEnqueue(() =>
            {
                IsAutomationRunning = false;
                RequestNavigateToSummary?.Invoke(completedRows);
            });
        });
    }

    /// <summary>Cancels the in-progress automation loop.</summary>
    [RelayCommand]
    private void CancelPush()
    {
        _cts?.Cancel();
        OverlayStatusMessage = "Cancelling — finishing current row…";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateOverlayMessage(Model_BulkInventoryTransaction row, string statusText)
    {
        OverlayStatusMessage =
            $"[{ProcessedCount + 1}/{TotalCount}]  "
            + $"{row.PartId}  {row.FromLocation} → {row.ToLocation}  — {statusText}";
    }
}
