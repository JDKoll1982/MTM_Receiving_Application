using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Two-step Advanced bulk-move dialog. Step 1 selects a From Location and the parts to move
/// (with an entry-count per part); Step 2 expands each selected part into destination rows
/// (one row per entry count) where the operator enters the To location and quantity. Returns
/// the final destination rows for the workbench to stage into the Items To Send table.
/// </summary>
public sealed partial class View_Scanner_AdvancedMoveDialog : ContentDialog
{
    private const string DefaultWarehouse = "002";

    private readonly IService_ScannerValidation _validationService;
    private readonly string _warehouseCode;
    private bool _isStepTwo;

    public ObservableCollection<Model_ScannerBulkMovePart> Parts { get; } = [];

    public ObservableCollection<Model_ScannerBulkMoveDestination> Destinations { get; } = [];

    public View_Scanner_AdvancedMoveDialog(
        IService_ScannerValidation validationService,
        string? warehouseCode = null
    )
    {
        _validationService =
            validationService ?? throw new ArgumentNullException(nameof(validationService));
        _warehouseCode = string.IsNullOrWhiteSpace(warehouseCode)
            ? DefaultWarehouse
            : warehouseCode.Trim().ToUpperInvariant();

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        FromLocationTextBox.Text = string.Empty;
    }

    /// <summary>
    /// Returns the destination rows the operator completed in Step 2. Empty when the dialog was
    /// dismissed before completing Step 2.
    /// </summary>
    public IReadOnlyList<Model_ScannerBulkMoveDestination> GetDestinations()
    {
        return Destinations.Select(CloneDestination).ToList();
    }

    private async void LoadParts_Click(object sender, RoutedEventArgs e)
    {
        var location = FromLocationTextBox.Text?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(location))
        {
            ShowInlineStatus("Enter a From Location before loading parts.");
            return;
        }

        PartsListView.ItemsSource = null;
        Parts.Clear();
        ShowInlineStatus("Loading parts...");

        var result = await _validationService.GetPartsInLocationAsync(location, _warehouseCode);
        if (!result.Success || result.Data is null)
        {
            ShowInlineStatus(
                string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Unable to load parts for that location."
                    : result.ErrorMessage
            );
            return;
        }

        foreach (var row in result.Data.OrderBy(part => part.PartId, StringComparer.OrdinalIgnoreCase))
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

        ShowInlineStatus(
            Parts.Count > 0
                ? $"{Parts.Count} part(s) found in {location}."
                : $"No parts with stock were found in {location}."
        );
        PartsListView.ItemsSource = Parts;
    }

    private void PrimaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (!_isStepTwo)
        {
            // Step 1 → Step 2: keep the dialog open and expand the selected parts.
            args.Cancel = true;
            if (!BuildDestinations())
            {
                return;
            }

            EnterStepTwo();
            return;
        }

        // Step 2: validate before closing with Primary result.
        if (!ValidateDestinations(out var message))
        {
            args.Cancel = true;
            ShowInlineStatus(message);
            return;
        }

        ShowInlineStatus(string.Empty);
    }

    private bool BuildDestinations()
    {
        var selected = Parts.Where(part => part.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ShowInlineStatus("Select at least one part to move.");
            return false;
        }

        Destinations.Clear();

        foreach (var part in selected)
        {
            var entryCount = Math.Clamp(part.EntryCount, 1, 99);
            var splitQuantity = part.Quantity > 0
                ? part.Quantity / entryCount
                : 0m;

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

    private void EnterStepTwo()
    {
        _isStepTwo = true;
        Title = "Add To Locations";
        PrimaryButtonText = "Save";
        CloseButtonText = "Cancel";
        Step1Panel.Visibility = Visibility.Collapsed;
        Step2Panel.Visibility = Visibility.Visible;
        ShowInlineStatus(
            $"Enter the destination location and quantity for {Destinations.Count} row(s)."
        );
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

    private void ShowInlineStatus(string message)
    {
        if (StatusTextBlock is not null)
        {
            StatusTextBlock.Text = message;
        }
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
