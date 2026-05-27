using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Loads the Customer Pull n' Pack waitlist queue using the MTM-managed persistence store.
/// </summary>
public sealed class Query_CustomerPullPackWaitlistQueueHandler
    : IRequestHandler<
        Query_CustomerPullPackWaitlistQueue,
        Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>
    >
{
    private readonly IService_CustomerPullPackWaitlistSource _waitlistSource;

    public Query_CustomerPullPackWaitlistQueueHandler(
        IService_CustomerPullPackWaitlistSource waitlistSource
    )
    {
        _waitlistSource = waitlistSource;
    }

    public Task<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>> Handle(
        Query_CustomerPullPackWaitlistQueue request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _waitlistSource.GetQueueAsync(
            waitlistId: request.WaitlistId,
            customerId: request.CustomerId,
            requesterUserId: request.RequesterUserId,
            currentOwnerUserId: request.CurrentOwnerUserId,
            locationId: request.LocationId,
            statusSet: request.StatusSet,
            useDefaultOpenWork: request.UseDefaultOpenWork,
            maxResults: request.MaxResults
        );
    }
}
