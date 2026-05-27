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
/// Removes the current owner from a queue item while preserving its workflow state.
/// </summary>
public sealed class Command_CustomerPullPackUnassignOwnerHandler
    : IRequestHandler<
        Command_CustomerPullPackUnassignOwner,
        Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
    >
{
    private readonly IService_CustomerPullPackWaitlistSource _waitlistSource;

    public Command_CustomerPullPackUnassignOwnerHandler(
        IService_CustomerPullPackWaitlistSource waitlistSource
    )
    {
        _waitlistSource = waitlistSource;
    }

    public async Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> Handle(
        Command_CustomerPullPackUnassignOwner request,
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
            string.IsNullOrWhiteSpace(existingResult.Data.CurrentOwnerUserId)
            || string.Equals(
                existingResult.Data.CurrentOwnerUserId,
                request.CurrentUserId,
                StringComparison.OrdinalIgnoreCase
            )
                is false
        )
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                "Only the current owner can un-assign this waitlist item."
            );
        }

        var utcNow = DateTime.UtcNow;
        var updatedEntry = new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = existingResult.Data.WaitlistId,
            SourceLineKey = existingResult.Data.SourceLineKey,
            CustomerId = existingResult.Data.CustomerId,
            CustomerName = existingResult.Data.CustomerName,
            CustomerOrderId = existingResult.Data.CustomerOrderId,
            ParentPartId = existingResult.Data.ParentPartId,
            RequestedQuantity = existingResult.Data.RequestedQuantity,
            SelectedLocations = existingResult.Data.SelectedLocations,
            RequestedByUserId = existingResult.Data.RequestedByUserId,
            RequestedByDisplayName = existingResult.Data.RequestedByDisplayName,
            RequesterContextNote = existingResult.Data.RequesterContextNote,
            CurrentStatus =
                existingResult.Data.CurrentStatus == Enum_CustomerPullPackWaitlistStatus.Accepted
                    ? Enum_CustomerPullPackWaitlistStatus.Requested
                    : existingResult.Data.CurrentStatus,
            CurrentOwnerUserId = string.Empty,
            CurrentOwnerDisplayName = string.Empty,
            LocationReviewFlag = existingResult.Data.LocationReviewFlag,
            ProblemReason = existingResult.Data.ProblemReason,
            HandlerNote = existingResult.Data.HandlerNote,
            CompletionUserId = existingResult.Data.CompletionUserId,
            CompletionTimestamp = existingResult.Data.CompletionTimestamp,
            LastUpdatedByUserId = request.CurrentUserId.Trim(),
            LastUpdatedTimestamp = utcNow,
            RequestTimestamp = existingResult.Data.RequestTimestamp,
            RecheckIndicator = existingResult.Data.RecheckIndicator,
        };

        return await _waitlistSource.UpsertAsync(updatedEntry);
    }
}
