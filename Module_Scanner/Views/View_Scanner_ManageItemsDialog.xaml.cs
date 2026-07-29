using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Views;

public sealed partial class View_Scanner_ManageItemsDialog : ContentDialog
{
    private readonly Model_ScannerBatchSession _session;

    public ObservableCollection<Model_ScannerBatchItem> Items { get; } = [];

    public Model_ScannerBatchItem? SelectedItem { get; set; }

    public View_Scanner_ManageItemsDialog(Model_ScannerBatchSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
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
            ExternalRecordKey = source.ExternalRecordKey,
            PayloadPartId = source.PayloadPartId,
            PayloadFromWarehouse = source.PayloadFromWarehouse,
            PayloadFromLocation = source.PayloadFromLocation,
            PayloadToWarehouse = source.PayloadToWarehouse,
            PayloadToLocation = source.PayloadToLocation,
            PayloadQuantity = source.PayloadQuantity,
            PayloadUnitOfMeasure = source.PayloadUnitOfMeasure,
            PayloadLotOrSerial = source.PayloadLotOrSerial,
            PayloadReferenceText = source.PayloadReferenceText,
            NavigationPattern = source.NavigationPattern,
            PreSendDelayMs = source.PreSendDelayMs,
            DelayBetweenFieldsMs = source.DelayBetweenFieldsMs,
            PostSendDelayMs = source.PostSendDelayMs,
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
            IsLockedAfterSend = source.IsLockedAfterSend,
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
}