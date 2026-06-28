using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Saves Customer Pull n' Pack defaults for the active user.
/// </summary>
/// <param name="Defaults"></param>
/// <param name="UpdatedByUserId"></param>
public sealed record Command_CustomerPullPackSaveDefaults(
    Model_CustomerPullPack_UserDefaults Defaults,
    string UpdatedByUserId
) : IRequest<Model_Dao_Result<Model_CustomerPullPack_UserDefaults>>;
