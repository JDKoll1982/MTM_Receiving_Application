using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Views;

public sealed partial class View_Scanner_ManageItemsDialog : ContentDialog
{
    private readonly Model_ScannerBatchSession _session;
    private readonly IService_ScannerValidation _validationService;
    private bool _isLocationPickerOpen;

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

    private async void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isLocationPickerOpen)
        {
            return;
        }

        if (sender is not TextBox textBox)
        {
            return;
        }

        if (textBox.DataContext is not Model_ScannerBatchItem item)
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
        if (!validation.Success)
        {
            item.ValidationState = Enum_ScannerValidationState.Invalid;
            item.ValidationMessage = "Location validation unavailable.";
            item.ValidationNotes = string.IsNullOrWhiteSpace(validation.ErrorMessage)
                ? "Location validation is currently unavailable."
                : validation.ErrorMessage;
            return;
        }

        if (validation.Data?.IsValid == true)
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

            return;
        }

        var suggestionsResult = await _validationService.GetLocationSuggestionsAsync(rawLocation, warehouseCode);
        if (suggestionsResult.IsSuccess && suggestionsResult.Data?.Count > 0)
        {
            _isLocationPickerOpen = true;
            try
            {
                var dialog = new Dialog_FuzzySearchPicker(
                    suggestionsResult.Data,
                    "Select Location",
                    $"No exact match was found for '{rawLocation}'. Select a matching location."
                )
                {
                    XamlRoot = textBox.XamlRoot,
                };

                var dialogResult = await dialog.ShowAsync();
                if (
                    dialogResult == ContentDialogResult.Primary
                    && dialog.SelectedResult is not null
                    && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
                )
                {
                    var selectedLocation = dialog.SelectedResult.Label.Trim();
                    if (isFromLocation)
                    {
                        item.PayloadFromLocation = selectedLocation;
                    }
                    else
                    {
                        item.PayloadToLocation = selectedLocation;
                    }

                    return;
                }
            }
            finally
            {
                _isLocationPickerOpen = false;
            }
        }

        var message = validation.Data?.Message ?? "Location is invalid.";
        if (!suggestionsResult.IsSuccess && string.IsNullOrWhiteSpace(suggestionsResult.ErrorMessage) is false)
        {
            message = $"{message} {suggestionsResult.ErrorMessage}";
        }

        item.ValidationState = Enum_ScannerValidationState.Invalid;
        item.ValidationMessage = "Location validation failed.";
        item.ValidationNotes = message;
    }
}