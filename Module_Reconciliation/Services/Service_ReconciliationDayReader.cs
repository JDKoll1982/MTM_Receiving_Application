using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Loads one day and answers where the neighbouring active days are. Activity is
/// decided by receiving rows alone, never by Infor Visual transfers, and day
/// stepping never runs a preview (FR-009, FR-010).
/// </summary>
public sealed class Service_ReconciliationDayReader : IService_ReconciliationDayReader
{
    private const int NearestDaySearchWindow = 370;

    private readonly Dao_ReconciliationReceivingRows _receivingRows;
    private readonly IService_LoggingUtility? _logger;

    /// <summary>Creates the reader over its own receiving-row data access.</summary>
    /// <param name="receivingRows">The module's receiving-row data access.</param>
    /// <param name="logger">An optional logger.</param>
    public Service_ReconciliationDayReader(
        Dao_ReconciliationReceivingRows receivingRows,
        IService_LoggingUtility? logger = null
    )
    {
        _receivingRows = receivingRows;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_ReconciliationDay>> GetDayAsync(DateOnly date)
    {
        Model_Dao_Result<List<Model_ReconciliationRow>> rowsResult =
            await _receivingRows.GetDayRowsAsync(date);

        if (rowsResult.IsSuccess is false)
        {
            return Model_Dao_Result_Factory.Failure<Model_ReconciliationDay>(
                $"The day {date:yyyy-MM-dd} could not be loaded: {rowsResult.ErrorMessage}",
                rowsResult.Exception
            );
        }

        List<Model_ReconciliationRow> allRows = rowsResult.Data ?? [];
        List<Model_ReconciliationRow> candidates = allRows
            .Where(row => row.IsReprint is false)
            .ToList();
        int excludedReprintCount = allRows.Count - candidates.Count;

        if (excludedReprintCount > 0)
        {
            _logger?.LogInfo(
                $"Reconciliation day {date:yyyy-MM-dd} excluded {excludedReprintCount} reprint row(s) from the candidate set."
            );
        }

        var day = new Model_ReconciliationDay
        {
            Date = date,
            HasActivity = allRows.Count > 0,
            Rows = candidates,
            State = Enum_ReconciliationDayState.Loaded,
            Summary = new Model_ReconciliationDaySummary
            {
                Scanned = candidates.Count,
                ExcludedReprintCount = excludedReprintCount,
            },
        };

        return Model_Dao_Result_Factory.Success(day);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<IReadOnlyList<DateOnly>>> GetActivityDatesAsync(
        DateOnly from,
        DateOnly to
    )
    {
        Model_Dao_Result<List<DateOnly>> result = await _receivingRows.GetActivityDatesAsync(
            from,
            to
        );

        return result.IsSuccess
            ? Model_Dao_Result_Factory.Success<IReadOnlyList<DateOnly>>(result.Data ?? [])
            : Model_Dao_Result_Factory.Failure<IReadOnlyList<DateOnly>>(
                result.ErrorMessage,
                result.Exception
            );
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<DateOnly?>> FindNearestActiveDayAsync(
        DateOnly from,
        int direction
    )
    {
        if (direction is not (-1 or 1))
        {
            return Model_Dao_Result_Factory.Failure<DateOnly?>(
                "Direction must be -1 for an earlier day or 1 for a later day."
            );
        }

        DateOnly rangeStart =
            direction < 0 ? from.AddDays(-NearestDaySearchWindow) : from.AddDays(1);
        DateOnly rangeEnd =
            direction < 0 ? from.AddDays(-1) : from.AddDays(NearestDaySearchWindow);

        Model_Dao_Result<List<DateOnly>> result = await _receivingRows.GetActivityDatesAsync(
            rangeStart,
            rangeEnd
        );

        if (result.IsSuccess is false)
        {
            return Model_Dao_Result_Factory.Failure<DateOnly?>(
                result.ErrorMessage,
                result.Exception
            );
        }

        List<DateOnly> activityDates = result.Data ?? [];

        if (activityDates.Count == 0)
        {
            return Model_Dao_Result_Factory.Success<DateOnly?>(null);
        }

        DateOnly nearest = direction < 0 ? activityDates.Max() : activityDates.Min();

        return Model_Dao_Result_Factory.Success<DateOnly?>(nearest);
    }
}
