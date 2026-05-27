using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Applies queue-side status transitions for Customer Pull n' Pack waitlist items.
/// </summary>
public sealed class Command_CustomerPullPackUpdateStatusHandler
    : IRequestHandler<
        Command_CustomerPullPackUpdateStatus,
        Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
    >
{
    private readonly IService_CustomerPullPackWaitlistSource _waitlistSource;

    public Command_CustomerPullPackUpdateStatusHandler(
        IService_CustomerPullPackWaitlistSource waitlistSource
    )
    {
        _waitlistSource = waitlistSource;
    }

    public async Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> Handle(
        Command_CustomerPullPackUpdateStatus request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingResult = await _waitlistSource.GetByIdAsync(request.WaitlistId);
        if (!existingResult.IsSuccess || existingResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                existingResult.ErrorMessage,
                existingResult.Exception
            );
        }

        if (
            request.NewStatus == Enum_CustomerPullPackWaitlistStatus.Problem
            && request.ProblemReason == Enum_CustomerPullPackProblemReason.None
            && string.IsNullOrWhiteSpace(request.HandlerNote)
        )
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                "Problem status requires a preset reason or a handler note."
            );
        }

        var updatedEntry = BuildUpdatedEntry(existingResult.Data, request);
        return await _waitlistSource.UpsertAsync(updatedEntry);
    }

    private static Model_CustomerPullPack_WaitlistEntry BuildUpdatedEntry(
        Model_CustomerPullPack_WaitlistEntry existingEntry,
        Command_CustomerPullPackUpdateStatus request
    )
    {
        var utcNow = DateTime.UtcNow;
        var status = request.NewStatus;
        var currentUserId = request.CurrentUserId.Trim();
        var currentUserDisplayName = string.IsNullOrWhiteSpace(request.CurrentUserDisplayName)
            ? currentUserId
            : request.CurrentUserDisplayName.Trim();

        var ownerUserId = existingEntry.CurrentOwnerUserId;
        var ownerDisplayName = existingEntry.CurrentOwnerDisplayName;

        switch (status)
        {
            case Enum_CustomerPullPackWaitlistStatus.Accepted:
                ownerUserId = currentUserId;
                ownerDisplayName = currentUserDisplayName;
                break;
            case Enum_CustomerPullPackWaitlistStatus.Requested:
                ownerUserId = string.Empty;
                ownerDisplayName = string.Empty;
                break;
        }

        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = existingEntry.WaitlistId,
            SourceLineKey = existingEntry.SourceLineKey,
            CustomerId = existingEntry.CustomerId,
            CustomerName = existingEntry.CustomerName,
            CustomerOrderId = existingEntry.CustomerOrderId,
            ParentPartId = existingEntry.ParentPartId,
            RequestedQuantity = existingEntry.RequestedQuantity,
            SelectedLocations = existingEntry.SelectedLocations,
            RequestedByUserId = existingEntry.RequestedByUserId,
            RequestedByDisplayName = existingEntry.RequestedByDisplayName,
            RequesterContextNote = existingEntry.RequesterContextNote,
            CurrentStatus = status,
            CurrentOwnerUserId = ownerUserId,
            CurrentOwnerDisplayName = ownerDisplayName,
            LocationReviewFlag = existingEntry.LocationReviewFlag,
            ProblemReason =
                status == Enum_CustomerPullPackWaitlistStatus.Problem
                    ? request.ProblemReason
                    : Enum_CustomerPullPackProblemReason.None,
            HandlerNote = request.HandlerNote?.Trim() ?? string.Empty,
            CompletionUserId = status
                is Enum_CustomerPullPackWaitlistStatus.Completed
                    or Enum_CustomerPullPackWaitlistStatus.Cancelled
                ? currentUserId
                : string.Empty,
            CompletionTimestamp = status
                is Enum_CustomerPullPackWaitlistStatus.Completed
                    or Enum_CustomerPullPackWaitlistStatus.Cancelled
                ? utcNow
                : null,
            LastUpdatedByUserId = currentUserId,
            LastUpdatedTimestamp = utcNow,
            RequestTimestamp = existingEntry.RequestTimestamp,
            RecheckIndicator = false,
        };
    }
}
