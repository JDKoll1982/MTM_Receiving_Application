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
    private readonly IReadOnlyList<Model_CustomerPullPack_DemandLine> _selectedLines;

    public Model_CustomerPullPack_DemandLine SelectedLine { get; }

    public IReadOnlyList<Model_CustomerPullPack_DemandLine> SelectedLines => _selectedLines;

    public string RequesterContextNote { get; set; }

    public string DialogTitle =>
        HasExistingLinkedContext ? "Update Waitlist Request" : "Create Waitlist Request";

    public string PrimaryButtonText => HasExistingLinkedContext ? "Save Update" : "Create Request";

    public bool HasExistingLinkedContext => _existingEntry is not null;

    public int ExistingLinkedItemCount => _existingEntry is null ? 0 : 1;

    public int SelectedLineCount => _selectedLines.Count;

    public bool HasLockedOperationalFields =>
        _existingEntry?.CurrentStatus
            is not null
                and not Enum_CustomerPullPackWaitlistStatus.Requested;

    public bool RequiresRequesterNote =>
        _selectedLines.Any(line => line.LocationOptions.Count == 0);

    public string CustomerDisplay
    {
        get
        {
            return string.IsNullOrWhiteSpace(SelectedLine.CustomerName)
                ? SelectedLine.CustomerId
                : $"{SelectedLine.CustomerId} - {SelectedLine.CustomerName}";
        }
    }

    public string ParentPartDisplay =>
        string.Join(
            ", ",
            _selectedLines
                .Select(static line => line.ParentPartId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );

    public string SelectionSummary =>
        $"{SelectedLineCount} selected row{(SelectedLineCount == 1 ? string.Empty : "s")} across {_selectedLines.Sum(static line => line.LocationOptions.Count(static option => option.Selected))} chosen locations.";

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
            _selectedLines.Select(line =>
                line.LocationOptions.Count(static option => option.Selected) == 0
                    ? $"{line.CustomerOrderId} / {line.ParentPartId}: no location selected"
                    : $"{line.CustomerOrderId} / {line.ParentPartId}: {string.Join(", ", line.LocationOptions.Where(static option => option.Selected).Select(static option => option.LocationId))}"
            )
        );

    public ViewModel_Dialog_CustomerPullPackWaitlistEditor(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> selectedLines,
        Model_CustomerPullPack_WaitlistEntry? existingEntry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(selectedLines);
        if (selectedLines.Count == 0)
        {
            throw new ArgumentException(
                "At least one selected line is required.",
                nameof(selectedLines)
            );
        }

        _selectedLines = selectedLines.ToList().AsReadOnly();
        SelectedLine = _selectedLines[0];
        _existingEntry = existingEntry;
        RequesterContextNote = existingEntry?.RequesterContextNote?.Trim() ?? string.Empty;
    }

    public bool ValidateInputs(out string errorMessage)
    {
        foreach (var line in _selectedLines)
        {
            if (
                line.LocationOptions.Count > 0
                && line.LocationOptions.All(static option => option.Selected is false)
            )
            {
                errorMessage =
                    $"Select at least one SUB PARTS ON HAND location for {line.CustomerOrderId} before saving.";
                return false;
            }
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
        return _selectedLines
            .Select(selectedLine =>
            {
                var selectedLocations = selectedLine
                    .LocationOptions.Where(static option => option.Selected)
                    .Select(static option => option.LocationId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new Model_CustomerPullPack_WaitlistEntry
                {
                    WaitlistId = _existingEntry?.WaitlistId ?? selectedLine.LinkedWaitlistId,
                    SourceLineKey = _existingEntry?.SourceLineKey ?? selectedLine.SourceLineKey,
                    CustomerId = _existingEntry?.CustomerId ?? selectedLine.CustomerId,
                    CustomerName = _existingEntry?.CustomerName ?? selectedLine.CustomerName,
                    CustomerOrderId =
                        _existingEntry?.CustomerOrderId ?? selectedLine.CustomerOrderId,
                    ParentPartId = _existingEntry?.ParentPartId ?? selectedLine.ParentPartId,
                    RequestedQuantity = isRequesterOnlyEdit
                        ? _existingEntry!.RequestedQuantity
                        : selectedLine.QuantityToPack,
                    SelectedLocations = isRequesterOnlyEdit
                        ? _existingEntry!.SelectedLocations.ToList()
                        : selectedLocations,
                    RequestedByUserId = _existingEntry?.RequestedByUserId ?? currentUserId,
                    RequestedByDisplayName =
                        _existingEntry?.RequestedByDisplayName ?? currentUserDisplayName,
                    RequesterContextNote = RequesterContextNote.Trim(),
                    CurrentStatus =
                        _existingEntry?.CurrentStatus
                        ?? Enum_CustomerPullPackWaitlistStatus.Requested,
                    CurrentOwnerUserId = _existingEntry?.CurrentOwnerUserId ?? string.Empty,
                    CurrentOwnerDisplayName =
                        _existingEntry?.CurrentOwnerDisplayName ?? string.Empty,
                    LocationReviewFlag = isRequesterOnlyEdit
                        ? _existingEntry!.LocationReviewFlag
                        : selectedLine.LocationOptions.Count == 0,
                    ProblemReason =
                        _existingEntry?.ProblemReason ?? Enum_CustomerPullPackProblemReason.None,
                    HandlerNote = _existingEntry?.HandlerNote ?? string.Empty,
                    CompletionUserId = _existingEntry?.CompletionUserId ?? string.Empty,
                    CompletionTimestamp = _existingEntry?.CompletionTimestamp,
                    LastUpdatedByUserId = currentUserId,
                    LastUpdatedTimestamp = utcNow,
                    RequestTimestamp = _existingEntry?.RequestTimestamp ?? utcNow,
                    RecheckIndicator =
                        _existingEntry?.RecheckIndicator ?? selectedLine.RecheckIndicator,
                };
            })
            .ToList();
    }
}
