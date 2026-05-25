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
    private readonly IReadOnlyDictionary<
        string,
        Model_CustomerPullPack_WaitlistEntry
    > _existingEntries;

    public ObservableCollection<Model_CustomerPullPack_DemandLine> SelectedLines { get; }

    public string RequesterContextNote { get; set; }

    public string DialogTitle =>
        HasExistingLinkedContext ? "Update Waitlist Request" : "Create Waitlist Request";

    public string PrimaryButtonText => HasExistingLinkedContext ? "Save Update" : "Create Request";

    public bool HasExistingLinkedContext => _existingEntries.Count > 0;

    public int ExistingLinkedItemCount => _existingEntries.Count;

    public bool HasLockedOperationalFields =>
        _existingEntries.Values.Any(static entry =>
            entry.CurrentStatus != Enum_CustomerPullPackWaitlistStatus.Requested
        );

    public bool RequiresRequesterNote => SelectedLines.Any(line => line.LocationOptions.Count == 0);

    public string CustomerDisplay
    {
        get
        {
            var firstLine = SelectedLines.FirstOrDefault();
            if (firstLine is null)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(firstLine.CustomerName)
                ? firstLine.CustomerId
                : $"{firstLine.CustomerId} - {firstLine.CustomerName}";
        }
    }

    public string ParentPartDisplay =>
        string.Join(
            ", ",
            SelectedLines
                .Select(static line => line.ParentPartId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );

    public string SelectionSummary =>
        $"{SelectedLines.Count} selected lines across {SelectedLines.SelectMany(static line => line.LocationOptions).Count(static option => option.Selected)} chosen locations.";

    public string InstructionText =>
        HasExistingLinkedContext
            ? "The location choices stay on the report. Use this window to confirm the request context and update requester-facing notes."
            : "The location choices stay on the report. Use this window to confirm the request context and add requester-facing notes before saving.";

    public string LockedOperationalFieldMessage =>
        HasLockedOperationalFields
            ? "This linked item has already been accepted or progressed. Requester edits are limited to requester-facing notes and must not replace the current owner or handler-managed fields."
            : string.Empty;

    public string LocationCoverageSummary =>
        string.Join(
            Environment.NewLine,
            SelectedLines.Select(line =>
            {
                var selectedLocations = line
                    .LocationOptions.Where(static option => option.Selected)
                    .Select(static option => option.LocationId)
                    .ToList();

                return selectedLocations.Count == 0
                    ? $"{line.CustomerOrderId} / {line.ParentPartId}: no location selected"
                    : $"{line.CustomerOrderId} / {line.ParentPartId}: {string.Join(", ", selectedLocations)}";
            })
        );

    public ViewModel_Dialog_CustomerPullPackWaitlistEditor(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines,
        IReadOnlyDictionary<string, Model_CustomerPullPack_WaitlistEntry> existingEntries,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(selectedLines);
        ArgumentNullException.ThrowIfNull(existingEntries);

        SelectedLines = new ObservableCollection<Model_CustomerPullPack_DemandLine>(selectedLines);
        _existingEntries = existingEntries;
        RequesterContextNote = ResolveInitialRequesterNote(existingEntries);
    }

    public bool ValidateInputs(out string errorMessage)
    {
        if (SelectedLines.Count == 0)
        {
            errorMessage =
                "Select at least one compatible report line before opening the waitlist editor.";
            return false;
        }

        var lineMissingRequiredSelection = SelectedLines.FirstOrDefault(line =>
            line.LocationOptions.Count > 0
            && line.LocationOptions.All(static option => option.Selected is false)
        );
        if (lineMissingRequiredSelection is not null)
        {
            errorMessage =
                $"Select at least one SUB PARTS ON HAND location for {lineMissingRequiredSelection.CustomerOrderId} before saving.";
            return false;
        }

        if (RequiresRequesterNote && string.IsNullOrWhiteSpace(RequesterContextNote))
        {
            errorMessage =
                "A requester note is required when a selected line has no selectable part locations.";
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

        return SelectedLines
            .Select(line =>
            {
                _existingEntries.TryGetValue(line.SourceLineKey, out var existingEntry);
                var isRequesterOnlyEdit =
                    existingEntry?.CurrentStatus
                    is not null
                        and not Enum_CustomerPullPackWaitlistStatus.Requested;
                var selectedLocations = line
                    .LocationOptions.Where(static option => option.Selected)
                    .Select(static option => option.LocationId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new Model_CustomerPullPack_WaitlistEntry
                {
                    WaitlistId = existingEntry?.WaitlistId ?? line.LinkedWaitlistId,
                    SourceLineKey = existingEntry?.SourceLineKey ?? line.SourceLineKey,
                    CustomerId = existingEntry?.CustomerId ?? line.CustomerId,
                    CustomerName = existingEntry?.CustomerName ?? line.CustomerName,
                    CustomerOrderId = existingEntry?.CustomerOrderId ?? line.CustomerOrderId,
                    ParentPartId = existingEntry?.ParentPartId ?? line.ParentPartId,
                    RequestedQuantity = isRequesterOnlyEdit
                        ? existingEntry!.RequestedQuantity
                        : line.QuantityToPack,
                    SelectedLocations = isRequesterOnlyEdit
                        ? existingEntry!.SelectedLocations.ToList()
                        : selectedLocations,
                    RequestedByUserId = existingEntry?.RequestedByUserId ?? currentUserId,
                    RequestedByDisplayName =
                        existingEntry?.RequestedByDisplayName ?? currentUserDisplayName,
                    RequesterContextNote = RequesterContextNote.Trim(),
                    CurrentStatus =
                        existingEntry?.CurrentStatus
                        ?? Enum_CustomerPullPackWaitlistStatus.Requested,
                    CurrentOwnerUserId = existingEntry?.CurrentOwnerUserId ?? string.Empty,
                    CurrentOwnerDisplayName =
                        existingEntry?.CurrentOwnerDisplayName ?? string.Empty,
                    LocationReviewFlag = isRequesterOnlyEdit
                        ? existingEntry!.LocationReviewFlag
                        : line.LocationOptions.Count == 0,
                    ProblemReason =
                        existingEntry?.ProblemReason ?? Enum_CustomerPullPackProblemReason.None,
                    HandlerNote = existingEntry?.HandlerNote ?? string.Empty,
                    CompletionUserId = existingEntry?.CompletionUserId ?? string.Empty,
                    CompletionTimestamp = existingEntry?.CompletionTimestamp,
                    LastUpdatedByUserId = currentUserId,
                    LastUpdatedTimestamp = utcNow,
                    RequestTimestamp = existingEntry?.RequestTimestamp ?? utcNow,
                    RecheckIndicator = existingEntry?.RecheckIndicator ?? line.RecheckIndicator,
                };
            })
            .ToList();
    }

    private static string ResolveInitialRequesterNote(
        IReadOnlyDictionary<string, Model_CustomerPullPack_WaitlistEntry> existingEntries
    )
    {
        if (existingEntries.Count == 0)
        {
            return string.Empty;
        }

        var distinctNotes = existingEntries
            .Values.Select(static entry => entry.RequesterContextNote?.Trim() ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return distinctNotes.Count == 1 ? distinctNotes[0] : string.Empty;
    }
}
