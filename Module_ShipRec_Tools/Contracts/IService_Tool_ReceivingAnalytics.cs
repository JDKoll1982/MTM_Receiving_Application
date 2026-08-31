using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Service contract for the Receiving Analytics tool: reads the History (past
/// received) and Forecast (incoming) datasets from Infor Visual (read-only) and
/// computes summary stats.
/// </summary>
public interface IService_Tool_ReceivingAnalytics
{
    /// <summary>
    /// Returns both analytics datasets: History (past received items) and
    /// Forecast (incoming items / open PO lines).
    /// </summary>
    Task<Model_Dao_Result<Model_Tool_ReceivingAnalytics>> GetAnalyticsAsync(
        Model_InforVisualReceivingAnalyticsFilter filter
    );

    /// <summary>
    /// Computes summary statistics (total, average/day, peak day) from the
    /// supplied current-period points, plus the trend vs the previous equivalent
    /// period when <paramref name="previousPeriodPoints"/> is provided.
    /// </summary>
    Task<Model_Tool_ReceivingAnalyticsStats> ComputeStatsAsync(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint> currentPoints,
        IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint>? previousPeriodPoints = null
    );
}
