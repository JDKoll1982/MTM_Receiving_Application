using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Commands;

/// <summary>
/// Normalizes and persists Customer Pull n' Pack defaults.
/// </summary>
public sealed class Command_CustomerPullPackSaveDefaultsHandler
    : IRequestHandler<
        Command_CustomerPullPackSaveDefaults,
        Model_Dao_Result<Model_CustomerPullPack_UserDefaults>
    >
{
    private readonly Dao_CustomerPullPackUserDefaults _defaultsDao;

    public Command_CustomerPullPackSaveDefaultsHandler(Dao_CustomerPullPackUserDefaults defaultsDao)
    {
        _defaultsDao = defaultsDao;
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_UserDefaults>> Handle(
        Command_CustomerPullPackSaveDefaults request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request.Defaults);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedDefaults = new Model_CustomerPullPack_UserDefaults
        {
            UserId = request.Defaults.UserId?.Trim() ?? string.Empty,
            DefaultCustomerId =
                request.Defaults.DefaultCustomerId?.Trim().ToUpperInvariant() ?? string.Empty,
            FavoriteCustomerIds = request
                .Defaults.FavoriteCustomerIds.Where(static customerId =>
                    string.IsNullOrWhiteSpace(customerId) is false
                )
                .Select(static customerId => customerId.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            LastGoodDateRangeType = request.Defaults.LastGoodDateRangeType?.Trim() ?? string.Empty,
            LastGoodDateFrom = request.Defaults.LastGoodDateFrom,
            LastGoodDateTo = request.Defaults.LastGoodDateTo,
            DefaultSortMode = request.Defaults.DefaultSortMode,
            DefaultShortagesOnly = request.Defaults.DefaultShortagesOnly,
            DefaultUnpulledOnly = request.Defaults.DefaultUnpulledOnly,
            DefaultLateOrdersOnly = request.Defaults.DefaultLateOrdersOnly,
            DefaultWaitlistStatusSet =
                request.Defaults.DefaultWaitlistStatusSet.Count == 0
                    ?
                    [
                        Enum_CustomerPullPackWaitlistStatus.Requested,
                        Enum_CustomerPullPackWaitlistStatus.Accepted,
                        Enum_CustomerPullPackWaitlistStatus.Problem,
                    ]
                    : request.Defaults.DefaultWaitlistStatusSet.Distinct().ToList(),
            DefaultPrintPreset = request.Defaults.DefaultPrintPreset,
        };

        return _defaultsDao.UpsertAsync(normalizedDefaults, request.UpdatedByUserId);
    }
}
