using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Handles report-demand retrieval for the Customer Pull n' Pack feature.
/// </summary>
public class Query_CustomerPullPackReportHandler
    : IRequestHandler<
        Query_CustomerPullPackReport,
        Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>
    >
{
    private readonly Dao_CustomerPullPackDemand _demandDao;
    private readonly Dao_CustomerPullPackWaitlist _waitlistDao;
    private readonly IService_LoggingUtility _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Query_CustomerPullPackReportHandler"/> class.
    /// </summary>
    /// <param name="demandDao"></param>
    /// <param name="logger"></param>
    public Query_CustomerPullPackReportHandler(
        Dao_CustomerPullPackDemand demandDao,
        Dao_CustomerPullPackWaitlist waitlistDao,
        IService_LoggingUtility logger
    )
    {
        _demandDao = demandDao;
        _waitlistDao = waitlistDao;
        _logger = logger;
    }

    /// <summary>
    /// Implements Workflow 1.1 by loading the filtered demand report for one selected customer.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async Task<Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>> Handle(
        Query_CustomerPullPackReport request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Filter.CustomerId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                "Customer ID is required."
            );
        }

        _logger.LogInfo(
            $"Loading Customer Pull n' Pack report for customer '{request.Filter.CustomerId}'."
        );

        var demandResult = await _demandDao.GetDemandAsync(request.Filter);
        if (!demandResult.IsSuccess || demandResult.Data is null || demandResult.Data.Count == 0)
        {
            return demandResult;
        }

        await ApplyCompletedLineRecheckIndicatorsAsync(demandResult.Data);
        return demandResult;
    }

    private async Task ApplyCompletedLineRecheckIndicatorsAsync(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> demandLines
    )
    {
        foreach (
            var line in demandLines.Where(line =>
                line.HasLinkedWaitlist && string.IsNullOrWhiteSpace(line.LinkedWaitlistId) is false
            )
        )
        {
            var waitlistResult = await _waitlistDao.GetByIdAsync(line.LinkedWaitlistId);
            if (!waitlistResult.IsSuccess || waitlistResult.Data is null)
            {
                continue;
            }

            if (waitlistResult.Data.CurrentStatus != Enum_CustomerPullPackWaitlistStatus.Completed)
            {
                continue;
            }

            line.RecheckIndicator =
                line.RecheckIndicator || ShouldShowRecheckIndicator(line, waitlistResult.Data);
        }
    }

    private static bool ShouldShowRecheckIndicator(
        Model_CustomerPullPack_DemandLine line,
        Model_CustomerPullPack_WaitlistEntry waitlistEntry
    )
    {
        if (waitlistEntry.RequestedQuantity != line.QuantityToPack)
        {
            return true;
        }

        var currentLocationIds = line
            .LocationOptions.Select(static option => option.LocationId)
            .Where(static locationId => string.IsNullOrWhiteSpace(locationId) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var savedLocationIds = waitlistEntry
            .SelectedLocations.Where(static locationId =>
                string.IsNullOrWhiteSpace(locationId) is false
            )
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return savedLocationIds.Except(currentLocationIds, StringComparer.OrdinalIgnoreCase).Any()
            || currentLocationIds.Except(savedLocationIds, StringComparer.OrdinalIgnoreCase).Any();
    }
}
