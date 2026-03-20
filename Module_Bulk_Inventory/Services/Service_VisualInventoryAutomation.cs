using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Services;

/// <summary>
/// Drives Infor Visual via UI Automation to process bulk inventory transactions.
/// All automation errors are captured as row <c>Failed</c> outcomes — no exceptions are thrown.
/// </summary>
/// <remarks>
/// Use <c>ExecuteTransferAsync</c> for <see cref="Enum_BulkInventoryTransactionType.Transfer"/>
/// rows and <c>ExecuteNewTransactionAsync</c> for
/// <see cref="Enum_BulkInventoryTransactionType.NewTransaction"/> rows.
/// </remarks>
public class Service_VisualInventoryAutomation : IService_VisualInventoryAutomation
{
    private readonly IService_UIAutomation _ui;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_MySQL_BulkInventory _bulkService;
    private readonly IService_SettingsCoreFacade _settings;

    /// <summary>
    /// True when this service opened the Inventory Transfers window during the current session.
    /// Set to false when an existing Visual session is reused.
    /// Used by <see cref="ExecuteNewTransactionAsync"/> to close a stale Transfer window.
    /// </summary>
    private bool _appOpenedTransferWindow;

    private const string SettingsCategory = "BulkInventory";
    private const string KeyWarehouseCode = "BulkInventory.Defaults.WarehouseCode";
    private const string KeyLotNo = "BulkInventory.Defaults.LotNo";

    private const string VisualClassName = "Gupta:AccFrame";
    private const string TransferWindowTitle = "Inventory Transfers - Infor VISUAL - MTMFG";
    private const string EntryWindowTitle = "Inventory Transaction Entry - Infor VISUAL - MTMFG";
    private const string EntryWindowShortTitle = "Inventory Transaction Entry";
    private const string PartsPopupTitle = "Parts";

    private static readonly TimeSpan PopupSettle = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan WindowLookup = TimeSpan.FromSeconds(5);

    public Service_VisualInventoryAutomation(
        IService_UIAutomation ui,
        IService_LoggingUtility logger,
        IService_MySQL_BulkInventory bulkService,
        IService_SettingsCoreFacade settings
    )
    {
        _ui = ui;
        _logger = logger;
        _bulkService = bulkService;
        _settings = settings;
    }

    // ── Settings helpers ──────────────────────────────────────────────────────

    private async Task<string> GetWarehouseCodeAsync()
    {
        var result = await _settings.GetSettingAsync(SettingsCategory, KeyWarehouseCode);
        return result.IsSuccess && result.Data?.Value is { Length: > 0 } v ? v : string.Empty;
    }

    private async Task<string> GetLotNoAsync()
    {
        var result = await _settings.GetSettingAsync(SettingsCategory, KeyLotNo);
        return result.IsSuccess && result.Data?.Value is { Length: > 0 } v ? v : "1";
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    private async Task<bool> TryDismissPartsPopupAsync(int rowId, CancellationToken ct)
    {
        var partsHwnd = await _ui.WaitForPopupAsync(
            VisualClassName,
            PartsPopupTitle,
            PopupSettle,
            pollMs: 100,
            ct: ct
        );

        if (partsHwnd == IntPtr.Zero)
        {
            return true;
        }

        if (!_ui.SetForegroundVerified(partsHwnd))
        {
            // [VISUAL_LOG]
            _logger.LogError(
                $"Visual: Could not acquire foreground on Parts popup for row {rowId}"
            );
            // [END_VISUAL_LOG]
            return false;
        }

        _ui.SendKeys("{UP}{ENTER}");
        await _ui.WaitForWindowToCloseAsync(
            VisualClassName,
            PartsPopupTitle,
            TimeSpan.FromSeconds(3),
            ct: ct
        );

        // [VISUAL_LOG]
        _logger.LogInfo($"Visual: Parts popup dismissed for row {rowId}");
        // [END_VISUAL_LOG]

        return true;
    }

    // ── T019 — Transfer fill sequence ─────────────────────────────────────────

    /// <summary>
    /// Fills the Inventory Transfers window in Visual for a Transfer-type row.
    /// All error paths mark the row as <see cref="Enum_BulkInventoryStatus.Failed"/>;
    /// no exceptions are thrown (PF-05).
    /// </summary>
    /// <param name="row">The transaction row to process.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExecuteTransferAsync(Model_BulkInventoryTransaction row, CancellationToken ct)
    {
        var warehouseCode = await GetWarehouseCodeAsync();

        var window = await _ui.FindWindowAsync(TransferWindowTitle, WindowLookup, ct);
        if (window is null)
        {
            const string reason = "Inventory Transfers window not found; Visual may not be open.";

            // [VISUAL_LOG]
            _logger.LogError($"Transfer: row {row.Id} Failed — {reason}");
            // [END_VISUAL_LOG]

            await _bulkService.CompleteRowAsync(row.Id, Enum_BulkInventoryStatus.Failed, reason);
            return;
        }

        // Step 1 — Part ID
        await _ui.FillFieldAsync(window, "4102", row.PartId, sendTab: false, ct);

        // [VISUAL_LOG]
        _logger.LogInfo($"Transfer: Part ID {row.PartId} filled for row {row.Id}");
        // [END_VISUAL_LOG]

        // Step 2 — Brief settle time for Visual to process
        await Task.Delay(1000, ct);

        // Step 3 — Poll for Parts lookup popup then dismiss
        if (!await TryDismissPartsPopupAsync(row.Id, ct))
        {
            await _bulkService.CompleteRowAsync(
                row.Id,
                Enum_BulkInventoryStatus.Failed,
                "Could not dismiss Parts popup during Transfer."
            );
            return;
        }

        // Step 4 — Quantity
        await _ui.FillFieldAsync(window, "4111", row.Quantity.ToString("G"), sendTab: false, ct);

        // Step 5 — From Warehouse
        await _ui.FillFieldAsync(window, "4123", warehouseCode, sendTab: false, ct);

        // Step 6 — From Location
        await _ui.FillFieldAsync(
            window,
            "4124",
            row.FromLocation ?? string.Empty,
            sendTab: false,
            ct
        );

        // Step 7 — To Warehouse
        await _ui.FillFieldAsync(window, "4142", warehouseCode, sendTab: false, ct);

        // Step 8 — To Location + TAB triggers duplicate check
        await _ui.FillFieldAsync(window, "4143", row.ToLocation, sendTab: true, ct);

        // Step 9 — Poll for duplicate transaction confirmation popup
        var confirmHwnd = await _ui.WaitForPopupAsync(
            VisualClassName,
            EntryWindowShortTitle,
            PopupSettle,
            pollMs: 100,
            ct: ct
        );

        if (confirmHwnd != IntPtr.Zero)
        {
            // [VISUAL_LOG]
            _logger.LogWarning(
                $"Transfer: WaitingForConfirmation — row {row.Id} awaiting user input"
            );
            // [END_VISUAL_LOG]

            await _bulkService.CompleteRowAsync(
                row.Id,
                Enum_BulkInventoryStatus.WaitingForConfirmation
            );

            try
            {
                await _ui.WaitForWindowToCloseAsync(
                    VisualClassName,
                    EntryWindowShortTitle,
                    TimeSpan.FromMinutes(5),
                    pollMs: 200,
                    ct: ct
                );
            }
            catch (OperationCanceledException)
            {
                const string reason = "Cancelled while waiting for duplicate confirmation popup.";

                // [VISUAL_LOG]
                _logger.LogError($"Transfer: row {row.Id} Failed — {reason}");
                // [END_VISUAL_LOG]

                await _bulkService.CompleteRowAsync(
                    row.Id,
                    Enum_BulkInventoryStatus.Failed,
                    reason
                );
                return;
            }
        }

        // [VISUAL_LOG]
        _logger.LogInfo($"Transfer: row {row.Id} completed successfully");
        // [END_VISUAL_LOG]

        await _bulkService.CompleteRowAsync(row.Id, Enum_BulkInventoryStatus.Success);
    }

    // ── T020 — New Transaction fill sequence ──────────────────────────────────

    /// <summary>
    /// Fills the Inventory Transaction Entry window in Visual for a NewTransaction-type row.
    /// Closes any stale Transfer window opened by this service during the session.
    /// </summary>
    /// <param name="row">The transaction row to process.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExecuteNewTransactionAsync(
        Model_BulkInventoryTransaction row,
        CancellationToken ct
    )
    {
        var warehouseCode = await GetWarehouseCodeAsync();
        var lotNo = row.LotNo ?? await GetLotNoAsync();

        var window = await _ui.FindWindowAsync(EntryWindowTitle, WindowLookup, ct);
        if (window is null)
        {
            const string reason =
                "Inventory Transaction Entry window not found; Visual may not be open.";

            // [VISUAL_LOG]
            _logger.LogError($"NewTransaction: row {row.Id} Failed — {reason}");
            // [END_VISUAL_LOG]

            await _bulkService.CompleteRowAsync(row.Id, Enum_BulkInventoryStatus.Failed, reason);
            return;
        }

        // Step 2 — Close stale Transfer window if this service opened it
        if (_appOpenedTransferWindow)
        {
            var transferHwnd = _ui.FindWindowByClassAndTitle(VisualClassName, TransferWindowTitle);
            if (transferHwnd != IntPtr.Zero)
            {
                NativeMethods.SendMessage(
                    transferHwnd,
                    NativeMethods.WM_CLOSE,
                    IntPtr.Zero,
                    IntPtr.Zero
                );
            }

            _appOpenedTransferWindow = false;
        }

        // Step 3 — Work Order + TAB
        await _ui.FillFieldAsync(window, "4115", row.WorkOrder ?? string.Empty, sendTab: true, ct);

        // Step 4 — Precautionary Parts popup check
        if (!await TryDismissPartsPopupAsync(row.Id, ct))
        {
            await _bulkService.CompleteRowAsync(
                row.Id,
                Enum_BulkInventoryStatus.Failed,
                "Could not dismiss Parts popup during NewTransaction."
            );
            return;
        }

        // Step 5 — Lot No + TAB
        await _ui.FillFieldAsync(window, "4116", lotNo, sendTab: true, ct);

        // Step 6 — Quantity (⚠️ AutomationId "4143" reused — different window from Transfer)
        await _ui.FillFieldAsync(window, "4143", row.Quantity.ToString("G"), sendTab: false, ct);

        // Step 7 — To Warehouse
        await _ui.FillFieldAsync(window, "4148", warehouseCode, sendTab: false, ct);

        // Step 8 — To Location + TAB
        await _ui.FillFieldAsync(window, "4152", row.ToLocation, sendTab: true, ct);

        // [VISUAL_LOG]
        _logger.LogInfo($"NewTransaction: row {row.Id} completed successfully");
        // [END_VISUAL_LOG]

        // Step 9 — Mark Success
        await _bulkService.CompleteRowAsync(row.Id, Enum_BulkInventoryStatus.Success);
    }

    /// <summary>Marks that the application opened the Transfer window (used to clean up on mode switch).</summary>
    public void SetTransferWindowOpened() => _appOpenedTransferWindow = true;

    // ── Win32 interop ─────────────────────────────────────────────────────────

    private static class NativeMethods
    {
        internal const int WM_CLOSE = 0x0010;

        [System.Runtime.InteropServices.DllImport(
            "user32.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Auto
        )]
        internal static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam
        );
    }
}
