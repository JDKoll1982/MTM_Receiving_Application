using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Composes one day. This is the only path a view model calls to produce findings,
/// and it writes nothing (FR-038).
/// </summary>
public interface IService_ReconciliationPreview
{
    /// <summary>
    /// Loads, groups, selects movements, builds buckets, allocates and proves the day.
    /// </summary>
    /// <param name="date">The calendar day to preview.</param>
    Task<Model_Dao_Result<Model_ReconciliationDay>> PreviewDayAsync(DateOnly date);
}
