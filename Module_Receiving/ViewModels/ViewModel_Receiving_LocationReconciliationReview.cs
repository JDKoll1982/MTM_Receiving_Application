using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels;

public partial class ViewModel_Receiving_LocationReconciliationReview : ViewModel_Shared_Base
{
    private readonly IService_ReceivingLocationReconciliation _locationReconciliationService;
    private readonly Queue<Model_ReceivingLocationReconciliationItem> _pendingItems = new();

    [ObservableProperty]
    private Model_ReceivingLocationReconciliationSummary? _previewSummary;

    [ObservableProperty]
    private Model_ReceivingLocationReconciliationItem? _currentItem;

    [ObservableProperty]
    private bool _isSummaryVisible;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private int _savedCount;

    [ObservableProperty]
    private int _ignoredCount;

    public ViewModel_Receiving_LocationReconciliationReview(
        IService_ReceivingLocationReconciliation locationReconciliationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _locationReconciliationService = locationReconciliationService;
    }

    public ObservableCollection<Model_ReceivingLocationReconciliationItem> SavedItems { get; } = [];

    public ObservableCollection<Model_ReceivingLocationReconciliationItem> IgnoredItems { get; } = [];

    public int TotalCandidateCount => PreviewSummary?.UpdatedItems.Count ?? 0;

    public int NeedsAttentionCount => PreviewSummary?.UnresolvedItems.Count ?? 0;

    public int ItemsRemainingCount => _pendingItems.Count + (CurrentItem == null ? 0 : 1);

    public bool HasPendingItems => !IsSummaryVisible && CurrentItem != null;

    public bool HasSavedItems => SavedItems.Count > 0;

    public bool HasIgnoredItems => IgnoredItems.Count > 0;

    public bool HasUnresolvedItems => NeedsAttentionCount > 0;

    public string DialogTitle => IsSummaryVisible
        ? "InforVisual Reconciliation Summary"
        : "Review InforVisual Location Changes";

    public string DialogDescription => IsSummaryVisible
        ? "All rows that needed review have been saved or ignored. Review the outcome before closing this window."
        : "Review each suggested location update before it is saved to MTM. Save applies the new location. Ignore leaves the saved row unchanged.";

    public string ProgressText => TotalCandidateCount == 0
        ? "No saved rows need a location change review."
        : $"{SavedCount + IgnoredCount + 1} of {TotalCandidateCount}";

    public double ProgressPercent => TotalCandidateCount == 0
        ? 0
        : ((double)(SavedCount + IgnoredCount) / TotalCandidateCount) * 100;

    public string CurrentPartIdText => CurrentItem?.PartID ?? string.Empty;

    public string CurrentOriginalLocationText => ValueOrUnknown(CurrentItem?.ExistingLocation);

    public string CurrentNewLocationText => ValueOrUnknown(CurrentItem?.ProposedLocation);

    public string CurrentSavedRowQuantityText => CurrentItem == null
        ? "Unknown"
        : string.IsNullOrWhiteSpace(CurrentItem.QuantityUnitOfMeasure)
            ? $"{CurrentItem.SavedRowQuantity:0.##}"
            : $"{CurrentItem.SavedRowQuantity:0.##} {CurrentItem.QuantityUnitOfMeasure}";

    public string CurrentQuantityMovedText => CurrentItem == null
        ? "Unknown"
        : string.IsNullOrWhiteSpace(CurrentItem.QuantityUnitOfMeasure)
            ? $"{CurrentItem.MatchedLocationQuantity:0.##}"
            : $"{CurrentItem.MatchedLocationQuantity:0.##} {CurrentItem.QuantityUnitOfMeasure}";

    public string CurrentAllocationMethodText => ValueOrUnknown(CurrentItem?.AllocationMethod);

    public string CurrentMovedByUserIdText => ValueOrUnknown(CurrentItem?.MovedByUserId);

    public string CurrentMovedDateText => CurrentItem?.MovedAt?.ToLocalTime().ToString("M/d/yyyy")
        ?? "Unknown";

    public string CurrentMovedTimeText => CurrentItem?.MovedAt?.ToLocalTime().ToString("h:mm tt")
        ?? "Unknown";

    public string CurrentPurchaseOrderText => CurrentItem == null
        ? string.Empty
        : string.IsNullOrWhiteSpace(CurrentItem.POLineNumber)
            ? CurrentItem.PONumber
            : $"{CurrentItem.PONumber} / Line {CurrentItem.POLineNumber}";

    public string CurrentDataSourceText => CurrentItem?.DataSource ?? string.Empty;

    public string CurrentReasonText => CurrentItem?.Details ?? string.Empty;

    public string SummaryHeadline => SavedCount > 0
        ? "Reconciliation review completed"
        : "No location changes were saved";

    public string SummaryDescription =>
        $"Saved {SavedCount} row(s), ignored {IgnoredCount} row(s), left {PreviewSummary?.UnchangedCount ?? 0} unchanged, and found {NeedsAttentionCount} row(s) that still need manual attention.";

    public string SavedSectionTitle => $"Saved Changes ({SavedCount})";

    public string IgnoredSectionTitle => $"Ignored Changes ({IgnoredCount})";

    public string UnresolvedSectionTitle => $"Needs Attention ({NeedsAttentionCount})";

    public bool CanProcessCurrentItem => !IsProcessing && HasPendingItems;

    public void Initialize(Model_ReceivingLocationReconciliationSummary previewSummary)
    {
        ArgumentNullException.ThrowIfNull(previewSummary);

        PreviewSummary = previewSummary;
        SavedItems.Clear();
        IgnoredItems.Clear();
        _pendingItems.Clear();
        SavedCount = 0;
        IgnoredCount = 0;
        IsSummaryVisible = false;
        IsProcessing = false;

        foreach (var item in previewSummary.UpdatedItems)
        {
            _pendingItems.Enqueue(item);
        }

        MoveToNextItem();
        RaiseComputedPropertyChanges();
    }

    [RelayCommand(CanExecute = nameof(CanProcessCurrentItem))]
    private async Task SaveCurrentAsync()
    {
        if (CurrentItem == null)
        {
            return;
        }

        IsProcessing = true;
        SaveCurrentCommand.NotifyCanExecuteChanged();
        IgnoreCurrentCommand.NotifyCanExecuteChanged();

        try
        {
            var result = await _locationReconciliationService.ApplyLocationUpdateAsync(CurrentItem);
            if (!result.IsSuccess)
            {
                await _errorHandler.ShowErrorDialogAsync(
                    "Unable to Save Reconciled Location",
                    result.ErrorMessage,
                    Enum_ErrorSeverity.Error
                );
                return;
            }

            SavedItems.Add(CurrentItem);
            SavedCount++;
            MoveToNextItem();
        }
        finally
        {
            IsProcessing = false;
            SaveCurrentCommand.NotifyCanExecuteChanged();
            IgnoreCurrentCommand.NotifyCanExecuteChanged();
            RaiseComputedPropertyChanges();
        }
    }

    [RelayCommand(CanExecute = nameof(CanProcessCurrentItem))]
    private void IgnoreCurrent()
    {
        if (CurrentItem == null)
        {
            return;
        }

        IgnoredItems.Add(CurrentItem);
        IgnoredCount++;
        MoveToNextItem();
        RaiseComputedPropertyChanges();
    }

    partial void OnCurrentItemChanged(Model_ReceivingLocationReconciliationItem? value)
    {
        RaiseComputedPropertyChanges();
        SaveCurrentCommand.NotifyCanExecuteChanged();
        IgnoreCurrentCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSummaryVisibleChanged(bool value)
    {
        RaiseComputedPropertyChanges();
    }

    private void MoveToNextItem()
    {
        if (_pendingItems.Count == 0)
        {
            CurrentItem = null;
            IsSummaryVisible = true;
            return;
        }

        CurrentItem = _pendingItems.Dequeue();
    }

    private void RaiseComputedPropertyChanges()
    {
        OnPropertyChanged(nameof(TotalCandidateCount));
        OnPropertyChanged(nameof(NeedsAttentionCount));
        OnPropertyChanged(nameof(ItemsRemainingCount));
        OnPropertyChanged(nameof(HasPendingItems));
        OnPropertyChanged(nameof(HasSavedItems));
        OnPropertyChanged(nameof(HasIgnoredItems));
        OnPropertyChanged(nameof(HasUnresolvedItems));
        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(DialogDescription));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(CurrentPartIdText));
        OnPropertyChanged(nameof(CurrentOriginalLocationText));
        OnPropertyChanged(nameof(CurrentNewLocationText));
        OnPropertyChanged(nameof(CurrentSavedRowQuantityText));
        OnPropertyChanged(nameof(CurrentQuantityMovedText));
        OnPropertyChanged(nameof(CurrentAllocationMethodText));
        OnPropertyChanged(nameof(CurrentMovedByUserIdText));
        OnPropertyChanged(nameof(CurrentMovedDateText));
        OnPropertyChanged(nameof(CurrentMovedTimeText));
        OnPropertyChanged(nameof(CurrentPurchaseOrderText));
        OnPropertyChanged(nameof(CurrentDataSourceText));
        OnPropertyChanged(nameof(CurrentReasonText));
        OnPropertyChanged(nameof(SummaryHeadline));
        OnPropertyChanged(nameof(SummaryDescription));
        OnPropertyChanged(nameof(SavedSectionTitle));
        OnPropertyChanged(nameof(IgnoredSectionTitle));
        OnPropertyChanged(nameof(UnresolvedSectionTitle));
        OnPropertyChanged(nameof(CanProcessCurrentItem));
    }

    private static string ValueOrUnknown(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
    }
}