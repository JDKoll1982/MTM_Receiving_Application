using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using InfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.ViewModels;

/// <summary>
/// ViewModel for the Bulk Inventory Data Entry grid.
/// Manages adding, deleting, validating, and routing rows to the Push view.
/// </summary>
public partial class ViewModel_BulkInventory_DataEntry : ViewModel_Shared_Base
{
    private readonly IService_MySQL_BulkInventory _bulkService;
    private readonly IService_BulkInventory_FuzzySearch _fuzzySearch;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_SettingsCoreFacade _settings;

    private const string SettingsCategory = "BulkInventory";
    private const string KeyWarehouseCode = "BulkInventory.Defaults.WarehouseCode";
    private const string FallbackWarehouseCode = "002";

    // ── XamlRoot needed to host ContentDialogs ──────────────────────────────
    /// <summary>
    /// Set by the View immediately after instantiation so <see cref="Dialog_FuzzySearchPicker"/>
    /// can be hosted correctly.
    /// </summary>
    public XamlRoot? XamlRoot { get; set; }

    /// <summary>
    /// Raised by <see cref="PushBatchCommand"/> to tell the Host to navigate to the Push view.
    /// The handler receives the current rows snapshot.
    /// </summary>
    public Action<ObservableCollection<Model_BulkInventoryTransaction>>? RequestNavigateToPush { get; set; }

    // ── Observable state ──────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PushBatchCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearAllCommand))]
    private ObservableCollection<Model_BulkInventoryTransaction> _rows = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PushBatchCommand))]
    private bool _hasValidationWarnings;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PushBatchCommand))]
    private bool _hasRows;

    /// <summary>
    /// Set to <see langword="true"/> when stale InProgress rows from a previous session are
    /// detected on startup (T030). Drives the interrupted-batch warning banner.
    /// </summary>
    [ObservableProperty]
    private bool _hasInterruptedRows;

    public ViewModel_BulkInventory_DataEntry(
        IService_MySQL_BulkInventory bulkService,
        IService_BulkInventory_FuzzySearch fuzzySearch,
        IService_UserSessionManager sessionManager,
        IService_InforVisual inforVisual,
        IService_SettingsCoreFacade settings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        _bulkService = bulkService;
        _fuzzySearch = fuzzySearch;
        _sessionManager = sessionManager;
        _inforVisual = inforVisual;
        _settings = settings;
    }

    // ── Row management ────────────────────────────────────────────────────────

    /// <summary>
    /// Loads all rows belonging to the current user from MySQL.
    /// Sets <see cref="HasInterruptedRows"/> when any Failed rows are found from a
    /// previous session (T030 crash-recovery banner).
    /// Call this once after the Host navigates to this view.
    /// </summary>
    public async Task LoadPendingRowsAsync()
    {
        var username = _sessionManager.CurrentSession?.User?.WindowsUsername;
        if (string.IsNullOrWhiteSpace(username))
            return;

        var result = await _bulkService.GetByUserAsync(username);
        if (!result.IsSuccess || result.Data is null)
            return;

        Rows.Clear();
        foreach (var row in result.Data
                     .Where(r => r.Status == Enum_BulkInventoryStatus.Pending
                              || r.Status == Enum_BulkInventoryStatus.Failed))
        {
            Rows.Add(row);
        }

        HasRows = Rows.Count > 0;
        HasInterruptedRows = result.Data.Any(r => r.Status == Enum_BulkInventoryStatus.Failed);
        RefreshValidationWarnings();
    }

    /// <summary>Appends a blank Pending row and persists it to MySQL.</summary>
    [RelayCommand]
    private async Task AddRowAsync()
    {
        var warehouseSetting = await _settings.GetSettingAsync(SettingsCategory, KeyWarehouseCode);
        var warehouseCode = warehouseSetting.IsSuccess
            && warehouseSetting.Data?.Value is { Length: > 0 } v
            ? v
            : FallbackWarehouseCode;

        var row = new Model_BulkInventoryTransaction
        {
            Status = Enum_BulkInventoryStatus.Pending,
            TransactionType = Enum_BulkInventoryTransactionType.Transfer,
            Quantity = 1,
            ToWarehouse = warehouseCode,
            CreatedAt = DateTime.Now,
            CreatedByUser = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "SYSTEM"
        };

        var result = await _bulkService.StartRowAsync(row);
        if (!result.IsSuccess)
        {
            await _errorHandler.ShowUserErrorAsync(result.ErrorMessage, "Add Row Error", nameof(AddRowAsync));
            return;
        }

        row.Id = result.Data;
        Rows.Add(row);
        HasRows = true;
        RefreshValidationWarnings();
    }

    /// <summary>Deletes a single row from MySQL and removes it from the grid.</summary>
    /// <param name="row">The row to delete.</param>
    [RelayCommand]
    private async Task DeleteRowAsync(Model_BulkInventoryTransaction row)
    {
        if (row.Id > 0)
        {
            var result = await _bulkService.DeleteByIdAsync(row.Id);
            if (!result.IsSuccess)
            {
                await _errorHandler.ShowUserErrorAsync(result.ErrorMessage, "Delete Row Error", nameof(DeleteRowAsync));
                return;
            }
        }

        Rows.Remove(row);
        HasRows = Rows.Count > 0;
        RefreshValidationWarnings();
    }

    private bool CanClearAll() => HasRows && !IsBusy;

    /// <summary>Prompts for confirmation then deletes all rows from MySQL and clears the grid.</summary>
    [RelayCommand(CanExecute = nameof(CanClearAll))]
    private async Task ClearAllAsync()
    {
        if (IsBusy || XamlRoot is null)
            return;

        var dialog = new ContentDialog
        {
            Title = "Clear All Rows?",
            Content = "This will delete all rows from the batch. This cannot be undone.",
            PrimaryButtonText = "Clear",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        try
        {
            IsBusy = true;
            foreach (var row in Rows.ToList())
            {
                if (row.Id > 0)
                    await _bulkService.DeleteByIdAsync(row.Id);
            }

            Rows.Clear();
            HasRows = false;
            HasValidationWarnings = false;
            ShowStatus("All rows cleared.", InfoBarSeverity.Informational);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(ClearAllAsync), nameof(ViewModel_BulkInventory_DataEntry));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// No-op stub — future implementation will skip the currently focused row without deleting it.
    /// Wired to F6 in the DataEntry keyboard handler.
    /// </summary>
    [RelayCommand]
    private void SkipCurrentRow() { }

    // ── Validation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates all Pending rows against Infor Visual using exact-match queries.
    /// Checks: required fields, PART existence, FromLocation and ToLocation existence.
    /// Surfaces warnings inline on each row; updates <see cref="HasValidationWarnings"/>.
    /// </summary>
    [RelayCommand]
    private async Task ValidateAllAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ShowStatus("Validating rows…", InfoBarSeverity.Informational);

            const string warehouseCode = "002";

            foreach (var row in Rows)
            {
                row.ValidationMessage = null;

                if (string.IsNullOrWhiteSpace(row.PartId))
                {
                    row.ValidationMessage = "Part ID is required.";
                    continue;
                }

                if (row.Quantity <= 0)
                {
                    row.ValidationMessage = "Quantity must be greater than zero.";
                    continue;
                }

                if (row.TransactionType == Enum_BulkInventoryTransactionType.Transfer
                    && string.IsNullOrWhiteSpace(row.ToLocation))
                {
                    row.ValidationMessage = "To Location is required for Transfer transactions.";
                    continue;
                }

                // Exact-match PART check against Infor Visual.
                var partCheck = await _inforVisual.PartExistsAsync(row.PartId);
                if (!partCheck.IsSuccess || !partCheck.Data)
                {
                    row.ValidationMessage = $"Part '{row.PartId}' not found in Infor Visual.";
                    continue;
                }

                // Exact-match location checks.
                if (!string.IsNullOrWhiteSpace(row.FromLocation))
                {
                    var locCheck = await _inforVisual.LocationExistsAsync(row.FromLocation, warehouseCode);
                    if (!locCheck.IsSuccess || !locCheck.Data)
                    {
                        row.ValidationMessage = $"From Location '{row.FromLocation}' not found in warehouse {warehouseCode}.";
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(row.ToLocation))
                {
                    var locCheck = await _inforVisual.LocationExistsAsync(row.ToLocation, warehouseCode);
                    if (!locCheck.IsSuccess || !locCheck.Data)
                    {
                        row.ValidationMessage = $"To Location '{row.ToLocation}' not found in warehouse {warehouseCode}.";
                        continue;
                    }
                }
            }

            RefreshValidationWarnings();

            var warnCount = Rows.Count(r => !string.IsNullOrEmpty(r.ValidationMessage));
            if (warnCount > 0)
                ShowStatus($"Validation complete — {warnCount} warning(s).", InfoBarSeverity.Warning);
            else
                ShowStatus("All rows valid.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(ValidateAllAsync), nameof(ViewModel_BulkInventory_DataEntry));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private bool CanPushBatch() => HasRows && !HasValidationWarnings && !IsBusy;

    /// <summary>Raises <see cref="RequestNavigateToPush"/> so the Host navigates to the Push view.</summary>
    [RelayCommand(CanExecute = nameof(CanPushBatch))]
    private void PushBatch()
    {
        RequestNavigateToPush?.Invoke(Rows);
    }

    // ── Fuzzy-search pickers ──────────────────────────────────────────────────

    /// <summary>Opens the part fuzzy-search picker and writes the selection back to <paramref name="row"/>.</summary>
    /// <param name="row">The row whose <c>PartId</c> will be updated with the selected result.</param>
    [RelayCommand]
    private async Task OpenPartSearchAsync(Model_BulkInventoryTransaction row)
    {
        if (XamlRoot is null)
            return;

        var term = string.IsNullOrWhiteSpace(row.PartId) ? " " : row.PartId;
        var searchResult = await _fuzzySearch.SearchPartsAsync(term.Trim());
        if (!searchResult.IsSuccess || searchResult.Data is null || searchResult.Data.Count == 0)
        {
            ShowStatus("No matching parts found.", InfoBarSeverity.Warning);
            return;
        }

        var dialog = new Dialog_FuzzySearchPicker(
            searchResult.Data,
            "Select Part",
            subtitle: $"Matching parts for '{term.Trim()}':")
        {
            XamlRoot = XamlRoot
        };

        var outcome = await dialog.ShowAsync();
        if (outcome == ContentDialogResult.Primary && dialog.SelectedResult is not null)
        {
            row.PartId = dialog.SelectedResult.Key;
            RefreshValidationWarnings();
        }
    }

    /// <summary>
    /// Opens the location fuzzy-search picker for <paramref name="args"/>.
    /// <para><c>args.Field</c> must be <c>"FromLocation"</c> or <c>"ToLocation"</c>.</para>
    /// </summary>
    /// <param name="args">Tuple containing the target row and the field name (<c>"FromLocation"</c> or <c>"ToLocation"</c>).</param>
    [RelayCommand]
    private async Task OpenLocationSearchAsync((Model_BulkInventoryTransaction Row, string Field) args)
    {
        if (XamlRoot is null)
            return;

        var currentValue = args.Field == "FromLocation" ? args.Row.FromLocation : args.Row.ToLocation;
        var term = string.IsNullOrWhiteSpace(currentValue) ? " " : currentValue;
        const string warehouseCode = "002";

        var searchResult = await _fuzzySearch.SearchLocationsAsync(term.Trim(), warehouseCode);
        if (!searchResult.IsSuccess || searchResult.Data is null || searchResult.Data.Count == 0)
        {
            ShowStatus("No matching locations found.", InfoBarSeverity.Warning);
            return;
        }

        var dialog = new Dialog_FuzzySearchPicker(
            searchResult.Data,
            "Select Location")
        {
            XamlRoot = XamlRoot
        };

        var outcome = await dialog.ShowAsync();
        if (outcome == ContentDialogResult.Primary && dialog.SelectedResult is not null)
        {
            if (args.Field == "FromLocation")
                args.Row.FromLocation = dialog.SelectedResult.Key;
            else
                args.Row.ToLocation = dialog.SelectedResult.Key;

            RefreshValidationWarnings();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshValidationWarnings()
    {
        HasValidationWarnings = Rows.Any(r => !string.IsNullOrEmpty(r.ValidationMessage));
    }
}


