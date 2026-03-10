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
    private const int InitialRowCount = 50;

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
    [NotifyCanExecuteChangedFor(nameof(ClearAllCommand))]
    private bool _hasRows;

    /// <summary>True when at least one row contains a Part ID. Controls whether Push Batch is enabled.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PushBatchCommand))]
    private bool _hasDataRows;

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

        Rows.Clear();

        if (!string.IsNullOrWhiteSpace(username))
        {
            var result = await _bulkService.GetByUserAsync(username);
            if (result.IsSuccess && result.Data is not null)
            {
                foreach (var row in result.Data
                             .Where(r => r.Status == Enum_BulkInventoryStatus.Pending
                                      || r.Status == Enum_BulkInventoryStatus.Failed))
                {
                    Rows.Add(row);
                }

                HasInterruptedRows = result.Data.Any(r => r.Status == Enum_BulkInventoryStatus.Failed);
            }
        }

        // Pad with blank in-memory rows so the grid always starts with InitialRowCount rows.
        var warehouseCode = await GetWarehouseCodeAsync();
        while (Rows.Count < InitialRowCount)
            Rows.Add(CreateBlankRow(warehouseCode));

        HasRows = true;
        RefreshState();
    }

    private static Model_BulkInventoryTransaction CreateBlankRow(string warehouseCode) =>
        new()
        {
            Status = Enum_BulkInventoryStatus.Pending,
            TransactionType = Enum_BulkInventoryTransactionType.Transfer,
            Quantity = 1,
            ToWarehouse = warehouseCode,
        };

    /// <summary>Appends a blank in-memory row (not persisted to MySQL until push time).</summary>
    [RelayCommand]
    private async Task AddRowAsync()
    {
        var warehouseCode = await GetWarehouseCodeAsync();
        Rows.Add(CreateBlankRow(warehouseCode));
        HasRows = true;
    }

    /// <summary>Removes a row. If it has been saved to MySQL (Id > 0) it is deleted there first.</summary>
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
        RefreshState();
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
            HasValidationWarnings = false;

            var warehouseCode = await GetWarehouseCodeAsync();
            while (Rows.Count < InitialRowCount)
                Rows.Add(CreateBlankRow(warehouseCode));

            HasRows = true;
            RefreshState();
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

    // ── Per-cell validation ───────────────────────────────────────────────────

    /// <summary>
    /// Called by the View's LostFocus handlers when the user leaves a Part ID or Location cell.
    /// Performs an exact-match lookup first; if not found, opens the FuzzySearch picker;
    /// if fuzzy search also returns nothing, sets an inline validation message on the row.
    /// </summary>
    /// <param name="row">Row whose field is being validated.</param>
    /// <param name="field">One of: "PartId", "FromLocation", "ToLocation".</param>
    public async Task ValidateFieldAsync(Model_BulkInventoryTransaction row, string field)
    {
        if (IsBusy || XamlRoot is null)
            return;

        var value = field switch
        {
            "PartId" => row.PartId ?? string.Empty,
            "FromLocation" => row.FromLocation ?? string.Empty,
            "ToLocation" => row.ToLocation ?? string.Empty,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(value))
        {
            row.ValidationMessage = null;
            RefreshState();
            return;
        }

        var warehouseCode = await GetWarehouseCodeAsync();

        if (field == "PartId")
        {
            var exactCheck = await _inforVisual.PartExistsAsync(value);
            if (exactCheck.IsSuccess && exactCheck.Data)
            {
                row.ValidationMessage = null;
                RefreshState();
                return;
            }

            var searchResult = await _fuzzySearch.SearchPartsAsync(value);
            if (!searchResult.IsSuccess || searchResult.Data is null || searchResult.Data.Count == 0)
            {
                row.ValidationMessage = $"Part '{value}' not found in Infor Visual.";
                RefreshState();
                return;
            }

            var dialog = new Dialog_FuzzySearchPicker(
                searchResult.Data,
                "Part Not Found – Select a Match",
                subtitle: $"'{value}' was not found. Select the correct part or Cancel.")
            {
                XamlRoot = XamlRoot
            };

            var outcome = await dialog.ShowAsync();
            if (outcome == ContentDialogResult.Primary && dialog.SelectedResult is not null)
            {
                row.PartId = dialog.SelectedResult.Key;
                row.ValidationMessage = null;
            }
            else
            {
                row.ValidationMessage = $"Part '{value}' not found in Infor Visual.";
            }
        }
        else
        {
            var exactCheck = await _inforVisual.LocationExistsAsync(value, warehouseCode);
            if (exactCheck.IsSuccess && exactCheck.Data)
            {
                row.ValidationMessage = null;
                RefreshState();
                return;
            }

            var searchResult = await _fuzzySearch.SearchLocationsAsync(value, warehouseCode);
            if (!searchResult.IsSuccess || searchResult.Data is null || searchResult.Data.Count == 0)
            {
                row.ValidationMessage = $"Location '{value}' not found in warehouse {warehouseCode}.";
                RefreshState();
                return;
            }

            var dialog = new Dialog_FuzzySearchPicker(
                searchResult.Data,
                "Location Not Found – Select a Match",
                subtitle: $"'{value}' was not found. Select the correct location or Cancel.")
            {
                XamlRoot = XamlRoot
            };

            var outcome = await dialog.ShowAsync();
            if (outcome == ContentDialogResult.Primary && dialog.SelectedResult is not null)
            {
                if (field == "FromLocation")
                    row.FromLocation = dialog.SelectedResult.Key;
                else
                    row.ToLocation = dialog.SelectedResult.Key;

                row.ValidationMessage = null;
            }
            else
            {
                row.ValidationMessage = $"Location '{value}' not found in warehouse {warehouseCode}.";
            }
        }

        RefreshState();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private bool CanPushBatch() => HasDataRows && !HasValidationWarnings && !IsBusy;

    /// <summary>
    /// Saves any in-memory-only rows to MySQL then raises <see cref="RequestNavigateToPush"/>
    /// with only the rows that contain data (blank rows are excluded).
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPushBatch))]
    private async Task PushBatchAsync()
    {
        var dataRows = Rows.Where(IsRowWithData).ToList();
        if (dataRows.Count == 0)
        {
            ShowStatus("No data rows to push.", InfoBarSeverity.Warning);
            return;
        }

        // Persist any in-memory-only rows to MySQL before handing off to the Push view.
        foreach (var row in dataRows.Where(r => r.Id == 0))
        {
            var saveResult = await _bulkService.StartRowAsync(row);
            if (saveResult.IsSuccess)
                row.Id = saveResult.Data;
        }

        RequestNavigateToPush?.Invoke(new ObservableCollection<Model_BulkInventoryTransaction>(dataRows));
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
            RefreshState();
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
        var warehouseCode = await GetWarehouseCodeAsync();

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

            RefreshState();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GetWarehouseCodeAsync()
    {
        var setting = await _settings.GetSettingAsync(SettingsCategory, KeyWarehouseCode);
        return setting.IsSuccess && setting.Data?.Value is { Length: > 0 } v
            ? v
            : FallbackWarehouseCode;
    }

    private static bool IsRowWithData(Model_BulkInventoryTransaction row)
        => !string.IsNullOrWhiteSpace(row.PartId);

    private void RefreshState()
    {
        HasDataRows = Rows.Any(IsRowWithData);
        HasValidationWarnings = Rows.Any(r => IsRowWithData(r) && !string.IsNullOrEmpty(r.ValidationMessage));
    }
}


