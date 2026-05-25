using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Retrieves one linked waitlist item by its stable waitlist ID.
/// </summary>
/// <param name="WaitlistId"></param>
public sealed record Query_CustomerPullPackLinkedWaitlist(string WaitlistId)
    : IRequest<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>>;
