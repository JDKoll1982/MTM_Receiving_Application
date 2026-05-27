using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;

public interface IService_CustomerPullPackDemandSource
{
    Task<Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>> GetDemandAsync(
        Model_CustomerPullPack_DemandFilter filter
    );
}
