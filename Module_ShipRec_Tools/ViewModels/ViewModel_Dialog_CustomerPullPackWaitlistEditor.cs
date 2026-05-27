using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the requester-side Customer Pull n' Pack waitlist confirmation editor.
/// </summary>
public sealed partial class ViewModel_Dialog_CustomerPullPackWaitlistEditor : ViewModel_Shared_Base
{
    private readonly Model_CustomerPullPack_WaitlistEntry? _existingEntry;

    public Model_CustomerPullPack_DemandLine SelectedLine { get; }

    public string RequesterContextNote { get; set; }

    public string DialogTitle =>
        HasExistingLinkedContext ? "Update Waitlist Request" : "Create Waitlist Request";

    public string PrimaryButtonText => HasExistingLinkedContext ? "Save Update" : "Create Request";

    public bool HasExistingLinkedContext => _existingEntry is not null;

    public int ExistingLinkedItemCount => _existingEntry is null ? 0 : 1;

    public bool HasLockedOperationalFields =>
        _existingEntry?.CurrentStatus
            is not null
                and not Enum_CustomerPullPackWaitlistStatus.Requested;

    public bool RequiresRequesterNote => SelectedLine.LocationOptions.Count == 0;

    public string CustomerDisplay
    {
        get
        {
            return string.IsNullOrWhiteSpace(SelectedLine.CustomerName)
                ? SelectedLine.CustomerId
                : $"{SelectedLine.CustomerId} - {SelectedLine.CustomerName}";
        }
    }

    public string ParentPartDisplay => SelectedLine.ParentPartId;

    public string SelectionSummary =>
        $"1 selected row across {SelectedLine.LocationOptions.Count(static option => option.Selected)} chosen locations.";

    public string InstructionText =>
        HasExistingLinkedContext
            ? "The location choices stay on the report. Use this window to confirm the request context and update requester-facing notes."
            : "The location choices stay on the report. Use this window to confirm the request context and add requester-facing notes before saving.";

    public string LockedOperationalFieldMessage =>
        HasLockedOperationalFields
            ? "This linked item has already been accepted or progressed. Requester edits are limited to requester-facing notes and must not replace the current owner or handler-managed fields."
            : string.Empty;

    public string LocationCoverageSummary =>
        SelectedLine.LocationOptions.Count(static option => option.Selected) == 0
            ? $"{SelectedLine.CustomerOrderId} / {SelectedLine.ParentPartId}: no location selected"
            : $"{SelectedLine.CustomerOrderId} / {SelectedLine.ParentPartId}: {string.Join(", ", SelectedLine.LocationOptions.Where(static option => option.Selected).Select(static option => option.LocationId))}";

    public ViewModel_Dialog_CustomerPullPackWaitlistEditor(
        Model_CustomerPullPack_DemandLine selectedLine,
        Model_CustomerPullPack_WaitlistEntry? existingEntry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(selectedLine);

        SelectedLine = selectedLine;
        _existingEntry = existingEntry;
        RequesterContextNote = existingEntry?.RequesterContextNote?.Trim() ?? string.Empty;
    }

    public bool ValidateInputs(out string errorMessage)
    {
        if (
            SelectedLine.LocationOptions.Count > 0
            && SelectedLine.LocationOptions.All(static option => option.Selected is false)
        )
        {
            errorMessage =
                $"Select at least one SUB PARTS ON HAND location for {SelectedLine.CustomerOrderId} before saving.";
            return false;
        }

        if (RequiresRequesterNote && string.IsNullOrWhiteSpace(RequesterContextNote))
        {
            errorMessage =
                "A requester note is required when the selected row has no selectable part locations.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public IReadOnlyList<Model_CustomerPullPack_WaitlistEntry> BuildBatchEntries(
        string currentUserId,
        string currentUserDisplayName
    )
    {
        var utcNow = DateTime.UtcNow;

        var isRequesterOnlyEdit =
            _existingEntry?.CurrentStatus
            is not null
                and not Enum_CustomerPullPackWaitlistStatus.Requested;
        var selectedLocations = SelectedLine
            .LocationOptions.Where(static option => option.Selected)
            .Select(static option => option.LocationId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return
        [
            new Model_CustomerPullPack_WaitlistEntry
            {
                WaitlistId = _existingEntry?.WaitlistId ?? SelectedLine.LinkedWaitlistId,
                SourceLineKey = _existingEntry?.SourceLineKey ?? SelectedLine.SourceLineKey,
                CustomerId = _existingEntry?.CustomerId ?? SelectedLine.CustomerId,
                CustomerName = _existingEntry?.CustomerName ?? SelectedLine.CustomerName,
                CustomerOrderId = _existingEntry?.CustomerOrderId ?? SelectedLine.CustomerOrderId,
                ParentPartId = _existingEntry?.ParentPartId ?? SelectedLine.ParentPartId,
                RequestedQuantity = isRequesterOnlyEdit
                    ? _existingEntry!.RequestedQuantity
                    : SelectedLine.QuantityToPack,
                SelectedLocations = isRequesterOnlyEdit
                    ? _existingEntry!.SelectedLocations.ToList()
                    : selectedLocations,
                RequestedByUserId = _existingEntry?.RequestedByUserId ?? currentUserId,
                RequestedByDisplayName =
                    _existingEntry?.RequestedByDisplayName ?? currentUserDisplayName,
                RequesterContextNote = RequesterContextNote.Trim(),
                CurrentStatus =
                    _existingEntry?.CurrentStatus ?? Enum_CustomerPullPackWaitlistStatus.Requested,
                CurrentOwnerUserId = _existingEntry?.CurrentOwnerUserId ?? string.Empty,
                CurrentOwnerDisplayName = _existingEntry?.CurrentOwnerDisplayName ?? string.Empty,
                LocationReviewFlag = isRequesterOnlyEdit
                    ? _existingEntry!.LocationReviewFlag
                    : SelectedLine.LocationOptions.Count == 0,
                ProblemReason =
                    _existingEntry?.ProblemReason ?? Enum_CustomerPullPackProblemReason.None,
                HandlerNote = _existingEntry?.HandlerNote ?? string.Empty,
                CompletionUserId = _existingEntry?.CompletionUserId ?? string.Empty,
                CompletionTimestamp = _existingEntry?.CompletionTimestamp,
                LastUpdatedByUserId = currentUserId,
                LastUpdatedTimestamp = utcNow,
                RequestTimestamp = _existingEntry?.RequestTimestamp ?? utcNow,
                RecheckIndicator =
                    _existingEntry?.RecheckIndicator ?? SelectedLine.RecheckIndicator,
            },
        ];
    }
}
