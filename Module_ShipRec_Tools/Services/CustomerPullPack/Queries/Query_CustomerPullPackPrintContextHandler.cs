using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Creates print-ready Customer Pull n' Pack view models for report and queue workflows.
/// </summary>
public sealed class Query_CustomerPullPackPrintContextHandler
    : IRequestHandler<
        Query_CustomerPullPackPrintContext,
        Model_Dao_Result<Model_CustomerPullPack_PrintContext>
    >
{
    public Task<Model_Dao_Result<Model_CustomerPullPack_PrintContext>> Handle(
        Query_CustomerPullPackPrintContext request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var demandLines = FilterDemandLines(request);
        var waitlistEntries = FilterWaitlistEntries(request);
        var printContext = new Model_CustomerPullPack_PrintContext
        {
            PrintMode = request.PrintMode,
            Title = ResolveTitle(request),
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            ActiveFiltersSummary = request.ActiveFiltersSummary,
            GeneratedAt = DateTime.UtcNow,
            DemandLines = demandLines,
            WaitlistEntries = waitlistEntries,
            GroupedSubPartTotals = BuildGroupedSubPartTotals(demandLines, waitlistEntries),
        };

        return Task.FromResult(Model_Dao_Result_Factory.Success(printContext));
    }

    private static List<Model_CustomerPullPack_DemandLine> FilterDemandLines(
        Query_CustomerPullPackPrintContext request
    )
    {
        return request.PrintMode switch
        {
            Enum_CustomerPullPackPrintMode.ShortageOnly => request
                .DemandLines.Where(static line => line.ShortageFlag)
                .ToList(),
            Enum_CustomerPullPackPrintMode.WaitlistOnly => request
                .DemandLines.Where(static line => line.HasLinkedWaitlist)
                .ToList(),
            Enum_CustomerPullPackPrintMode.SelectedContext => request
                .DemandLines.Where(static line => line.IsSelected)
                .ToList(),
            _ => request.DemandLines.ToList(),
        };
    }

    private static List<Model_CustomerPullPack_WaitlistEntry> FilterWaitlistEntries(
        Query_CustomerPullPackPrintContext request
    )
    {
        return request.PrintMode switch
        {
            Enum_CustomerPullPackPrintMode.PullList => request
                .WaitlistEntries.Where(static entry => entry.SelectedLocations.Count > 0)
                .ToList(),
            Enum_CustomerPullPackPrintMode.SelectedContext => request.WaitlistEntries.ToList(),
            Enum_CustomerPullPackPrintMode.WaitlistOnly => request.WaitlistEntries.ToList(),
            _ => request.WaitlistEntries.ToList(),
        };
    }

    private static List<Model_CustomerPullPack_SubPartPrintGroup> BuildGroupedSubPartTotals(
        IReadOnlyList<Model_CustomerPullPack_DemandLine> demandLines,
        IReadOnlyList<Model_CustomerPullPack_WaitlistEntry> waitlistEntries
    )
    {
        if (waitlistEntries.Count > 0)
        {
            return waitlistEntries
                .GroupBy(static entry => entry.ParentPartId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new Model_CustomerPullPack_SubPartPrintGroup
                {
                    SubPartId = group.Key,
                    TotalQuantityNeeded = group.Sum(static entry => entry.RequestedQuantity),
                    TotalQuantityOnHand = 0,
                    ChosenLocations = group
                        .SelectMany(static entry => entry.SelectedLocations)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(static location => location, StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                })
                .ToList();
        }

        return demandLines
            .GroupBy(static line => line.ParentPartId, StringComparer.OrdinalIgnoreCase)
            .Select(group => new Model_CustomerPullPack_SubPartPrintGroup
            {
                SubPartId = group.Key,
                TotalQuantityNeeded = group.Sum(static line => line.QuantityToPack),
                TotalQuantityOnHand = group
                    .SelectMany(static line => line.LocationOptions)
                    .Where(static option => option.Selected)
                    .Sum(static option => option.OnHandQuantity),
                ChosenLocations = group
                    .SelectMany(static line => line.LocationOptions)
                    .Where(static option => option.Selected)
                    .Select(static option => option.LocationId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(static location => location, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            })
            .ToList();
    }

    private static string ResolveTitle(Query_CustomerPullPackPrintContext request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) is false)
        {
            return request.Title.Trim();
        }

        return request.PrintMode switch
        {
            Enum_CustomerPullPackPrintMode.FloorCopy => "Customer Pull n' Pack - Floor Copy",
            Enum_CustomerPullPackPrintMode.PullList => "Customer Pull n' Pack - Pull List",
            Enum_CustomerPullPackPrintMode.ShortageOnly => "Customer Pull n' Pack - Shortage Only",
            Enum_CustomerPullPackPrintMode.WaitlistOnly => "Customer Pull n' Pack - Waitlist Only",
            Enum_CustomerPullPackPrintMode.SelectedContext =>
                "Customer Pull n' Pack - Selected Context",
            _ => "Customer Pull n' Pack - Current View",
        };
    }
}
