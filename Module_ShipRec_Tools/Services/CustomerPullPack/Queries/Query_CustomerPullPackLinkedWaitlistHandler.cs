using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Loads one linked Customer Pull n' Pack waitlist item for report-side reopen and update flows.
/// </summary>
public class Query_CustomerPullPackLinkedWaitlistHandler
    : IRequestHandler<
        Query_CustomerPullPackLinkedWaitlist,
        Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
    >
{
    private readonly IService_CustomerPullPackWaitlistSource _waitlistSource;

    public Query_CustomerPullPackLinkedWaitlistHandler(
        IService_CustomerPullPackWaitlistSource waitlistSource
    )
    {
        _waitlistSource = waitlistSource;
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> Handle(
        Query_CustomerPullPackLinkedWaitlist request,
        CancellationToken cancellationToken
    )
    {
        return _waitlistSource.GetByIdAsync(request.WaitlistId);
    }
}
