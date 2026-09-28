using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for searching PO line binary specs using weighted fuzzy ranking.
/// </summary>
public partial class ViewModel_Tool_POLineSpecSearch : ViewModel_Shared_Base
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> PoStatusAliases =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Firmed"] = ["F", "FIRMED"],
            ["Released"] = ["R", "RELEASED"],
            ["Closed"] = ["C", "CLOSED"],
            ["Cancelled/Void"] = ["V", "VOID", "CANCELLED", "CANCELED"],
            ["F"] = ["F", "FIRMED"],
            ["R"] = ["R", "RELEASED"],
            ["C"] = ["C", "CLOSED"],
            ["V"] = ["V", "VOID", "CANCELLED", "CANCELED"],
        };

    private readonly IService_Tool_POLineSpecSearch _service;
    private readonly IService_ShipRecToolsSettings _shipRecToolsSettings;
    private readonly List<Model_Tool_POLineSpecSearchResult> _allResults = [];
    private bool _isInitialized;
    private Model_Tool_POLineSpecSearchOptions _options = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private string _searchModeDisplay = Model_Tool_POLineSpecSearchOptions.DefaultSearchMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPoStatusFilter))]
    private string _poStatusFilterDisplay = "All";

    [ObservableProperty]
    private int _visibleLines = Model_Tool_POLineSpecSearchOptions.DefaultVisibleLines;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<Model_Tool_POLineSpecSearchResult> _results = new();

    public bool HasResults => Results.Count > 0;

    public bool HasPoStatusFilter => string.Equals(PoStatusFilterDisplay, "All", StringComparison.OrdinalIgnoreCase) is false;

    public Func<
        Model_Tool_POLineSpecSearchOptions,
        Task<Model_Tool_POLineSpecSearchOptions?>
    >? ShowOptionsDialogAsync { get; set; }

    public event Action<IReadOnlyCollection<string>>? VisibleColumnsChanged;

    public ViewModel_Tool_POLineSpecSearch(
        IService_Tool_POLineSpecSearch service,
        IService_ShipRecToolsSettings shipRecToolsSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(shipRecToolsSettings);
        _service = service;
        _shipRecToolsSettings = shipRecToolsSettings;
    }

    public void ActivateView()
    {
        _ = InitializeOptionsAsync();
        ShowStatus("Enter a binary/spec term and click Search.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task OpenOptionsAsync()
    {
        await InitializeOptionsAsync();

        if (ShowOptionsDialogAsync is null)
        {
            return;
        }

        var selectedOptions = await ShowOptionsDialogAsync(_options.Clone());
        if (selectedOptions is null)
        {
            return;
        }

        _options = NormalizeOptions(selectedOptions);
        SearchModeDisplay = _options.SearchMode;
        PoStatusFilterDisplay = GetPoStatusDisplay(_options);
        VisibleLines = _options.VisibleLines;
        VisibleColumnsChanged?.Invoke(_options.VisibleColumnKeys.ToList());

        var saveResult = await _shipRecToolsSettings.SavePOLineSpecSearchOptionsAsync(_options);
        if (!saveResult.IsSuccess)
        {
            ShowStatus(saveResult.ErrorMessage, InfoBarSeverity.Warning);
            return;
        }

        ApplyResultFilters();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await _errorHandler.ShowUserErrorAsync(
                "Please enter a binary/spec search term.",
                "Input Required",
                nameof(SearchAsync)
            );
            return;
        }

        try
        {
            IsBusy = true;
            await InitializeOptionsAsync();
            var trimmedTerm = SearchTerm.Trim();
            ShowStatus($"Searching PO line specs for '{trimmedTerm}'...");

            var result = await _service.SearchAsync(trimmedTerm, _options, 1000);
            if (!result.IsSuccess || result.Data is null)
            {
                ReplaceResults(Array.Empty<Model_Tool_POLineSpecSearchResult>());
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            _allResults.Clear();
            _allResults.AddRange(result.Data);
            ApplyResultFilters();

            var statusSuffix = HasPoStatusFilter
                ? $" (PO status filter: {PoStatusFilterDisplay})"
                : string.Empty;
            var modeSuffix = $" [mode: {SearchModeDisplay}]";
            ShowStatus(
                _allResults.Count > 0
                    ? $"Found {_allResults.Count} ranked match(es); showing {Results.Count}{statusSuffix}.{modeSuffix}"
                    : "No matching PO line specs were found.",
                _allResults.Count > 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SearchAsync),
                nameof(ViewModel_Tool_POLineSpecSearch)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        SearchTerm = string.Empty;
        _allResults.Clear();
        ReplaceResults(Array.Empty<Model_Tool_POLineSpecSearchResult>());
        ShowStatus("Enter a binary/spec term and click Search.", InfoBarSeverity.Informational);
    }

    private async Task InitializeOptionsAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        var loadedOptions = await _shipRecToolsSettings.GetPOLineSpecSearchOptionsAsync();
        _options = NormalizeOptions(loadedOptions);
        SearchModeDisplay = _options.SearchMode;
        PoStatusFilterDisplay = GetPoStatusDisplay(_options);
        VisibleLines = _options.VisibleLines;
        VisibleColumnsChanged?.Invoke(_options.VisibleColumnKeys.ToList());
        _isInitialized = true;
    }

    private void ApplyResultFilters()
    {
        IEnumerable<Model_Tool_POLineSpecSearchResult> query = _allResults;

        if (string.IsNullOrWhiteSpace(_options.PoStatusFilter) is false)
        {
            var normalizedStatus = _options.PoStatusFilter.Trim();
            query = query.Where(row => MatchesPoStatusFilter(row.PoStatus, normalizedStatus));
        }

        query = query.Take(_options.VisibleLines);
        ReplaceResults(query.ToList());
    }

    private static bool MatchesPoStatusFilter(string rowPoStatus, string selectedFilter)
    {
        if (string.IsNullOrWhiteSpace(rowPoStatus))
        {
            return false;
        }

        var normalizedRowStatus = rowPoStatus.Trim();

        if (PoStatusAliases.TryGetValue(selectedFilter, out var aliases) is false)
        {
            return normalizedRowStatus.Contains(selectedFilter, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var alias in aliases)
        {
            if (alias.Length == 1)
            {
                if (string.Equals(normalizedRowStatus, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                continue;
            }

            if (
                string.Equals(normalizedRowStatus, alias, StringComparison.OrdinalIgnoreCase)
                || normalizedRowStatus.Contains(alias, StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }

        return false;
    }

    private static Model_Tool_POLineSpecSearchOptions NormalizeOptions(
        Model_Tool_POLineSpecSearchOptions options
    )
    {
        var normalized = options.Clone();

        if (
            Model_Tool_POLineSpecSearchOptions.VisibleLinesOptions.Contains(normalized.VisibleLines)
            is false
        )
        {
            normalized.VisibleLines = Model_Tool_POLineSpecSearchOptions.DefaultVisibleLines;
        }

        var allowedColumns = Model_Tool_POLineSpecSearchOptions
            .AvailableColumnKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        normalized.VisibleColumnKeys = normalized
            .VisibleColumnKeys.Where(allowedColumns.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalized.VisibleColumnKeys.Count == 0)
        {
            normalized.VisibleColumnKeys = new HashSet<string>(
                Model_Tool_POLineSpecSearchOptions.DefaultVisibleColumnKeys,
                StringComparer.OrdinalIgnoreCase
            );
        }

        normalized.VisibleColumnKeys.Add("SpecExcerpt");

        normalized.SearchMode = Model_Tool_POLineSpecSearchOptions.SearchModeOptions.Contains(
            normalized.SearchMode,
            StringComparer.OrdinalIgnoreCase
        )
            ? Model_Tool_POLineSpecSearchOptions.SearchModeOptions.First(mode =>
                string.Equals(mode, normalized.SearchMode, StringComparison.OrdinalIgnoreCase)
            )
            : Model_Tool_POLineSpecSearchOptions.DefaultSearchMode;

        normalized.PoStatusFilter = normalized.PoStatusFilter?.Trim() ?? string.Empty;
        return normalized;
    }

    private static string GetPoStatusDisplay(Model_Tool_POLineSpecSearchOptions options)
    {
        return string.IsNullOrWhiteSpace(options.PoStatusFilter) ? "All" : options.PoStatusFilter;
    }

    private void ReplaceResults(IEnumerable<Model_Tool_POLineSpecSearchResult> rows)
    {
        Results = new ObservableCollection<Model_Tool_POLineSpecSearchResult>(rows);
    }
}
