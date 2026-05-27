using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Service_CustomerPullPackWaitlistSource : IService_CustomerPullPackWaitlistSource
{
    private readonly IService_CustomerPullPackDataSourceResolver _resolver;
    private readonly Dao_CustomerPullPackWaitlist _liveWaitlistDao;
    private readonly Service_CustomerPullPackMockWaitlistSource _mockWaitlistSource;

    public Service_CustomerPullPackWaitlistSource(
        IService_CustomerPullPackDataSourceResolver resolver,
        Dao_CustomerPullPackWaitlist liveWaitlistDao,
        Service_CustomerPullPackMockWaitlistSource mockWaitlistSource
    )
    {
        _resolver = resolver;
        _liveWaitlistDao = liveWaitlistDao;
        _mockWaitlistSource = mockWaitlistSource;
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> UpsertAsync(
        Model_CustomerPullPack_WaitlistEntry entry
    )
    {
        _resolver.ResolveForWorkflow();
        return _resolver.IsMockMode
            ? _mockWaitlistSource.UpsertAsync(entry)
            : _liveWaitlistDao.UpsertAsync(entry);
    }

    public Task<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>> GetQueueAsync(
        string? waitlistId = null,
        string? customerId = null,
        string? requesterUserId = null,
        string? currentOwnerUserId = null,
        string? locationId = null,
        IReadOnlyCollection<Enum_CustomerPullPackWaitlistStatus>? statusSet = null,
        bool useDefaultOpenWork = true,
        int maxResults = 250
    )
    {
        _resolver.ResolveForWorkflow();
        return _resolver.IsMockMode
            ? _mockWaitlistSource.GetQueueAsync(
                waitlistId,
                customerId,
                requesterUserId,
                currentOwnerUserId,
                locationId,
                statusSet,
                useDefaultOpenWork,
                maxResults
            )
            : _liveWaitlistDao.GetQueueAsync(
                waitlistId,
                customerId,
                requesterUserId,
                currentOwnerUserId,
                locationId,
                statusSet,
                useDefaultOpenWork,
                maxResults
            );
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> GetByIdAsync(
        string waitlistId
    )
    {
        _resolver.ResolveForWorkflow();
        return _resolver.IsMockMode
            ? _mockWaitlistSource.GetByIdAsync(waitlistId)
            : _liveWaitlistDao.GetByIdAsync(waitlistId);
    }
}
