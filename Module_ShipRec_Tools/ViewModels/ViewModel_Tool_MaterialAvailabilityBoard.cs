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
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

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
    private readonly bool _isMockDataEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchLabel))]
    [NotifyPropertyChangedFor(nameof(SearchPlaceholder))]
    [NotifyPropertyChangedFor(nameof(IsSearchByLocation))]
    [NotifyPropertyChangedFor(nameof(IsSearchByPart))]
    [NotifyPropertyChangedFor(nameof(MockDataHintText))]
    private bool _isSearchByLocationMode = true;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCards))]
    private ObservableCollection<Model_Tool_MaterialAvailabilityCard> _cards = new();

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

    public string LookAheadLabel => "Look Ahead:";

    public bool HasCards => Cards.Count > 0;

    public bool IsMockDataHintVisible => _isMockDataEnabled;

    public string MockDataHintText =>
        _isMockDataEnabled is false ? string.Empty
        : IsSearchByLocationMode
            ? $"Mock data is enabled. Search location {MockModalLocationSearchTerm} to load a card that supports both detail windows."
        : $"Mock data is enabled. Search part {MockModalPartSearchTerm} to load a card that supports both detail windows.";

    public ViewModel_Tool_MaterialAvailabilityBoard(
        IService_Tool_MaterialAvailabilityBoard service,
        IService_ShipRecToolsSettings shipRecToolsSettings,
        IService_AppSettings appSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(shipRecToolsSettings);
        ArgumentNullException.ThrowIfNull(appSettings);
        _service = service;
        _shipRecToolsSettings = shipRecToolsSettings;
        _isMockDataEnabled = appSettings.GetUseInforVisualMockData();
        SetLocalStatus("Enter a warehouse location or part number and click Search.");
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
        StatusMessage = message ?? string.Empty;
        StatusSeverity = severity;
        IsStatusOpen = string.IsNullOrWhiteSpace(StatusMessage) is false;
    }

    private sealed class NullNotificationServiceShim : IService_Notification
    {
        public string StatusMessage => string.Empty;

        public InfoBarSeverity StatusSeverity => InfoBarSeverity.Informational;

        public bool IsStatusOpen { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }

        public void ShowStatus(
            string message,
            InfoBarSeverity severity = InfoBarSeverity.Informational
        ) { }
    }
}
