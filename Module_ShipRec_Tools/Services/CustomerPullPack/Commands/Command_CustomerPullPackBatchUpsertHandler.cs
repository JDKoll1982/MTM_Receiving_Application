using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Saves one Customer Pull n' Pack waitlist entry per selected source line.
/// </summary>
public class Command_CustomerPullPackBatchUpsertHandler
    : IRequestHandler<
        Command_CustomerPullPackBatchUpsert,
        Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>
    >
{
    private readonly Dao_CustomerPullPackWaitlist _waitlistDao;

    public Command_CustomerPullPackBatchUpsertHandler(Dao_CustomerPullPackWaitlist waitlistDao)
    {
        _waitlistDao = waitlistDao;
    }

    public async Task<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>> Handle(
        Command_CustomerPullPackBatchUpsert request,
        CancellationToken cancellationToken
    )
    {
        var savedEntries = new List<Model_CustomerPullPack_WaitlistEntry>();

        foreach (var entry in request.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var preparedEntry = PrepareEntry(entry);
            if (string.IsNullOrWhiteSpace(preparedEntry.WaitlistId) is false)
            {
                var existingResult = await _waitlistDao.GetByIdAsync(preparedEntry.WaitlistId);
                if (!existingResult.IsSuccess || existingResult.Data is null)
                {
                    return Model_Dao_Result_Factory.Failure<
                        List<Model_CustomerPullPack_WaitlistEntry>
                    >(existingResult.ErrorMessage);
                }

                if (
                    existingResult.Data.CurrentStatus
                    != Enum_CustomerPullPackWaitlistStatus.Requested
                )
                {
                    preparedEntry = PreservePostAcceptanceFields(
                        preparedEntry,
                        existingResult.Data
                    );
                }
            }

            var upsertResult = await _waitlistDao.UpsertAsync(preparedEntry);
            if (!upsertResult.IsSuccess)
            {
                var failureEntries = upsertResult.Data is null
                    ? savedEntries
                    : savedEntries.Concat([upsertResult.Data]).ToList();

                return new Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>
                {
                    Success = false,
                    Data = failureEntries,
                    ErrorMessage = upsertResult.ErrorMessage,
                    Severity = upsertResult.Severity,
                    Exception = upsertResult.Exception,
                    ReturnValue = upsertResult.ReturnValue,
                    AffectedRows = upsertResult.AffectedRows,
                };
            }

            savedEntries.Add(upsertResult.Data ?? preparedEntry);
        }

        return Model_Dao_Result_Factory.Success(savedEntries, savedEntries.Count);
    }

    private static Model_CustomerPullPack_WaitlistEntry PrepareEntry(
        Model_CustomerPullPack_WaitlistEntry entry
    )
    {
        ArgumentNullException.ThrowIfNull(entry);

        var utcNow = DateTime.UtcNow;
        var selectedLocations = entry
            .SelectedLocations.Where(static location =>
                string.IsNullOrWhiteSpace(location) is false
            )
            .Select(static location => location.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = entry.WaitlistId?.Trim() ?? string.Empty,
            SourceLineKey = entry.SourceLineKey?.Trim() ?? string.Empty,
            CustomerId = entry.CustomerId?.Trim().ToUpperInvariant() ?? string.Empty,
            CustomerName = entry.CustomerName?.Trim() ?? string.Empty,
            CustomerOrderId = entry.CustomerOrderId?.Trim().ToUpperInvariant() ?? string.Empty,
            ParentPartId = entry.ParentPartId?.Trim().ToUpperInvariant() ?? string.Empty,
            RequestedQuantity = entry.RequestedQuantity,
            SelectedLocations = selectedLocations,
            RequestedByUserId = entry.RequestedByUserId?.Trim() ?? string.Empty,
            RequestedByDisplayName = string.IsNullOrWhiteSpace(entry.RequestedByDisplayName)
                ? entry.RequestedByUserId?.Trim() ?? string.Empty
                : entry.RequestedByDisplayName.Trim(),
            RequesterContextNote = entry.RequesterContextNote?.Trim() ?? string.Empty,
            CurrentStatus =
                entry.CurrentStatus == default
                    ? Enum_CustomerPullPackWaitlistStatus.Requested
                    : entry.CurrentStatus,
            CurrentOwnerUserId = entry.CurrentOwnerUserId?.Trim() ?? string.Empty,
            CurrentOwnerDisplayName = entry.CurrentOwnerDisplayName?.Trim() ?? string.Empty,
            LocationReviewFlag = selectedLocations.Count == 0,
            ProblemReason = entry.ProblemReason,
            HandlerNote = entry.HandlerNote?.Trim() ?? string.Empty,
            CompletionUserId = entry.CompletionUserId?.Trim() ?? string.Empty,
            CompletionTimestamp = entry.CompletionTimestamp,
            LastUpdatedByUserId = string.IsNullOrWhiteSpace(entry.LastUpdatedByUserId)
                ? entry.RequestedByUserId?.Trim() ?? string.Empty
                : entry.LastUpdatedByUserId.Trim(),
            LastUpdatedTimestamp =
                entry.LastUpdatedTimestamp == default ? utcNow : entry.LastUpdatedTimestamp,
            RequestTimestamp = entry.RequestTimestamp == default ? utcNow : entry.RequestTimestamp,
            RecheckIndicator = entry.RecheckIndicator,
        };
    }

    private static Model_CustomerPullPack_WaitlistEntry PreservePostAcceptanceFields(
        Model_CustomerPullPack_WaitlistEntry preparedEntry,
        Model_CustomerPullPack_WaitlistEntry existingEntry
    )
    {
        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = existingEntry.WaitlistId,
            SourceLineKey = existingEntry.SourceLineKey,
            CustomerId = existingEntry.CustomerId,
            CustomerName = existingEntry.CustomerName,
            CustomerOrderId = existingEntry.CustomerOrderId,
            ParentPartId = existingEntry.ParentPartId,
            RequestedQuantity = existingEntry.RequestedQuantity,
            SelectedLocations = existingEntry.SelectedLocations.ToList(),
            RequestedByUserId = existingEntry.RequestedByUserId,
            RequestedByDisplayName = existingEntry.RequestedByDisplayName,
            RequesterContextNote = preparedEntry.RequesterContextNote,
            CurrentStatus = existingEntry.CurrentStatus,
            CurrentOwnerUserId = existingEntry.CurrentOwnerUserId,
            CurrentOwnerDisplayName = existingEntry.CurrentOwnerDisplayName,
            LocationReviewFlag = existingEntry.LocationReviewFlag,
            ProblemReason = existingEntry.ProblemReason,
            HandlerNote = existingEntry.HandlerNote,
            CompletionUserId = existingEntry.CompletionUserId,
            CompletionTimestamp = existingEntry.CompletionTimestamp,
            LastUpdatedByUserId = preparedEntry.LastUpdatedByUserId,
            LastUpdatedTimestamp = preparedEntry.LastUpdatedTimestamp,
            RequestTimestamp = existingEntry.RequestTimestamp,
            RecheckIndicator = existingEntry.RecheckIndicator,
        };
    }
}
