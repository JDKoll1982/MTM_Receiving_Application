using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Helpers;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the location and part-based material availability board.
/// </summary>
public partial class ViewModel_Tool_MaterialAvailabilityBoard : ViewModel_Shared_Base
{
    private const string DefaultWarehouseCode = "002";
    private const string MockModalLocationSearchTerm = "T-00";
    private const string MockModalPartSearchTerm = "MMF0005007";

    private readonly IService_Tool_MaterialAvailabilityBoard _service;
    private readonly IService_ShipRecToolsSettings _shipRecToolsSettings;
    private readonly IService_SharedLocationRange _locationRangeService;
    private readonly bool _isMockDataEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchLabel))]
    [NotifyPropertyChangedFor(nameof(SearchPlaceholder))]
    [NotifyPropertyChangedFor(nameof(IsSearchByLocation))]
    [NotifyPropertyChangedFor(nameof(IsSearchByPart))]
    [NotifyPropertyChangedFor(nameof(MockDataHintText))]
    [NotifyPropertyChangedFor(nameof(IsLocationRangeToggleVisible))]
    [NotifyPropertyChangedFor(nameof(IsLocationRangeInputVisible))]
    [NotifyPropertyChangedFor(nameof(IsSingleLocationSearchVisible))]
    private bool _isSearchByLocationMode = true;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    /// <summary>
    /// When on, the single warehouse-location box is swapped for the Start/Stop range inputs.
    /// Location mode only; switching to Part Number clears it.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocationRangeInputVisible))]
    [NotifyPropertyChangedFor(nameof(IsSingleLocationSearchVisible))]
    private bool _isLocationRangeEnabled;

    [ObservableProperty]
    private string _rangeStartLocation = string.Empty;

    [ObservableProperty]
    private string _rangeStopLocation = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCards))]
    [NotifyPropertyChangedFor(nameof(ShowFlatCards))]
    [NotifyPropertyChangedFor(nameof(ShowGroupedCards))]
    private ObservableCollection<Model_Tool_MaterialAvailabilityCard> _cards = new();

    /// <summary>
    /// Collapsible per-location sections. Populated only when the board was loaded from a
    /// location range; the flat <see cref="Cards"/> list holds the same cards for printing.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowFlatCards))]
    [NotifyPropertyChangedFor(nameof(ShowGroupedCards))]
    private ObservableCollection<Model_Tool_MaterialAvailabilityLocationGroup> _cardGroups = new();

    [ObservableProperty]
    private string _selectedLookAheadOption = "30";

    public Func<
        IReadOnlyList<Model_FuzzySearchResult>,
        string,
        Task<Model_FuzzySearchResult?>
    >? ShowFuzzyPickerAsync { get; set; }

    public Func<
        Model_FormattedReportDocument,
        Task<Model_Dao_Result<bool>>
    >? RequestPrintAsync { get; set; }

    public Func<Task<bool?>>? SelectLocationPrintModeAsync { get; set; }

    public Func<
        ViewModel_Dialog_MaterialAvailabilityIncomingDetails,
        Task
    >? ShowIncomingMaterialDetailsAsync { get; set; }

    public Func<
        ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails,
        Task
    >? ShowWorkOrderDetailsDialogAsync { get; set; }

    public IReadOnlyList<string> LookAheadOptions { get; } = ["30", "60", "90", "All"];

    public bool IsSearchByLocation => IsSearchByLocationMode;

    public bool IsSearchByPart => !IsSearchByLocationMode;

    public string SearchLabel => IsSearchByLocationMode ? "Warehouse Location:" : "Part Number:";

    public string SearchPlaceholder => IsSearchByLocationMode ? "e.g. RECV" : "e.g. MMC0000658";

    /// <summary>True when the range toggle should be offered (warehouse-location mode only).</summary>
    public bool IsLocationRangeToggleVisible => IsSearchByLocationMode;

    /// <summary>True when the Start/Stop range inputs replace the single location box.</summary>
    public bool IsLocationRangeInputVisible => IsSearchByLocationMode && IsLocationRangeEnabled;

    /// <summary>True when the single search box should be shown instead.</summary>
    public bool IsSingleLocationSearchVisible => !IsLocationRangeInputVisible;

    public string LookAheadLabel => "Look Ahead:";

    public bool HasCards => Cards.Count > 0;

    /// <summary>
    /// True when the board was loaded from a location range, so results are shown as one
    /// collapsible section per location instead of a flat card list.
    /// </summary>
    public bool ShowGroupedCards => CardGroups.Count > 0;

    /// <summary>True when the flat card list should be shown instead of the location sections.</summary>
    public bool ShowFlatCards => ShowGroupedCards is false && Cards.Count > 0;

    public bool IsMockDataHintVisible => _isMockDataEnabled;

    public string MockDataHintText =>
        _isMockDataEnabled is false ? string.Empty
        : IsSearchByLocationMode
            ? $"Mock data is enabled. Search location {MockModalLocationSearchTerm} to load a card that supports both detail windows."
        : $"Mock data is enabled. Search part {MockModalPartSearchTerm} to load a card that supports both detail windows.";

    public ViewModel_Tool_MaterialAvailabilityBoard(
        IService_Tool_MaterialAvailabilityBoard service,
        IService_ShipRecToolsSettings shipRecToolsSettings,
        IService_SharedLocationRange locationRangeService,
        IService_AppSettings appSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(shipRecToolsSettings);
        ArgumentNullException.ThrowIfNull(locationRangeService);
        ArgumentNullException.ThrowIfNull(appSettings);
        _service = service;
        _shipRecToolsSettings = shipRecToolsSettings;
        _locationRangeService = locationRangeService;
        _isMockDataEnabled = appSettings.GetUseInforVisualMockData();
    }

    /// <summary>
    /// Shows the shared guidance message for the Material Availability Board when it becomes active.
    /// </summary>
    public void ActivateView()
    {
        SetLocalStatus(
            IsSearchByLocationMode
                ? "Enter a warehouse location and click Search."
                : "Enter a part number and click Search."
        );
    }

    [RelayCommand]
    private void SetSearchByLocation()
    {
        IsSearchByLocationMode = true;
        SearchTerm = string.Empty;
        ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
        SetLocalStatus("Enter a warehouse location and click Search.");
    }

    [RelayCommand]
    private void SetSearchByPart()
    {
        IsSearchByLocationMode = false;

        // The range only applies to warehouse locations, so Part Number mode always returns
        // to the single search box.
        IsLocationRangeEnabled = false;

        SearchTerm = string.Empty;
        ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
        SetLocalStatus("Enter a part number and click Search.");
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await _errorHandler.ShowUserErrorAsync(
                $"Please enter a {(IsSearchByLocationMode ? "warehouse location" : "part number")} to search.",
                "Input Required",
                nameof(SearchAsync)
            );
            return;
        }

        try
        {
            IsBusy = true;
            SetLocalStatus($"Searching for '{SearchTerm.Trim()}'…");

            if (IsSearchByLocationMode)
            {
                var confirmedLocation = await ResolveLocationAsync(SearchTerm);
                if (confirmedLocation is null)
                {
                    return;
                }

                SearchTerm = confirmedLocation.Label;
                await LoadBoardByLocationAsync(confirmedLocation.Key, GetSelectedLookAheadDays());
            }
            else
            {
                var confirmedPart = await ResolvePartAsync(SearchTerm);
                if (confirmedPart is null)
                {
                    return;
                }

                SearchTerm = confirmedPart.Label;
                await LoadBoardByPartAsync(confirmedPart.Key, GetSelectedLookAheadDays());
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SearchAsync),
                nameof(ViewModel_Tool_MaterialAvailabilityBoard)
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
        ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
        SetLocalStatus(
            IsSearchByLocationMode
                ? "Enter a warehouse location and click Search."
                : "Enter a part number and click Search."
        );
    }

    /// <summary>
    /// Loads the board for every location in the operator-entered Start/Stop range. Each location
    /// becomes its own card group, marked with a header strip, so a range reads as one section per
    /// location instead of one undifferentiated list.
    /// </summary>
    [RelayCommand]
    private async Task LoadLocationRangeAsync()
    {
        if (
            string.IsNullOrWhiteSpace(RangeStartLocation)
            || string.IsNullOrWhiteSpace(RangeStopLocation)
        )
        {
            await _errorHandler.ShowUserErrorAsync(
                "Enter both a start and a stop location for the range.",
                "Input Required",
                nameof(LoadLocationRangeAsync)
            );
            return;
        }

        try
        {
            IsBusy = true;
            SetLocalStatus(
                $"Resolving the location range {RangeStartLocation} to {RangeStopLocation}…"
            );

            var rangeResult = await _locationRangeService.ResolveRangeAsync(
                RangeStartLocation,
                RangeStopLocation,
                DefaultWarehouseCode
            );

            if (!rangeResult.IsSuccess || rangeResult.Data is null)
            {
                ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
                SetLocalStatus(
                    string.IsNullOrWhiteSpace(rangeResult.ErrorMessage)
                        ? "The location range could not be resolved."
                        : rangeResult.ErrorMessage,
                    InfoBarSeverity.Warning
                );
                return;
            }

            var range = rangeResult.Data;

            // Show the canonical values back so the operator sees the formatted range.
            RangeStartLocation = range.StartLocation;
            RangeStopLocation = range.StopLocation;

            if (range.Locations.Count == 0)
            {
                ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
                SetLocalStatus(
                    $"No warehouse locations exist between {range.StartLocation} and {range.StopLocation}.",
                    InfoBarSeverity.Warning
                );
                return;
            }

            var lookAheadDays = GetSelectedLookAheadDays();
            var allCards = new List<Model_Tool_MaterialAvailabilityCard>();
            var locationGroups = new List<Model_Tool_MaterialAvailabilityLocationGroup>();

            foreach (var location in range.Locations)
            {
                var boardResult = await _service.GetBoardByLocationAsync(
                    location,
                    DefaultWarehouseCode,
                    lookAheadDays
                );

                if (!boardResult.IsSuccess || boardResult.Data is null)
                {
                    ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
                    SetLocalStatus(
                        string.IsNullOrWhiteSpace(boardResult.ErrorMessage)
                            ? $"The board for location {location} could not be loaded."
                            : boardResult.ErrorMessage,
                        InfoBarSeverity.Warning
                    );
                    return;
                }

                var locationCards = boardResult.Data;
                if (locationCards.Count == 0)
                {
                    continue;
                }

                // One collapsible section per location so the operator can collapse the
                // locations they are not working while keeping the location headers in view.
                var group = new Model_Tool_MaterialAvailabilityLocationGroup
                {
                    LocationId = location,
                };

                foreach (var card in locationCards)
                {
                    group.Cards.Add(card);
                    allCards.Add(card);
                }

                locationGroups.Add(group);
            }

            ReplaceCards(allCards, locationGroups);
            SearchTerm = $"{range.StartLocation} to {range.StopLocation}";

            var status =
                allCards.Count > 0
                    ? $"Found {allCards.Count} part card(s) across {range.Locations.Count} location(s) from {range.StartLocation} to {range.StopLocation}."
                    : $"No positive-quantity parts were found in the {range.Locations.Count} location(s) from {range.StartLocation} to {range.StopLocation}.";

            if (range.WasSwapped)
            {
                status +=
                    " Start and stop were swapped so the range runs in warehouse order.";
            }

            if (range.WasTruncated)
            {
                status +=
                    $" The range was limited to the first {_locationRangeService.MaxLocationsInRange} locations.";
            }

            SetLocalStatus(status);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadLocationRangeAsync),
                nameof(ViewModel_Tool_MaterialAvailabilityBoard)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        if (!HasCards)
        {
            SetLocalStatus(
                "Search and load material cards before printing.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (RequestPrintAsync is null)
        {
            SetLocalStatus("Print is not available from this view.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            var useTransactionSheet = false;
            if (IsSearchByLocationMode && SelectLocationPrintModeAsync is not null)
            {
                var selection = await SelectLocationPrintModeAsync();
                if (!selection.HasValue)
                {
                    SetLocalStatus("Print cancelled.", InfoBarSeverity.Informational);
                    return;
                }

                useTransactionSheet = selection.Value;
            }

            SetLocalStatus(
                useTransactionSheet
                    ? "Preparing printable Material Availability transaction sheet…"
                    : "Preparing printable Material Availability Board…"
            );

            var documentResult = await _service.FormatBoardForPrintAsync(
                Cards,
                SearchLabel.TrimEnd(':'),
                SearchTerm,
                DefaultWarehouseCode,
                SelectedLookAheadOption,
                useTransactionSheet
            );

            if (!documentResult.IsSuccess || documentResult.Data is null)
            {
                SetLocalStatus(documentResult.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            var printResult = await RequestPrintAsync(documentResult.Data);
            if (!printResult.IsSuccess || !printResult.Data)
            {
                SetLocalStatus(printResult.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            SetLocalStatus(
                useTransactionSheet
                    ? "Opened a print-ready Material Availability transaction sheet in your browser."
                    : "Opened a print-ready Material Availability Board in your browser.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(PrintAsync),
                nameof(ViewModel_Tool_MaterialAvailabilityBoard)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Model_FuzzySearchResult?> ResolveLocationAsync(string rawInput)
    {
        var normalizedInput = rawInput.Trim().ToUpperInvariant();
        var exactMatchResult = await _service.LocationExistsAsync(
            normalizedInput,
            DefaultWarehouseCode
        );
        if (!exactMatchResult.IsSuccess)
        {
            SetLocalStatus(exactMatchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (exactMatchResult.Data)
        {
            return new Model_FuzzySearchResult
            {
                Key = normalizedInput,
                Label = normalizedInput,
                Detail = $"Warehouse {DefaultWarehouseCode}",
            };
        }

        var fuzzySearchResult = await _service.FuzzySearchLocationsAsync(
            normalizedInput,
            DefaultWarehouseCode
        );
        if (!fuzzySearchResult.IsSuccess || fuzzySearchResult.Data is null)
        {
            SetLocalStatus(fuzzySearchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (fuzzySearchResult.Data.Count == 0)
        {
            SetLocalStatus(
                $"No warehouse locations found matching '{normalizedInput}'.",
                InfoBarSeverity.Warning
            );
            return null;
        }

        return await ConfirmCandidateAsync(fuzzySearchResult.Data, "Select Warehouse Location");
    }

    private async Task<Model_FuzzySearchResult?> ResolvePartAsync(string rawInput)
    {
        var normalizedInput = rawInput.Trim().ToUpperInvariant();
        var exactMatchResult = await _service.PartExistsAsync(normalizedInput);
        if (!exactMatchResult.IsSuccess)
        {
            SetLocalStatus(exactMatchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (exactMatchResult.Data)
        {
            return new Model_FuzzySearchResult { Key = normalizedInput, Label = normalizedInput };
        }

        var fuzzySearchResult = await _service.FuzzySearchPartsAsync(normalizedInput);
        if (!fuzzySearchResult.IsSuccess || fuzzySearchResult.Data is null)
        {
            SetLocalStatus(fuzzySearchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (fuzzySearchResult.Data.Count == 0)
        {
            SetLocalStatus(
                $"No part numbers found matching '{normalizedInput}'.",
                InfoBarSeverity.Warning
            );
            return null;
        }

        return await ConfirmCandidateAsync(fuzzySearchResult.Data, "Select Part Number");
    }

    private async Task<Model_FuzzySearchResult?> ConfirmCandidateAsync(
        IReadOnlyList<Model_FuzzySearchResult> candidates,
        string dialogTitle
    )
    {
        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        if (ShowFuzzyPickerAsync is null)
        {
            return candidates[0];
        }

        var picked = await ShowFuzzyPickerAsync(candidates, dialogTitle);
        if (picked is null)
        {
            SetLocalStatus("Search cancelled.", InfoBarSeverity.Informational);
        }

        return picked;
    }

    private async Task LoadBoardByLocationAsync(string locationId, int? incomingWindowDays)
    {
        var boardResult = await _service.GetBoardByLocationAsync(
            locationId,
            DefaultWarehouseCode,
            incomingWindowDays
        );
        if (!boardResult.IsSuccess || boardResult.Data is null)
        {
            SetLocalStatus(boardResult.ErrorMessage, InfoBarSeverity.Warning);
            ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
            return;
        }

        ReplaceCards(boardResult.Data);
        SetLocalStatus(
            Cards.Count > 0
                ? $"Found {Cards.Count} part card(s) for location {locationId}."
                : $"No positive-quantity parts were found in location {locationId}."
        );
    }

    private async Task LoadBoardByPartAsync(string partId, int? incomingWindowDays)
    {
        var boardResult = await _service.GetBoardByPartAsync(
            partId,
            DefaultWarehouseCode,
            incomingWindowDays
        );
        if (!boardResult.IsSuccess || boardResult.Data is null)
        {
            SetLocalStatus(boardResult.ErrorMessage, InfoBarSeverity.Warning);
            ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
            return;
        }

        ReplaceCards(boardResult.Data);
        SetLocalStatus(
            Cards.Count > 0
                ? $"Built the material board for part {partId}."
                : $"No material board data was available for part {partId}."
        );
    }

    [RelayCommand]
    private async Task ShowIncomingDetailsAsync(Model_Tool_MaterialAvailabilityCard? card)
    {
        if (card is null || ShowIncomingMaterialDetailsAsync is null)
        {
            return;
        }

        var dialogViewModel = new ViewModel_Dialog_MaterialAvailabilityIncomingDetails(
            card,
            _service,
            _errorHandler,
            _logger,
            new NullNotificationServiceShim()
        )
        {
            RequestPrintAsync = RequestPrintAsync,
        };

        await ShowIncomingMaterialDetailsAsync(dialogViewModel);
    }

    [RelayCommand]
    private async Task ShowWorkOrderDetailsAsync(Model_Tool_MaterialAvailabilityCard? card)
    {
        if (card?.PrimaryAssociatedPartRun is null || ShowWorkOrderDetailsDialogAsync is null)
        {
            return;
        }

        var fieldSettings = await _shipRecToolsSettings.GetMaterialAvailabilityFieldSettingsAsync();
        var dialogViewModel = new ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails(
            card.PrimaryAssociatedPartRun,
            fieldSettings,
            _service,
            _errorHandler,
            _logger,
            new NullNotificationServiceShim()
        )
        {
            RequestPrintAsync = RequestPrintAsync,
        };

        await ShowWorkOrderDetailsDialogAsync(dialogViewModel);
    }

    private void ReplaceCards(IEnumerable<Model_Tool_MaterialAvailabilityCard> cards)
    {
        Cards = new ObservableCollection<Model_Tool_MaterialAvailabilityCard>(cards);

        // Any caller that does not supply location groups (single location or part search)
        // clears whatever grouped range results were on screen.
        CardGroups = new ObservableCollection<Model_Tool_MaterialAvailabilityLocationGroup>();
    }

    /// <summary>
    /// Replaces the board contents with location-range results: the same cards are exposed flat
    /// for printing and grouped into collapsible per-location sections for display.
    /// </summary>
    private void ReplaceCards(
        IEnumerable<Model_Tool_MaterialAvailabilityCard> cards,
        IEnumerable<Model_Tool_MaterialAvailabilityLocationGroup> groups
    )
    {
        Cards = new ObservableCollection<Model_Tool_MaterialAvailabilityCard>(cards);
        CardGroups = new ObservableCollection<Model_Tool_MaterialAvailabilityLocationGroup>(groups);
    }

    /// <summary>
    /// Applies the shared warehouse-location rule to a typed range bound so "r04" displays as
    /// "R-04" and "va001" as "V-A0-01", exactly like the location lookup used elsewhere.
    /// </summary>
    /// <param name="location">Raw location text from the Start/Stop box.</param>
    public string FormatLocation(string location)
    {
        return Helper_SharedLocationFormat.FormatOrPassThrough(location);
    }

    private int? GetSelectedLookAheadDays()
    {
        return string.Equals(SelectedLookAheadOption, "All", StringComparison.OrdinalIgnoreCase)
                ? null
            : int.TryParse(SelectedLookAheadOption, out var days) ? days
            : 30;
    }

    private void SetLocalStatus(
        string? message,
        InfoBarSeverity severity = InfoBarSeverity.Informational
    )
    {
        ShowStatus(message ?? string.Empty, severity);
    }

    private sealed class NullNotificationServiceShim : IService_Notification
    {
        public string StatusMessage => string.Empty;

        public InfoBarSeverity StatusSeverity => InfoBarSeverity.Informational;

        public bool IsStatusOpen { get; set; }

        public string StatusActionLabel => string.Empty;

        public bool IsStatusActionVisible => false;

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }

        public void ShowStatus(
            string message,
            InfoBarSeverity severity = InfoBarSeverity.Informational
        ) { }

        public void ShowStatusWithAction(
            string message,
            InfoBarSeverity severity,
            string actionLabel,
            Func<Task> action
        ) { }

        public Task ExecuteStatusActionAsync()
        {
            return Task.CompletedTask;
        }

        public void ClearStatusAction() { }
    }
}
