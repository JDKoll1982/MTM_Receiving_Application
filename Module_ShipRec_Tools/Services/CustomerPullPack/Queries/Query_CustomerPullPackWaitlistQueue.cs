using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Retrieves Customer Pull n' Pack waitlist queue rows with optional filters.
/// </summary>
/// <param name="WaitlistId"></param>
/// <param name="CustomerId"></param>
/// <param name="RequesterUserId"></param>
/// <param name="CurrentOwnerUserId"></param>
/// <param name="LocationId"></param>
/// <param name="StatusSet"></param>
/// <param name="UseDefaultOpenWork"></param>
/// <param name="MaxResults"></param>
public sealed record Query_CustomerPullPackWaitlistQueue(
    string? WaitlistId = null,
    string? CustomerId = null,
    string? RequesterUserId = null,
    string? CurrentOwnerUserId = null,
    string? LocationId = null,
    IReadOnlyCollection<Enum_CustomerPullPackWaitlistStatus>? StatusSet = null,
    bool UseDefaultOpenWork = true,
    int MaxResults = 250
) : IRequest<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>>;
