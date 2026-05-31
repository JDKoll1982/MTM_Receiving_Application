using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
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
    private readonly IService_CustomerPullPackDemandSource _demandSource;
    private readonly IService_CustomerPullPackWaitlistSource _waitlistSource;
    private readonly IService_LoggingUtility _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Query_CustomerPullPackReportHandler"/> class.
    /// </summary>
    /// <param name="logger"></param>
    public Query_CustomerPullPackReportHandler(
        IService_CustomerPullPackDemandSource demandSource,
        IService_CustomerPullPackWaitlistSource waitlistSource,
        IService_LoggingUtility logger
    )
    {
        _demandSource = demandSource;
        _waitlistSource = waitlistSource;
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

        var demandResult = await _demandSource.GetDemandAsync(request.Filter);
        if (!demandResult.IsSuccess || demandResult.Data is null || demandResult.Data.Count == 0)
        {
            return demandResult;
        }

        ApplyFulfillmentAllocation(demandResult.Data);
        await ApplyCompletedLineRecheckIndicatorsAsync(demandResult.Data);
        return demandResult;
    }

    private static void ApplyFulfillmentAllocation(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> demandLines
    )
    {
        foreach (
            var group in demandLines.GroupBy(
                static line => line.ParentPartId,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            var orderedLines = group
                .OrderBy(static line => line.OldestAdded ?? line.PullDate)
                .ThenBy(static line => line.CustomerOrderId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static line => line.SourceLineKey, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var remainingInventory = Math.Max(group.Max(static line => line.FgOnHandQuantity), 0m);

            foreach (var line in orderedLines)
            {
                line.QtySatisfied = 0m;
                line.FulfillmentStatusDisplay = string.Empty;

                var requestedQuantity =
                    line.ShipQuantity > 0 ? line.ShipQuantity : line.QuantityToPack;
                if (requestedQuantity <= 0 || remainingInventory <= 0)
                {
                    continue;
                }

                if (remainingInventory >= requestedQuantity)
                {
                    line.QtySatisfied = requestedQuantity;
                    line.FulfillmentStatusDisplay = "Complete";
                    remainingInventory -= requestedQuantity;
                    continue;
                }

                line.QtySatisfied = remainingInventory;
                line.FulfillmentStatusDisplay = "Partially Filled";
                remainingInventory = 0m;
            }
        }
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
            var waitlistResult = await _waitlistSource.GetByIdAsync(line.LinkedWaitlistId);
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
