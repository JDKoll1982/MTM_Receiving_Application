using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Service contract for the Receiving Analytics App Data tab: reads the app's
/// own records from MySQL for the three app panels and reads Infor Visual
/// (read-only) for the comparison panel. Both methods are async and take the
/// same filter, which carries the range bounds only.
/// </summary>
public interface IService_Tool_ReceivingAnalyticsApp
{
    /// <summary>
    /// Returns the app dataset for the given range. The four DAO reads run
    /// concurrently and the whole dataset fails if any one of them fails.
    /// </summary>
    Task<Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppDataset>> GetAppDataAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    );

    /// <summary>
    /// Returns the comparison dataset for the given range. The ERP side is read
    /// through IService_InforVisual.GetReceivingAnalyticsHistoryAsync with the
    /// identical date bounds and all five category scopes enabled. The ERP half
    /// can fail on its own and reports that failure on the comparison panel
    /// alone.
    /// </summary>
    Task<Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppComparison>> GetComparisonAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    );

    /// <summary>
    /// Returns the busiest receiving days per item type (Coil, Sheet, Standard) for the range,
    /// ranked by distinct loads. Reading MySQL is the only data source for this panel.
    /// </summary>
    Task<Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppTopDayRow>>> GetTopDaysByItemTypeAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter,
        int topCount
    );

    /// <summary>
    /// Returns one row per received day in the range, carrying the day's own load and label
    /// totals across every item type. Ranked and cut to the top count by the view model.
    /// </summary>
    Task<Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppBusiestDayRow>>> GetBusiestDaysAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    );
}
