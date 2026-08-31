using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Service contract for the Delivery Schedule tool: reads the receiving-schedule
/// grid (read-only Infor Visual) and maps rows to presentation models.
/// </summary>
public interface IService_Tool_DeliverySchedule
{
    /// <summary>
    /// Returns receiving-schedule grid rows matching the supplied filter.
    /// </summary>
    Task<Model_Dao_Result<List<Model_Tool_DeliveryScheduleLine>>> SearchAsync(
        Model_InforVisualDeliveryScheduleFilter filter
    );
}
