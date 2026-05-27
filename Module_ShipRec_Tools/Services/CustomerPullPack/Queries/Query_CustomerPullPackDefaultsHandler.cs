using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Loads persisted Customer Pull n' Pack defaults for the active user.
/// </summary>
public sealed class Query_CustomerPullPackDefaultsHandler
    : IRequestHandler<
        Query_CustomerPullPackDefaults,
        Model_Dao_Result<Model_CustomerPullPack_UserDefaults>
    >
{
    private readonly Dao_CustomerPullPackUserDefaults _defaultsDao;

    public Query_CustomerPullPackDefaultsHandler(Dao_CustomerPullPackUserDefaults defaultsDao)
    {
        _defaultsDao = defaultsDao;
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_UserDefaults>> Handle(
        Query_CustomerPullPackDefaults request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _defaultsDao.GetByUserIdAsync(request.UserId);
    }
}
