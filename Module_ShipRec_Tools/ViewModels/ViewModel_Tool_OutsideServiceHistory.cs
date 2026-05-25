using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

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
    [NotifyPropertyChangedFor(nameof(CanAggregateVendorNames))]
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
    [NotifyPropertyChangedFor(nameof(CanAggregateVendorNames))]
    private bool _aggregateVendorNames;

    [ObservableProperty]
    private ObservableCollection<Model_OutsideServiceHistory> _results = new();

    private List<Model_OutsideServiceHistory> _sourceResults = new();
    private List<Model_OutsideServiceHistory> _allResults = new();
    private string _sortPropertyName = string.Empty;
    private bool _sortAscending = true;

    // ─── Fuzzy Picker Bridge ────────────────────────────────────────────────

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can request a fuzzy-picker dialog
    /// without taking a hard dependency on UI types.
    /// Parameters: (candidates, dialogTitle) → selected item, or null if cancelled.
    /// </summary>
    public Func<
        IReadOnlyList<Model_FuzzySearchResult>,
        string,
        Task<Model_FuzzySearchResult?>
    >? ShowFuzzyPickerAsync { get; set; }

    /// <summary>
    /// Set by the view's code-behind so the ViewModel can request column sort indicators to reset
    /// whenever new results are loaded or the search is cleared.
    /// </summary>
    public Action? ResetSortIndicators { get; set; }

    public bool CanAggregateVendorNames => true;

    partial void OnAggregateVendorNamesChanged(bool value)
    {
        if (_sourceResults.Count == 0)
        {
            return;
        }

        RefreshDisplayedResults();
    }

    // ─── Constructor ────────────────────────────────────────────────────────

    public ViewModel_Tool_OutsideServiceHistory(
        IService_Tool_OutsideServiceHistory service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    /// <summary>
    /// Shows the shared guidance message for the Outside Service History tool when it becomes active.
    /// </summary>
    public void ActivateView()
    {
        ShowStatus(
            IsSearchByPartMode
                ? "Enter a part number and click Search."
                : "Enter a vendor name and click Search.",
            InfoBarSeverity.Informational
        );
    }

    // ─── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    private void SetSearchByPart()
    {
        IsSearchByPartMode = true;
        SearchTerm = string.Empty;
        _sourceResults.Clear();
        _allResults.Clear();
        ReplaceResults(Array.Empty<Model_OutsideServiceHistory>());
        ShowStatus("Enter a part number and click Search.");
    }

    [RelayCommand]
    private void SetSearchByVendor()
    {
        IsSearchByPartMode = false;
        SearchTerm = string.Empty;
        _sourceResults.Clear();
        _allResults.Clear();
        ReplaceResults(Array.Empty<Model_OutsideServiceHistory>());
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
                nameof(SearchAsync)
            );
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
            if (IsSearchByVendor && AggregateVendorNames)
            {
                candidates = AggregateVendorCandidates(candidates);
            }

            if (candidates.Count == 0)
            {
                ShowStatus(
                    $"No {(IsSearchByPartMode ? "parts" : "vendors")} found matching '{SearchTerm}'.",
                    InfoBarSeverity.Warning
                );
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
                nameof(ViewModel_Tool_OutsideServiceHistory)
            );
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
            _logger.LogWarning(
                $"Outside service history failed for part '{confirmedPart.Key}': {historyResult.ErrorMessage}"
            );
            return;
        }

        SetHistoryResults(historyResult.Data);

        var summary =
            Results.Count > 0
                ? $"Found {Results.Count} dispatch record(s) for part {confirmedPart.Label}."
                : $"No outside service history found for part {confirmedPart.Label}.";

        ShowStatus(summary);
        _logger.LogInfo(
            $"Outside service history: {Results.Count} records for part '{confirmedPart.Key}'"
        );
    }

    private async Task LoadHistoryByVendorAsync(Model_FuzzySearchResult confirmedVendor)
    {
        var vendorIds = ParseVendorIds(confirmedVendor.Key);
        if (vendorIds.Count == 0)
        {
            ShowStatus(
                "No vendor ID was available for the selected vendor.",
                InfoBarSeverity.Warning
            );
            return;
        }

        // Step 3 (vendor mode): fetch all distinct parts that vendor has serviced
        ShowStatus($"Loading parts serviced by '{confirmedVendor.Label}'…");

        var partsResult =
            vendorIds.Count == 1
                ? await _service.GetPartsByVendorAsync(vendorIds[0])
                : await _service.GetPartsByVendorsAsync(vendorIds);

        if (!partsResult.IsSuccess || partsResult.Data is null)
        {
            ShowStatus(partsResult.ErrorMessage, InfoBarSeverity.Warning);
            _logger.LogWarning(
                $"Parts query failed for vendor '{confirmedVendor.Key}': {partsResult.ErrorMessage}"
            );
            return;
        }

        var parts = partsResult.Data;

        if (parts.Count == 0)
        {
            ShowStatus(
                $"No parts found for vendor '{confirmedVendor.Label}'.",
                InfoBarSeverity.Warning
            );
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
                $"Select Part — {confirmedVendor.Label}"
            );

            if (picked is null)
            {
                ShowStatus("Search cancelled.", InfoBarSeverity.Informational);
                return;
            }

            selectedPart = picked;
        }

        // Step 5 (vendor mode): load dispatch records for that specific vendor + part
        ShowStatus(
            $"Loading dispatch history for {selectedPart.Label} at {confirmedVendor.Label}…"
        );

        var historyResult =
            vendorIds.Count == 1
                ? await _service.GetHistoryByVendorAndPartAsync(vendorIds[0], selectedPart.Key)
                : await _service.GetHistoryByVendorsAndPartAsync(vendorIds, selectedPart.Key);

        if (!historyResult.IsSuccess || historyResult.Data is null)
        {
            ShowStatus(historyResult.ErrorMessage, InfoBarSeverity.Warning);
            _logger.LogWarning(
                $"History failed for vendor '{confirmedVendor.Key}', part '{selectedPart.Key}': {historyResult.ErrorMessage}"
            );
            return;
        }

        SetHistoryResults(historyResult.Data);

        var summary =
            Results.Count > 0
                ? $"Found {Results.Count} dispatch record(s) for {selectedPart.Label} at {confirmedVendor.Label}."
                : $"No dispatch history found for {selectedPart.Label} at {confirmedVendor.Label}.";

        ShowStatus(summary);
        _logger.LogInfo(
            $"Outside service history: {Results.Count} records for vendor '{confirmedVendor.Key}', part '{selectedPart.Key}'"
        );
    }

    [RelayCommand]
    private void Clear()
    {
        SearchTerm = string.Empty;
        _sourceResults.Clear();
        _allResults.Clear();
        _sortPropertyName = string.Empty;
        _sortAscending = true;
        ResetSortIndicators?.Invoke();
        ReplaceResults(Array.Empty<Model_OutsideServiceHistory>());
        ShowStatus($"Enter a {(IsSearchByPartMode ? "part number" : "vendor name")} to search.");
    }

    private void SetHistoryResults(IEnumerable<Model_OutsideServiceHistory> results)
    {
        _sourceResults = results.Select(CloneHistoryRecord).ToList();
        _sortPropertyName = string.Empty;
        _sortAscending = true;
        ResetSortIndicators?.Invoke();
        RefreshDisplayedResults();
    }

    private void RefreshDisplayedResults()
    {
        _allResults = BuildDisplayResults(_sourceResults);

        if (string.IsNullOrEmpty(_sortPropertyName))
        {
            ReplaceResults(_allResults);
            return;
        }

        ApplySort();
    }

    private List<Model_OutsideServiceHistory> BuildDisplayResults(
        IReadOnlyList<Model_OutsideServiceHistory> results
    )
    {
        if (AggregateVendorNames is false)
        {
            return results.Select(CloneHistoryRecord).ToList();
        }

        return results
            .GroupBy(static row => BuildCombinedResultKey(row), StringComparer.Ordinal)
            .Select(group =>
            {
                if (group.Count() == 1)
                {
                    return CloneHistoryRecord(group.First());
                }

                var latestRecord = group
                    .OrderByDescending(static row => row.DispatchDate)
                    .ThenBy(static row => row.DispatchID, StringComparer.OrdinalIgnoreCase)
                    .First();
                var distinctVendorIds = group
                    .Select(static row => row.VendorID?.Trim() ?? string.Empty)
                    .Where(static vendorId => string.IsNullOrWhiteSpace(vendorId) is false)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new Model_OutsideServiceHistory
                {
                    PartNumber = latestRecord.PartNumber?.Trim(),
                    VendorID = distinctVendorIds.Count == 1 ? distinctVendorIds[0] : "Multiple",
                    VendorName = latestRecord.VendorName,
                    VendorCity = latestRecord.VendorCity,
                    VendorState = latestRecord.VendorState,
                    DispatchID = "Multiple",
                    DispatchDate = group.Max(static row => row.DispatchDate),
                    QuantitySent = group.Sum(static row => row.QuantitySent ?? 0m),
                    DispatchStatus = latestRecord.DispatchStatus,
                    IsCombinedRecord = true,
                };
            })
            .OrderBy(static row => row.PartNumber, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static row => row.VendorName, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(static row => row.DispatchDate)
            .ToList();
    }

    private static Model_OutsideServiceHistory CloneHistoryRecord(
        Model_OutsideServiceHistory source
    )
    {
        return new Model_OutsideServiceHistory
        {
            PartNumber = source.PartNumber,
            VendorID = source.VendorID,
            VendorName = source.VendorName,
            VendorCity = source.VendorCity,
            VendorState = source.VendorState,
            DispatchID = source.DispatchID,
            DispatchDate = source.DispatchDate,
            QuantitySent = source.QuantitySent,
            DispatchStatus = source.DispatchStatus,
            IsCombinedRecord = source.IsCombinedRecord,
        };
    }

    private static string BuildCombinedResultKey(Model_OutsideServiceHistory result)
    {
        var normalizedPartNumber = NormalizeCombinedKeyPart(result.PartNumber);
        var normalizedVendorName = NormalizeCombinedKeyPart(
            string.IsNullOrWhiteSpace(result.VendorName) ? result.VendorID : result.VendorName
        );

        return $"{normalizedPartNumber}|{normalizedVendorName}";
    }

    private static string NormalizeCombinedKeyPart(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
    }

    private static List<Model_FuzzySearchResult> AggregateVendorCandidates(
        IReadOnlyList<Model_FuzzySearchResult> candidates
    )
    {
        return candidates
            .Where(static candidate => string.IsNullOrWhiteSpace(candidate.Label) is false)
            .GroupBy(static candidate => candidate.Label.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var vendorIds = group
                    .Select(static candidate => candidate.Key?.Trim() ?? string.Empty)
                    .Where(static key => string.IsNullOrWhiteSpace(key) is false)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var locationDetails = group
                    .Select(static candidate => candidate.Detail?.Trim())
                    .Where(static detail => string.IsNullOrWhiteSpace(detail) is false)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(2)
                    .ToList();

                var detailPrefix =
                    vendorIds.Count == 1 ? "1 vendor ID" : $"{vendorIds.Count} vendor IDs";
                var detailSuffix =
                    locationDetails.Count == 0
                        ? string.Empty
                        : $" — {string.Join("; ", locationDetails)}";

                return new Model_FuzzySearchResult
                {
                    Key = string.Join("|", vendorIds),
                    Label = group.First().Label,
                    Detail = detailPrefix + detailSuffix,
                };
            })
            .OrderBy(static candidate => candidate.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ParseVendorIds(string? vendorKey)
    {
        return (vendorKey ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ─── Sorting ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Sorts <see cref="Results"/> by the named property, toggling direction on repeated calls.
    /// Returns <see langword="true"/> if now sorted ascending, <see langword="false"/> if descending.
    /// </summary>
    /// <param name="propertyName">The property name to sort by.</param>
    public bool SortBy(string propertyName)
    {
        if (_sortPropertyName == propertyName)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortPropertyName = propertyName;
            _sortAscending = true;
        }

        ApplySort();
        return _sortAscending;
    }

    private void ApplySort()
    {
        if (_allResults.Count == 0 || string.IsNullOrEmpty(_sortPropertyName))
        {
            return;
        }

        IEnumerable<Model_OutsideServiceHistory> sorted = _sortPropertyName switch
        {
            nameof(Model_OutsideServiceHistory.PartNumber) => _sortAscending
                ? _allResults.OrderBy(r => r.PartNumber)
                : _allResults.OrderByDescending(r => r.PartNumber),
            nameof(Model_OutsideServiceHistory.VendorID) => _sortAscending
                ? _allResults.OrderBy(r => r.VendorID)
                : _allResults.OrderByDescending(r => r.VendorID),
            nameof(Model_OutsideServiceHistory.VendorName) => _sortAscending
                ? _allResults.OrderBy(r => r.VendorName)
                : _allResults.OrderByDescending(r => r.VendorName),
            nameof(Model_OutsideServiceHistory.VendorCity) => _sortAscending
                ? _allResults.OrderBy(r => r.VendorCity)
                : _allResults.OrderByDescending(r => r.VendorCity),
            nameof(Model_OutsideServiceHistory.VendorState) => _sortAscending
                ? _allResults.OrderBy(r => r.VendorState)
                : _allResults.OrderByDescending(r => r.VendorState),
            nameof(Model_OutsideServiceHistory.DispatchID) => _sortAscending
                ? _allResults.OrderBy(r => r.DispatchID)
                : _allResults.OrderByDescending(r => r.DispatchID),
            nameof(Model_OutsideServiceHistory.DispatchDate) => _sortAscending
                ? _allResults.OrderBy(r => r.DispatchDate)
                : _allResults.OrderByDescending(r => r.DispatchDate),
            nameof(Model_OutsideServiceHistory.QuantitySent) => _sortAscending
                ? _allResults.OrderBy(r => r.QuantitySent)
                : _allResults.OrderByDescending(r => r.QuantitySent),
            nameof(Model_OutsideServiceHistory.DispatchStatus) => _sortAscending
                ? _allResults.OrderBy(r => r.DispatchStatus)
                : _allResults.OrderByDescending(r => r.DispatchStatus),
            _ => _allResults,
        };

        ReplaceResults(sorted);
    }

    private void ReplaceResults(IEnumerable<Model_OutsideServiceHistory> results)
    {
        Results = new ObservableCollection<Model_OutsideServiceHistory>(results);
    }
}
