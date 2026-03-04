using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Outside Service Provider History lookup tool.
/// Supports searching Infor Visual (READ-ONLY) by either Part Number or Vendor Name.
/// A fuzzy LIKE search is run first; if multiple candidates are found the view is asked to
/// show a <c>Dialog_FuzzySearchPicker</c> so the user can confirm the exact match, then the
/// full dispatch history is loaded for the confirmed key.
/// </summary>
public partial class ViewModel_Tool_OutsideServiceHistory : ViewModel_Shared_Base
{
    private readonly IService_Tool_OutsideServiceHistory _service;

    // ─── Search Mode ────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchLabel))]
    [NotifyPropertyChangedFor(nameof(SearchPlaceholder))]
    [NotifyPropertyChangedFor(nameof(IsSearchByPart))]
    [NotifyPropertyChangedFor(nameof(IsSearchByVendor))]
    private bool _isSearchByPartMode = true;

    /// <summary>True when searching by Part Number (default mode).</summary>
    public bool IsSearchByPart => IsSearchByPartMode;
    /// <summary>True when searching by Vendor Name.</summary>
    public bool IsSearchByVendor => !IsSearchByPartMode;

    public string SearchLabel => IsSearchByPartMode ? "Part Number:" : "Vendor Name:";
    public string SearchPlaceholder => IsSearchByPartMode ? "e.g. 21-28841" : "e.g. Acme Plating";

    // ─── Search Input & Results ─────────────────────────────────────────────

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_OutsideServiceHistory> _results = new();

    // ─── Fuzzy Picker Bridge ────────────────────────────────────────────────

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can request a fuzzy-picker dialog
    /// without taking a hard dependency on UI types.
    /// Parameters: (candidates, dialogTitle) → selected item, or null if cancelled.
    /// </summary>
    public Func<IReadOnlyList<Model_FuzzySearchResult>, string, Task<Model_FuzzySearchResult?>>? ShowFuzzyPickerAsync { get; set; }

    // ─── Constructor ────────────────────────────────────────────────────────

    public ViewModel_Tool_OutsideServiceHistory(
        IService_Tool_OutsideServiceHistory service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    // ─── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    private void SetSearchByPart()
    {
        IsSearchByPartMode = true;
        SearchTerm = string.Empty;
        Results.Clear();
        ShowStatus("Enter a part number and click Search.");
    }

    [RelayCommand]
    private void SetSearchByVendor()
    {
        IsSearchByPartMode = false;
        SearchTerm = string.Empty;
        Results.Clear();
        ShowStatus("Enter a vendor name and click Search.");
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await _errorHandler.ShowUserErrorAsync(
                $"Please enter a {(IsSearchByPartMode ? "part number" : "vendor name")} to search.",
                "Input Required",
                nameof(SearchAsync));
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus($"Searching for '{SearchTerm}'…");

            // Step 1: fuzzy search for candidates
            var fuzzyResult = IsSearchByPartMode
                ? await _service.FuzzySearchPartsAsync(SearchTerm)
                : await _service.FuzzySearchVendorsAsync(SearchTerm);

            if (!fuzzyResult.IsSuccess || fuzzyResult.Data is null)
            {
                ShowStatus(fuzzyResult.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            var candidates = fuzzyResult.Data;

            if (candidates.Count == 0)
            {
                ShowStatus($"No {(IsSearchByPartMode ? "parts" : "vendors")} found matching '{SearchTerm}'.", InfoBarSeverity.Warning);
                return;
            }

            // Step 2: if multiple hits, show picker so the user confirms the exact item
            Model_FuzzySearchResult confirmed;

            if (candidates.Count == 1)
            {
                confirmed = candidates[0];
            }
            else
            {
                var pickerTitle = IsSearchByPartMode ? "Select Part Number" : "Select Vendor";

                if (ShowFuzzyPickerAsync is null)
                {
                    confirmed = candidates[0];
                }
                else
                {
                    var picked = await ShowFuzzyPickerAsync(candidates, pickerTitle);
                    if (picked is null)
                    {
                        ShowStatus("Search cancelled.", InfoBarSeverity.Informational);
                        return;
                    }

                    confirmed = picked;
                }
            }

            if (IsSearchByPartMode)
            {
                await LoadHistoryByPartAsync(confirmed);
            }
            else
            {
                await LoadHistoryByVendorAsync(confirmed);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SearchAsync),
                nameof(ViewModel_Tool_OutsideServiceHistory));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadHistoryByPartAsync(Model_FuzzySearchResult confirmedPart)
    {
        ShowStatus($"Loading dispatch history for '{confirmedPart.Label}'…");

        var historyResult = await _service.GetHistoryByPartAsync(confirmedPart.Key);

        if (!historyResult.IsSuccess || historyResult.Data is null)
        {
            ShowStatus(historyResult.ErrorMessage, InfoBarSeverity.Warning);
            _logger.LogWarning($"Outside service history failed for part '{confirmedPart.Key}': {historyResult.ErrorMessage}");
            return;
        }

        Results.Clear();
        foreach (var item in historyResult.Data)
        {
            Results.Add(item);
        }

        var summary = Results.Count > 0
            ? $"Found {Results.Count} dispatch record(s) for part {confirmedPart.Label}."
            : $"No outside service history found for part {confirmedPart.Label}.";

        ShowStatus(summary);
        _logger.LogInfo($"Outside service history: {Results.Count} records for part '{confirmedPart.Key}'");
    }

    private async Task LoadHistoryByVendorAsync(Model_FuzzySearchResult confirmedVendor)
    {
        // Step 3 (vendor mode): fetch all distinct parts that vendor has serviced
        ShowStatus($"Loading parts serviced by '{confirmedVendor.Label}'…");

        var partsResult = await _service.GetPartsByVendorAsync(confirmedVendor.Key);

        if (!partsResult.IsSuccess || partsResult.Data is null)
        {
            ShowStatus(partsResult.ErrorMessage, InfoBarSeverity.Warning);
            _logger.LogWarning($"Parts query failed for vendor '{confirmedVendor.Key}': {partsResult.ErrorMessage}");
            return;
        }

        var parts = partsResult.Data;

        if (parts.Count == 0)
        {
            ShowStatus($"No parts found for vendor '{confirmedVendor.Label}'.", InfoBarSeverity.Warning);
            return;
        }

        // Step 4 (vendor mode): show part picker so user selects which part to drill into
        Model_FuzzySearchResult selectedPart;

        if (parts.Count == 1)
        {
            selectedPart = parts[0];
        }
        else if (ShowFuzzyPickerAsync is null)
        {
            selectedPart = parts[0];
        }
        else
        {
            var picked = await ShowFuzzyPickerAsync(
                parts,
                $"Select Part — {confirmedVendor.Label}");

            if (picked is null)
            {
                ShowStatus("Search cancelled.", InfoBarSeverity.Informational);
                return;
            }

            selectedPart = picked;
        }

        // Step 5 (vendor mode): load dispatch records for that specific vendor + part
        ShowStatus($"Loading dispatch history for {selectedPart.Label} at {confirmedVendor.Label}…");

        var historyResult = await _service.GetHistoryByVendorAndPartAsync(confirmedVendor.Key, selectedPart.Key);

        if (!historyResult.IsSuccess || historyResult.Data is null)
        {
            ShowStatus(historyResult.ErrorMessage, InfoBarSeverity.Warning);
            _logger.LogWarning($"History failed for vendor '{confirmedVendor.Key}', part '{selectedPart.Key}': {historyResult.ErrorMessage}");
            return;
        }

        Results.Clear();
        foreach (var item in historyResult.Data)
        {
            Results.Add(item);
        }

        var summary = Results.Count > 0
            ? $"Found {Results.Count} dispatch record(s) for {selectedPart.Label} at {confirmedVendor.Label}."
            : $"No dispatch history found for {selectedPart.Label} at {confirmedVendor.Label}.";

        ShowStatus(summary);
        _logger.LogInfo($"Outside service history: {Results.Count} records for vendor '{confirmedVendor.Key}', part '{selectedPart.Key}'");
    }

    [RelayCommand]
    private void Clear()
    {
        SearchTerm = string.Empty;
        Results.Clear();
        ShowStatus($"Enter a {(IsSearchByPartMode ? "part number" : "vendor name")} to search.");
    }
}

