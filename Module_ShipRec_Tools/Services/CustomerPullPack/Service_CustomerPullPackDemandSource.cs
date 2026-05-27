using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Service_CustomerPullPackDemandSource : IService_CustomerPullPackDemandSource
{
    private readonly IService_CustomerPullPackDataSourceResolver _resolver;
    private readonly Dao_CustomerPullPackDemand _liveDemandDao;
    private readonly Service_CustomerPullPackMockDemandSource _mockDemandSource;

    public Service_CustomerPullPackDemandSource(
        IService_CustomerPullPackDataSourceResolver resolver,
        Dao_CustomerPullPackDemand liveDemandDao,
        Service_CustomerPullPackMockDemandSource mockDemandSource
    )
    {
        _resolver = resolver;
        _liveDemandDao = liveDemandDao;
        _mockDemandSource = mockDemandSource;
    }

    public Task<Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>> GetDemandAsync(
        Model_CustomerPullPack_DemandFilter filter
    )
    {
        _resolver.ResolveForWorkflow();
        return _resolver.IsMockMode
            ? _mockDemandSource.GetDemandAsync(filter)
            : _liveDemandDao.GetDemandAsync(filter);
    }
}
