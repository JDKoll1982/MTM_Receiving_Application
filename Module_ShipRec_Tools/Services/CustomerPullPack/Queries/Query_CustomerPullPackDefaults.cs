using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Loads one user's saved Customer Pull n' Pack defaults.
/// </summary>
/// <param name="UserId"></param>
public sealed record Query_CustomerPullPackDefaults(string UserId)
    : IRequest<Model_Dao_Result<Model_CustomerPullPack_UserDefaults>>;
