using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Contract for the Outside Service Provider History lookup tool.
/// Delegates to <see cref="MTM_Receiving_Application.Module_Core.Contracts.Services.IService_InforVisual"/>
/// for read-only Infor Visual queries.
/// </summary>
public interface IService_Tool_OutsideServiceHistory
{
    /// <summary>
    /// Retrieves all outside service dispatch records for the specified part number.
    /// </summary>
    /// <param name="partNumber">Part ID to search for in SERVICE_DISP_LINE.</param>
    Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByPartAsync(string partNumber);
}
