using System;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;

namespace MTM_Receiving_Application.Module_Core.Helpers.Reprint;

/// <summary>
/// Shared helpers for the per-module reprint services. Keeps the batch bucketing logic
/// (queued / already-queued / failed) in one place so Receiving, Dunnage, and Volvo behave
/// identically.
/// </summary>
public static class Helper_ReprintBatch
{
    /// <summary>
    /// Buckets a single reprint attempt into <paramref name="result"/>. A DAO result whose error
    /// message indicates the row is already queued (the stored procedures raise SQLSTATE 45000
    /// with "already queued for reprint") is recorded as an already-queued duplicate rather than
    /// a hard failure, so one duplicate does not abort the whole batch.
    /// </summary>
    public static void Categorize(
        Model_ReprintBatchResult result,
        string historyId,
        Model_Dao_Result daoResult
    )
    {
        if (daoResult.IsSuccess)
        {
            result.Queued.Add(historyId);
            return;
        }

        if (
            daoResult.ErrorMessage?.IndexOf(
                "already queued",
                StringComparison.OrdinalIgnoreCase
            ) >= 0
        )
        {
            result.AlreadyQueued.Add(historyId);
            return;
        }

        result.Failed.Add(historyId);
    }
}
