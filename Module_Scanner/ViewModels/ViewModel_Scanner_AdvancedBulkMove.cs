using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.ViewModels;

/// <summary>
/// Two-step Advanced bulk-move page. Step 1 searches a From Location and lets the operator
/// choose the parts to move (with an entry-count per part); Step 2 expands each selected part
/// into destination rows (one row per entry count) where the operator enters the To location
/// and quantity. Save hands the completed destination rows back to the workbench through the
/// navigation service and returns to the workbench page.
/// </summary>
public partial class ViewModel_Scanner_AdvancedBulkMove : ViewModel_Shared_Base
{
    private const string DefaultWarehouse = "002";

    private readonly IService_ScannerValidation _validationService;
    private readonly IService_ScannerNavigation _navigationService;
    private readonly string _warehouseCode;

    /// <summary>Parts found at the searched From Location (Step 1 list).</summary>
    public ObservableCollection<Model_ScannerBulkMovePart> Parts { get; } = [];

    /// <summary>Destination rows the operator completes in Step 2.</summary>
    public ObservableCollection<Model_ScannerBulkMoveDestination> Destinations { get; } = [];

    /// <summary>Location suggestions for the fuzzy From Location search box.</summary>
    public ObservableCollection<Model_FuzzySearchResult> LocationSuggestions { get; } = [];

    [ObservableProperty]
    private string _fromLocation = string.Empty;

    [ObservableProperty]
    private bool _isStepTwo;

    [ObservableProperty]
    private string _inlineStatus = string.Empty;

    [ObservableProperty]
    private string _stepHeaderText = "1. Select a From Location and choose the parts to move.";

    [ObservableProperty]
    private string _nextButtonText = "Next";

    [ObservableProperty]
    private string _nextButtonIcon = "\uE72A";

    partial void OnIsStepTwoChanged(bool value)
    {
        if (value)
        {
            StepHeaderText = "2. Enter the destination location and quantity for each row.";
            NextButtonText = "Save";
            NextButtonIcon = "\uE74E";
        }
        else
        {
            StepHeaderText = "1. Select a From Location and choose the parts to move.";
            NextButtonText = "Next";
            NextButtonIcon = "\uE72A";
        }
    }

    public ViewModel_Scanner_AdvancedBulkMove(
        IService_ScannerValidation validationService,
        IService_ScannerNavigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _validationService =
            validationService ?? throw new ArgumentNullException(nameof(validationService));
        _navigationService =
            navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _warehouseCode = DefaultWarehouse;
    }

    /// <summary>
    /// Refreshes the fuzzy location suggestions as the operator types in the From Location
    /// search box.
    /// </summary>
    public async Task UpdateLocationSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            LocationSuggestions.Clear();
            return;
        }

        var result = await _validationService.GetLocationSuggestionsAsync(query, _warehouseCode);
        if (!result.Success || result.Data is null)
        {
            return;
        }

        LocationSuggestions.Clear();
        foreach (var suggestion in result.Data)
        {
            LocationSuggestions.Add(suggestion);
        }
    }

    /// <summary>
    /// Searches the entered From Location for parts that currently hold on-hand stock and fills
    /// the Step 1 parts list.
    /// </summary>
    [RelayCommand]
    private async Task SearchPartsAsync()
    {
        var location = FromLocation?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(location))
        {
            InlineStatus = "Enter a From Location before searching.";
            return;
        }

        Parts.Clear();
        InlineStatus = "Searching for parts...";

        var result = await _validationService.GetPartsInLocationAsync(location, _warehouseCode);
        if (!result.Success || result.Data is null)
        {
            InlineStatus = string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? "Unable to load parts for that location."
                : result.ErrorMessage;
            return;
        }

        foreach (
            var row in result.Data.OrderBy(part => part.PartId, StringComparer.OrdinalIgnoreCase)
        )
        {
            Parts.Add(
                new Model_ScannerBulkMovePart
                {
                    PartId = row.PartId,
                    PartDescription = row.PartDescription,
                    WarehouseCode = row.WarehouseCode,
                    LocationId = row.LocationId,
                    Quantity = row.Quantity,
                    IsSelected = false,
                    EntryCount = 1,
                }
            );
        }

        InlineStatus =
            Parts.Count > 0
                ? $"{Parts.Count} part(s) found in {location}."
                : $"No parts with stock were found in {location}.";
    }

    /// <summary>
    /// Advances from Step 1 to Step 2, expanding the selected parts into destination rows, or
    /// (when already on Step 2) saves the completed rows back to the workbench.
    /// </summary>
    [RelayCommand]
    private void NextStep()
    {
        if (!IsStepTwo)
        {
            if (!BuildDestinations())
            {
                return;
            }

            IsStepTwo = true;
            InlineStatus =
                $"Enter the destination location and quantity for {Destinations.Count} row(s).";
            return;
        }

        if (!ValidateDestinations(out var message))
        {
            InlineStatus = message;
            return;
        }

        _navigationService.PendingBulkMoveDestinations = GetDestinations();
        _navigationService.ShowWorkbench();
    }

    /// <summary>Returns from Step 2 to Step 1, discarding the in-progress destination rows.</summary>
    [RelayCommand]
    private void BackToStepOne()
    {
        Destinations.Clear();
        IsStepTwo = false;
        InlineStatus = string.Empty;
    }

    /// <summary>Leaves the page without saving and returns to the workbench.</summary>
    [RelayCommand]
    private void Cancel()
    {
        _navigationService.PendingBulkMoveDestinations = null;
        _navigationService.ShowWorkbench();
    }

    /// <summary>Returns a snapshot of the completed destination rows.</summary>
    public IReadOnlyList<Model_ScannerBulkMoveDestination> GetDestinations()
    {
        return Destinations.Select(CloneDestination).ToList();
    }

    private bool BuildDestinations()
    {
        var selected = Parts.Where(part => part.IsSelected).ToList();
        if (selected.Count == 0)
        {
            InlineStatus = "Select at least one part to move.";
            return false;
        }

        Destinations.Clear();

        foreach (var part in selected)
        {
            var entryCount = Math.Clamp(part.EntryCount, 1, 99);
            var splitQuantity = part.Quantity > 0 ? part.Quantity / entryCount : 0m;

            for (var index = 0; index < entryCount; index++)
            {
                Destinations.Add(
                    new Model_ScannerBulkMoveDestination
                    {
                        PartId = part.PartId,
                        PartDescription = part.PartDescription,
                        FromWarehouse = part.WarehouseCode,
                        FromLocation = part.LocationId,
                        ToLocation = string.Empty,
                        QuantityNumber = (double)splitQuantity,
                    }
                );
            }
        }

        return true;
    }

    private bool ValidateDestinations(out string message)
    {
        var rows = Destinations.ToList();
        if (rows.Count == 0)
        {
            message = "No destination rows to save.";
            return false;
        }

        var missingTo = rows.Count(row => string.IsNullOrWhiteSpace(row.ToLocation));
        if (missingTo > 0)
        {
            message = $"Enter a destination location for the {missingTo} row(s) missing one.";
            return false;
        }

        var invalidQty = rows.Count(row => row.QuantityNumber <= 0);
        if (invalidQty > 0)
        {
            message = $"Enter a quantity greater than zero for the {invalidQty} row(s) missing one.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static Model_ScannerBulkMoveDestination CloneDestination(
        Model_ScannerBulkMoveDestination source
    )
    {
        return new Model_ScannerBulkMoveDestination
        {
            PartId = source.PartId,
            PartDescription = source.PartDescription,
            FromWarehouse = source.FromWarehouse,
            FromLocation = source.FromLocation,
            ToLocation = source.ToLocation,
            QuantityNumber = source.QuantityNumber,
        };
    }
}
