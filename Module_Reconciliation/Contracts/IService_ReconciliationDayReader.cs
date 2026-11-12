using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Loads one day's rows and answers where the neighbouring active days are.
/// "No activity" means no receiving rows, never no Infor Visual transfers (FR-010).
/// </summary>
public interface IService_ReconciliationDayReader
{
    /// <summary>Loads the day's candidate rows and returns it in state Loaded.</summary>
    /// <param name="date">The calendar day to load.</param>
    Task<Model_Dao_Result<Model_ReconciliationDay>> GetDayAsync(DateOnly date);

    /// <summary>The days in the range that hold at least one receiving row.</summary>
    /// <param name="from">Range start, inclusive.</param>
    /// <param name="to">Range end, inclusive.</param>
    Task<Model_Dao_Result<IReadOnlyList<DateOnly>>> GetActivityDatesAsync(DateOnly from, DateOnly to);

    /// <summary>
    /// The nearest day in <paramref name="direction"/> that has activity, or null
    /// when the range is exhausted. Never runs a preview (FR-009).
    /// </summary>
    /// <param name="from">The day to step away from.</param>
    /// <param name="direction">-1 for earlier, 1 for later.</param>
    Task<Model_Dao_Result<DateOnly?>> FindNearestActiveDayAsync(DateOnly from, int direction);
}
