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
