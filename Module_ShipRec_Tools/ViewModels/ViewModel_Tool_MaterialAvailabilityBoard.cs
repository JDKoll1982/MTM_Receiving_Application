using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
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

    private readonly IService_Tool_MaterialAvailabilityBoard _service;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchLabel))]
    [NotifyPropertyChangedFor(nameof(SearchPlaceholder))]
    [NotifyPropertyChangedFor(nameof(IsSearchByLocation))]
    [NotifyPropertyChangedFor(nameof(IsSearchByPart))]
    private bool _isSearchByLocationMode = true;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCards))]
    private ObservableCollection<Model_Tool_MaterialAvailabilityCard> _cards = new();

    public Func<
        IReadOnlyList<Model_FuzzySearchResult>,
        string,
        Task<Model_FuzzySearchResult?>
    >? ShowFuzzyPickerAsync { get; set; }

    public bool IsSearchByLocation => IsSearchByLocationMode;

    public bool IsSearchByPart => !IsSearchByLocationMode;

    public string SearchLabel => IsSearchByLocationMode ? "Warehouse Location:" : "Part Number:";

    public string SearchPlaceholder => IsSearchByLocationMode ? "e.g. RECV" : "e.g. MMC0000658";

    public bool HasCards => Cards.Count > 0;

    public ViewModel_Tool_MaterialAvailabilityBoard(
        IService_Tool_MaterialAvailabilityBoard service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
        ShowStatus("Enter a warehouse location or part number and click Search.");
    }

    [RelayCommand]
    private void SetSearchByLocation()
    {
        IsSearchByLocationMode = true;
        SearchTerm = string.Empty;
        ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
        ShowStatus("Enter a warehouse location and click Search.");
    }

    [RelayCommand]
    private void SetSearchByPart()
    {
        IsSearchByLocationMode = false;
        SearchTerm = string.Empty;
        ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
        ShowStatus("Enter a part number and click Search.");
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
            ShowStatus($"Searching for '{SearchTerm.Trim()}'…");

            if (IsSearchByLocationMode)
            {
                var confirmedLocation = await ResolveLocationAsync(SearchTerm);
                if (confirmedLocation is null)
                {
                    return;
                }

                SearchTerm = confirmedLocation.Label;
                await LoadBoardByLocationAsync(confirmedLocation.Key);
            }
            else
            {
                var confirmedPart = await ResolvePartAsync(SearchTerm);
                if (confirmedPart is null)
                {
                    return;
                }

                SearchTerm = confirmedPart.Label;
                await LoadBoardByPartAsync(confirmedPart.Key);
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
        ShowStatus(
            IsSearchByLocationMode
                ? "Enter a warehouse location and click Search."
                : "Enter a part number and click Search."
        );
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
            ShowStatus(exactMatchResult.ErrorMessage, InfoBarSeverity.Warning);
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
            ShowStatus(fuzzySearchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (fuzzySearchResult.Data.Count == 0)
        {
            ShowStatus(
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
            ShowStatus(exactMatchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (exactMatchResult.Data)
        {
            return new Model_FuzzySearchResult { Key = normalizedInput, Label = normalizedInput };
        }

        var fuzzySearchResult = await _service.FuzzySearchPartsAsync(normalizedInput);
        if (!fuzzySearchResult.IsSuccess || fuzzySearchResult.Data is null)
        {
            ShowStatus(fuzzySearchResult.ErrorMessage, InfoBarSeverity.Warning);
            return null;
        }

        if (fuzzySearchResult.Data.Count == 0)
        {
            ShowStatus(
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
            ShowStatus("Search cancelled.", InfoBarSeverity.Informational);
        }

        return picked;
    }

    private async Task LoadBoardByLocationAsync(string locationId)
    {
        var boardResult = await _service.GetBoardByLocationAsync(locationId, DefaultWarehouseCode);
        if (!boardResult.IsSuccess || boardResult.Data is null)
        {
            ShowStatus(boardResult.ErrorMessage, InfoBarSeverity.Warning);
            ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
            return;
        }

        ReplaceCards(boardResult.Data);
        ShowStatus(
            Cards.Count > 0
                ? $"Found {Cards.Count} part card(s) for location {locationId}."
                : $"No positive-quantity parts were found in location {locationId}."
        );
    }

    private async Task LoadBoardByPartAsync(string partId)
    {
        var boardResult = await _service.GetBoardByPartAsync(partId, DefaultWarehouseCode);
        if (!boardResult.IsSuccess || boardResult.Data is null)
        {
            ShowStatus(boardResult.ErrorMessage, InfoBarSeverity.Warning);
            ReplaceCards(Array.Empty<Model_Tool_MaterialAvailabilityCard>());
            return;
        }

        ReplaceCards(boardResult.Data);
        ShowStatus(
            Cards.Count > 0
                ? $"Built the material board for part {partId}."
                : $"No material board data was available for part {partId}."
        );
    }

    private void ReplaceCards(IEnumerable<Model_Tool_MaterialAvailabilityCard> cards)
    {
        Cards = new ObservableCollection<Model_Tool_MaterialAvailabilityCard>(cards);
    }
}
