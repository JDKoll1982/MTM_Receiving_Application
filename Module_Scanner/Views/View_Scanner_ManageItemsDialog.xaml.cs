using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Manage Items dialog for the current list: reorder, duplicate, add, and delete rows.
/// Applies the validated snapshot back to the Workbench on "Apply".
/// </summary>
public sealed partial class View_Scanner_ManageItemsDialog : ContentDialog
{
    private readonly Model_ScannerBatchSession _session;
    private readonly IService_ScannerValidation _validationService;

    public ObservableCollection<Model_ScannerBatchItem> Items { get; } = [];

    public Model_ScannerBatchItem? SelectedItem { get; set; }

    public View_Scanner_ManageItemsDialog(
        Model_ScannerBatchSession session,
        IService_ScannerValidation validationService
    )
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _validationService =
            validationService ?? throw new ArgumentNullException(nameof(validationService));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        // Edge-case fix: block Apply when any row is incomplete so blank parts or bad values
        // cannot be saved into the current list.
        PrimaryButtonClick += OnApplyPrimaryButtonClick;

        foreach (var item in session.Items.OrderBy(candidate => candidate.SequenceNumber))
        {
            Items.Add(CloneItem(item));
        }

        RefreshSequenceNumbers();
        SelectedItem = Items.FirstOrDefault();
    }

    
    public IReadOnlyList<Model_ScannerBatchItem> GetItemsSnapshot()
    {
        RefreshSequenceNumbers();
        return Items.Select(CloneItem).ToList();
    }

    private void OnApplyPrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs args
    )
    {
        RefreshSequenceNumbers();
        var error = ValidateRowsForApply();
        if (error is not null)
        {
            args.Cancel = true;
            ApplyErrorText.Text = error;
            ApplyErrorText.Visibility = Visibility.Visible;
            return;
        }

        ApplyErrorText.Text = string.Empty;
        ApplyErrorText.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Returns an error message when any row cannot be saved into the current list (blank
    /// part/source or invalid quantity); otherwise returns null. Keeps the dialog open so the
    /// operator fixes or deletes the offending row before applying.
    /// </summary>
    private string? ValidateRowsForApply()
    {
        if (Items.Count == 0)
        {
            return null;
        }

        foreach (var item in Items)
        {
            if (string.IsNullOrWhiteSpace(item.PayloadPartId))
            {
                return $"Row {item.SequenceNumber} is missing a part number. Enter one or delete the row before applying.";
            }

            if (string.IsNullOrWhiteSpace(item.PayloadFromLocation))
            {
                return $"Row {item.SequenceNumber} is missing a source (From) location. Enter one or delete the row before applying.";
            }

            if (!TryParseDecimalOnly(item.PayloadQuantity, out var quantity) || quantity <= 0)
            {
                return $"Row {item.SequenceNumber} has an invalid quantity. Use a number greater than zero (decimals only).";
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a quantity as a plain decimal with the period as the decimal point. Commas
    /// (thousands separators) and exponent notation are rejected.
    /// </summary>
    private static bool TryParseDecimalOnly(string? raw, out decimal value)
    {
        return decimal.TryParse(
            raw?.Trim(),
            NumberStyles.AllowLeadingSign
                | NumberStyles.AllowDecimalPoint
                | NumberStyles.AllowLeadingWhite
                | NumberStyles.AllowTrailingWhite,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        var newItem = CreateNewItem();
        var insertIndex = SelectedItem is null ? Items.Count : Items.IndexOf(SelectedItem) + 1;

        if (insertIndex < 0 || insertIndex > Items.Count)
        {
            insertIndex = Items.Count;
        }

        Items.Insert(insertIndex, newItem);
        RefreshSequenceNumbers();
        SelectedItem = newItem;
        ItemsListView.SelectedItem = newItem;
    }

    private void DuplicateItem_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem is null)
        {
            return;
        }

        var duplicate = CloneItem(SelectedItem);
        duplicate.ItemId = Guid.NewGuid();

        var insertIndex = Items.IndexOf(SelectedItem) + 1;
        Items.Insert(insertIndex, duplicate);
        RefreshSequenceNumbers();
        SelectedItem = duplicate;
        ItemsListView.SelectedItem = duplicate;
    }

    private void MoveUpItem_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem is null)
        {
            return;
        }

        var currentIndex = Items.IndexOf(SelectedItem);
        if (currentIndex <= 0)
        {
            return;
        }

        Items.Move(currentIndex, currentIndex - 1);
        RefreshSequenceNumbers();
        ItemsListView.SelectedItem = SelectedItem;
    }

    private void MoveDownItem_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem is null)
        {
            return;
        }

        var currentIndex = Items.IndexOf(SelectedItem);
        if (currentIndex < 0 || currentIndex >= Items.Count - 1)
        {
            return;
        }

        Items.Move(currentIndex, currentIndex + 1);
        RefreshSequenceNumbers();
        ItemsListView.SelectedItem = SelectedItem;
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem is null)
        {
            return;
        }

        var index = Items.IndexOf(SelectedItem);
        if (index < 0)
        {
            return;
        }

        Items.RemoveAt(index);
        RefreshSequenceNumbers();

        SelectedItem = index < Items.Count ? Items[index] : Items.LastOrDefault();
        ItemsListView.SelectedItem = SelectedItem;
    }

    private void RefreshSequenceNumbers()
    {
        for (var index = 0; index < Items.Count; index++)
        {
            Items[index].SequenceNumber = index + 1;
            Items[index].SessionId = _session.SessionId;
        }
    }

    private static Model_ScannerBatchItem CloneItem(Model_ScannerBatchItem source)
    {
        return new Model_ScannerBatchItem
        {
            ItemId = source.ItemId,
            SessionItemId = source.SessionItemId,
            SessionId = source.SessionId,
            SequenceNumber = source.SequenceNumber,
            PayloadPartId = source.PayloadPartId,
            PayloadFromWarehouse = source.PayloadFromWarehouse,
            PayloadFromLocation = source.PayloadFromLocation,
            PayloadToWarehouse = source.PayloadToWarehouse,
            PayloadToLocation = source.PayloadToLocation,
            PayloadQuantity = source.PayloadQuantity,
            ValidationState = source.ValidationState,
            ValidationMessage = source.ValidationMessage,
            ValidationNotes = source.ValidationNotes,
            FuzzyMatchedPartId = source.FuzzyMatchedPartId,
            FuzzyMatchedFromLocation = source.FuzzyMatchedFromLocation,
            FuzzyMatchedToLocation = source.FuzzyMatchedToLocation,
            ExecutionState = source.ExecutionState,
            SentUtc = source.SentUtc,
            FailedUtc = source.FailedUtc,
            IssueType = source.IssueType,
            IssueMessage = source.IssueMessage,
            RetryCount = source.RetryCount,
            LastAttemptUtc = source.LastAttemptUtc,
            CreatedUtc = source.CreatedUtc,
            LastUpdatedUtc = source.LastUpdatedUtc,
        };
    }

    private Model_ScannerBatchItem CreateNewItem()
    {
        var template = SelectedItem ?? Items.FirstOrDefault();

        return new Model_ScannerBatchItem
        {
            ItemId = Guid.NewGuid(),
            SessionId = _session.SessionId,
            PayloadFromWarehouse = template?.PayloadFromWarehouse ?? "002",
            PayloadToWarehouse = template?.PayloadToWarehouse ?? "002",
            PayloadQuantity = "1",
            ValidationState = Enum_ScannerValidationState.NotValidated,
            ExecutionState = Enum_ScannerExecutionState.Waiting,
        };
    }

    private async void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.DataContext is not Model_ScannerBatchItem item)
        {
            return;
        }

        var isFromLocation = string.Equals(textBox.Tag?.ToString(), "From", StringComparison.Ordinal);
        var rawLocation = (isFromLocation ? item.PayloadFromLocation : item.PayloadToLocation)?.Trim() ?? string.Empty;
        var warehouseCode = (isFromLocation ? item.PayloadFromWarehouse : item.PayloadToWarehouse)?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(rawLocation))
        {
            return;
        }

        var validation = await _validationService.ValidateLocationAsync(rawLocation, warehouseCode);
        if (validation.Success && validation.Data?.IsValid == true)
        {
            var canonicalLocation = validation.Data.CanonicalLocation.Trim();
            if (isFromLocation)
            {
                item.PayloadFromLocation = canonicalLocation;
            }
            else
            {
                item.PayloadToLocation = canonicalLocation;
            }

            item.ValidationState = Enum_ScannerValidationState.Valid;
            return;
        }

        item.ValidationState = Enum_ScannerValidationState.Invalid;
        item.ValidationMessage = "Location was not found.";
        item.ValidationNotes = string.IsNullOrWhiteSpace(validation.ErrorMessage)
            ? "Location validation is currently unavailable."
            : validation.ErrorMessage;
    }
}
