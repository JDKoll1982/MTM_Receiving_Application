using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Orchestrates read-only Infor Visual data into card models for the material availability board.
/// </summary>
public class Service_Tool_MaterialAvailabilityBoard : IService_Tool_MaterialAvailabilityBoard
{
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_MaterialAvailabilityBoard(
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        ArgumentNullException.ThrowIfNull(inforVisual);
        ArgumentNullException.ThrowIfNull(logger);
        _inforVisual = inforVisual;
        _logger = logger;
    }

    public async Task<
        Model_Dao_Result<List<Model_Tool_MaterialAvailabilityCard>>
    > GetBoardByLocationAsync(string locationId, string warehouseCode, int? incomingWindowDays)
    {
        _logger.LogInfo(
            $"Building material availability board for location '{locationId}' in warehouse '{warehouseCode}'"
        );

        var currentStockResult = await _inforVisual.GetMaterialAvailabilityCurrentStockAsync(
            locationId,
            null,
            warehouseCode
        );
        if (!currentStockResult.IsSuccess || currentStockResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                currentStockResult.ErrorMessage,
                currentStockResult.Exception
            );
        }

        var incomingSupplyResult = await _inforVisual.GetMaterialAvailabilityIncomingSupplyAsync(
            locationId,
            null,
            warehouseCode
        );
        if (!incomingSupplyResult.IsSuccess || incomingSupplyResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                incomingSupplyResult.ErrorMessage,
                incomingSupplyResult.Exception
            );
        }

        var associatedPartRunsResult =
            await _inforVisual.GetMaterialAvailabilityAssociatedPartRunsAsync(
                locationId,
                null,
                warehouseCode
            );
        if (!associatedPartRunsResult.IsSuccess || associatedPartRunsResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                associatedPartRunsResult.ErrorMessage,
                associatedPartRunsResult.Exception
            );
        }

        var cards = BuildCards(
            searchLocationId: locationId,
            requestedPart: null,
            incomingWindowDays: incomingWindowDays,
            currentStockRows: currentStockResult.Data,
            incomingSupplyRows: incomingSupplyResult.Data,
            associatedPartRuns: associatedPartRunsResult.Data
        );

        return Model_Dao_Result_Factory.Success(cards);
    }

    public async Task<
        Model_Dao_Result<List<Model_Tool_MaterialAvailabilityCard>>
    > GetBoardByPartAsync(string partId, string warehouseCode, int? incomingWindowDays)
    {
        _logger.LogInfo(
            $"Building material availability board for part '{partId}' in warehouse '{warehouseCode}'"
        );

        var partLookupResult = await _inforVisual.GetPartByIDAsync(partId);
        if (!partLookupResult.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                partLookupResult.ErrorMessage,
                partLookupResult.Exception
            );
        }

        var currentStockResult = await _inforVisual.GetMaterialAvailabilityCurrentStockAsync(
            null,
            partId,
            warehouseCode
        );
        if (!currentStockResult.IsSuccess || currentStockResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                currentStockResult.ErrorMessage,
                currentStockResult.Exception
            );
        }

        var incomingSupplyResult = await _inforVisual.GetMaterialAvailabilityIncomingSupplyAsync(
            null,
            partId,
            warehouseCode
        );
        if (!incomingSupplyResult.IsSuccess || incomingSupplyResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                incomingSupplyResult.ErrorMessage,
                incomingSupplyResult.Exception
            );
        }

        var associatedPartRunsResult =
            await _inforVisual.GetMaterialAvailabilityAssociatedPartRunsAsync(
                null,
                partId,
                warehouseCode
            );
        if (!associatedPartRunsResult.IsSuccess || associatedPartRunsResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                associatedPartRunsResult.ErrorMessage,
                associatedPartRunsResult.Exception
            );
        }

        var cards = BuildCards(
            searchLocationId: null,
            requestedPart: partLookupResult.Data,
            incomingWindowDays: incomingWindowDays,
            currentStockRows: currentStockResult.Data,
            incomingSupplyRows: incomingSupplyResult.Data,
            associatedPartRuns: associatedPartRunsResult.Data
        );

        return Model_Dao_Result_Factory.Success(cards);
    }

    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(
        string term
    )
    {
        return await _inforVisual.FuzzySearchPartsAsync(term);
    }

    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchLocationsAsync(
        string term,
        string warehouseCode
    )
    {
        return await _inforVisual.FuzzySearchLocationsAsync(term, warehouseCode);
    }

    public async Task<Model_Dao_Result<bool>> PartExistsAsync(string partId)
    {
        return await _inforVisual.PartExistsAsync(partId);
    }

    public async Task<Model_Dao_Result<bool>> LocationExistsAsync(
        string locationId,
        string warehouseCode
    )
    {
        return await _inforVisual.LocationExistsAsync(locationId, warehouseCode);
    }

    private static List<Model_Tool_MaterialAvailabilityCard> BuildCards(
        string? searchLocationId,
        Model_InforVisualPart? requestedPart,
        int? incomingWindowDays,
        IReadOnlyList<Model_InforVisualMaterialLocationRow> currentStockRows,
        IReadOnlyList<Model_InforVisualIncomingSupplyRow> incomingSupplyRows,
        IReadOnlyList<Model_InforVisualAssociatedPartRunRow> associatedPartRuns
    )
    {
        var partIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in currentStockRows)
        {
            if (string.IsNullOrWhiteSpace(row.PartId) is false)
            {
                partIds.Add(row.PartId);
            }
        }

        foreach (var row in incomingSupplyRows)
        {
            if (string.IsNullOrWhiteSpace(row.PartId) is false)
            {
                partIds.Add(row.PartId);
            }
        }

        foreach (var row in associatedPartRuns)
        {
            if (string.IsNullOrWhiteSpace(row.InputPartNumber) is false)
            {
                partIds.Add(row.InputPartNumber);
            }
        }

        if (requestedPart is not null && string.IsNullOrWhiteSpace(requestedPart.PartID) is false)
        {
            partIds.Add(requestedPart.PartID);
        }

        var cards = new List<Model_Tool_MaterialAvailabilityCard>();

        foreach (var partId in partIds)
        {
            var partCurrentRows = currentStockRows
                .Where(row => string.Equals(row.PartId, partId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var partIncomingRows = incomingSupplyRows
                .Where(row => string.Equals(row.PartId, partId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var partAssociatedRuns = associatedPartRuns
                .Where(row =>
                    string.Equals(row.InputPartNumber, partId, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            var currentLocations = partCurrentRows
                .OrderByDescending(row =>
                    string.IsNullOrWhiteSpace(searchLocationId) is false
                    && string.Equals(
                        row.LocationId,
                        searchLocationId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ThenBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
                .Select(row => new Model_Tool_MaterialAvailabilityLocation
                {
                    LocationId = row.LocationId,
                    Quantity = row.Quantity,
                    IsSearchLocation =
                        string.IsNullOrWhiteSpace(searchLocationId) is false
                        && string.Equals(
                            row.LocationId,
                            searchLocationId,
                            StringComparison.OrdinalIgnoreCase
                        ),
                })
                .ToList();

            var incomingPresentation = BuildIncomingPresentation(
                partIncomingRows,
                incomingWindowDays
            );
            var associatedPartPresentation = BuildAssociatedPartPresentation(
                partAssociatedRuns,
                incomingWindowDays
            );
            var searchLocationQuantity = currentLocations
                .Where(location => location.IsSearchLocation)
                .Sum(location => location.Quantity);
            var totalPositiveQuantity = currentLocations.Sum(location => location.Quantity);
            var partDescription =
                partCurrentRows
                    .Select(row => row.PartDescription)
                    .Concat(partIncomingRows.Select(row => row.PartDescription))
                    .Concat(partAssociatedRuns.Select(row => row.InputPartDescription))
                    .Concat(
                        requestedPart is not null
                        && string.Equals(
                            requestedPart.PartID,
                            partId,
                            StringComparison.OrdinalIgnoreCase
                        )
                            ? new[] { requestedPart.Description }
                            : Array.Empty<string>()
                    )
                    .FirstOrDefault(description => string.IsNullOrWhiteSpace(description) is false)
                ?? string.Empty;

            var headerSummary =
                associatedPartPresentation.AssociatedPartRuns.Count > 0
                    ? associatedPartPresentation.SummaryText
                    : incomingPresentation.NextDateSummary;
            var headerSortBucket =
                associatedPartPresentation.AssociatedPartRuns.Count > 0
                    ? associatedPartPresentation.SortBucket
                    : incomingPresentation.SortBucket;
            var headerSortDate =
                associatedPartPresentation.AssociatedPartRuns.Count > 0
                    ? associatedPartPresentation.SortDate
                    : incomingPresentation.SortDate;

            cards.Add(
                new Model_Tool_MaterialAvailabilityCard
                {
                    PartId = partId,
                    PartDescription = partDescription,
                    SearchLocationId = searchLocationId ?? string.Empty,
                    QuantityInSearchLocation = searchLocationQuantity,
                    TotalPositiveQuantity = totalPositiveQuantity,
                    CurrentLocations = currentLocations,
                    IncomingRollup = incomingPresentation.Rollup,
                    UpcomingDates = incomingPresentation.UpcomingDates,
                    AssociatedPartRuns = associatedPartPresentation.AssociatedPartRuns,
                    NextDateSummary = headerSummary,
                    EarliestRelevantDate =
                        headerSortDate == DateTime.MaxValue ? null : headerSortDate,
                    SortBucket = headerSortBucket,
                    SortDate = headerSortDate,
                    IsExpanded = false,
                }
            );
        }

        return cards
            .OrderBy(card => card.SortBucket)
            .ThenBy(card => card.SortDate)
            .ThenBy(card => card.PartId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IncomingPresentation BuildIncomingPresentation(
        IReadOnlyList<Model_InforVisualIncomingSupplyRow> partIncomingRows,
        int? incomingWindowDays
    )
    {
        var today = DateTime.Today;
        var horizon = today.AddDays(incomingWindowDays ?? 30);

        var qualifyingLines = new List<IncomingLinePresentation>();
        var qualifyingRollupRows = new List<Model_InforVisualIncomingSupplyRow>();

        foreach (var row in partIncomingRows)
        {
            if (row.IsBlanketOrder)
            {
                qualifyingRollupRows.Add(row);

                if (row.LineLastReceivedDate is DateTime lastReceivedDate)
                {
                    qualifyingLines.Add(
                        new IncomingLinePresentation(
                            row,
                            lastReceivedDate.Date,
                            "Last received on",
                            true
                        )
                    );
                }

                continue;
            }

            var selectedDate = SelectBestIncomingDate(row);
            if (selectedDate is null)
            {
                qualifyingRollupRows.Add(row);
                continue;
            }

            if (
                incomingWindowDays.HasValue
                && (selectedDate.Date < today || selectedDate.Date > horizon)
            )
            {
                continue;
            }

            qualifyingRollupRows.Add(row);
            qualifyingLines.Add(selectedDate);
        }

        if (qualifyingRollupRows.Count == 0)
        {
            return IncomingPresentation.Empty;
        }

        var distinctDates = qualifyingLines
            .OrderBy(line => line.IsBlanket)
            .ThenBy(line => line.Date)
            .ThenBy(line => line.Label, StringComparer.OrdinalIgnoreCase)
            .GroupBy(line => new { line.Label, line.Date })
            .Select(group => new Model_Tool_MaterialAvailabilityIncomingDate
            {
                Label = group.Key.Label,
                Date = group.Key.Date,
            })
            .ToList();

        var distinctPoLines = qualifyingRollupRows
            .GroupBy(row => new
            {
                PONumber = (row.PONumber ?? string.Empty).Trim().ToUpperInvariant(),
                POLineNumber = row.POLineNumber,
            })
            .Select(group => group.First())
            .ToList();

        var rollup = new Model_Tool_MaterialAvailabilityRollup
        {
            OrderedQty = distinctPoLines.Sum(line => line.OrderedQty),
            ReceivedQty = distinctPoLines.Sum(line => line.ReceivedQty),
            RemainingQty = distinctPoLines.Sum(line => line.RemainingQty),
            PurchaseOrderCount = distinctPoLines
                .Select(line => line.PONumber)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            POLineCount = distinctPoLines.Count,
        };

        var earliestFutureDate = qualifyingLines
            .Where(line => line.IsBlanket is false)
            .Min(line => (DateTime?)line.Date);
        var earliestBlanketDate = qualifyingLines
            .Where(line => line.IsBlanket)
            .Min(line => (DateTime?)line.Date);

        var sortBucket = earliestFutureDate.HasValue ? 0 : 1;
        var sortDate = earliestFutureDate ?? earliestBlanketDate ?? DateTime.MaxValue;
        var nextDateSummary =
            distinctDates.Count > 0 ? $"Next material date: {distinctDates[0].DisplayText}"
            : distinctPoLines.Count > 0
                ? "Incoming material found, but no qualifying due dates are available."
            : incomingWindowDays.HasValue
                ? $"No inbound material due in the next {incomingWindowDays.Value} days."
            : "No inbound material found.";

        return new IncomingPresentation(
            distinctDates,
            rollup,
            nextDateSummary,
            sortBucket,
            sortDate
        );
    }

    private static AssociatedPartPresentation BuildAssociatedPartPresentation(
        IReadOnlyList<Model_InforVisualAssociatedPartRunRow> partAssociatedRuns,
        int? incomingWindowDays
    )
    {
        if (partAssociatedRuns.Count == 0)
        {
            return AssociatedPartPresentation.Empty(incomingWindowDays);
        }

        var today = DateTime.Today;
        var horizon = incomingWindowDays.HasValue
            ? today.AddDays(incomingWindowDays.Value)
            : DateTime.MaxValue;

        var filteredRows = partAssociatedRuns
            .Where(row => row.NextDueToRunDate.HasValue)
            .Where(row =>
                incomingWindowDays.HasValue is false
                || (row.IsFutureOrTodayRun && row.NextDueToRunDate!.Value.Date <= horizon)
            )
            .ToList();

        if (filteredRows.Count == 0)
        {
            return AssociatedPartPresentation.Empty(incomingWindowDays);
        }

        var associatedRuns = filteredRows
            .GroupBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                group
                    .OrderBy(row => row.IsFutureOrTodayRun ? 0 : 1)
                    .ThenBy(row =>
                        row.IsFutureOrTodayRun
                            ? row.NextDueToRunDate!.Value.Date
                            : DateTime.MaxValue
                    )
                    .ThenByDescending(row =>
                        row.IsFutureOrTodayRun
                            ? DateTime.MinValue
                            : row.NextDueToRunDate!.Value.Date
                    )
                    .ThenBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(row => row.WorkOrderBaseId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(row => row.OperationSeqNo)
                    .ThenBy(row => row.RequirementPieceNo)
                    .First()
            )
            .OrderBy(row => row.IsFutureOrTodayRun ? 0 : 1)
            .ThenBy(row =>
                row.IsFutureOrTodayRun ? row.NextDueToRunDate!.Value.Date : DateTime.MaxValue
            )
            .ThenByDescending(row =>
                row.IsFutureOrTodayRun ? DateTime.MinValue : row.NextDueToRunDate!.Value.Date
            )
            .ThenBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
            .Select(row => new Model_Tool_MaterialAvailabilityAssociatedPartRun
            {
                AssociatedPartNumber = row.AssociatedPartNumber,
                AssociatedPartDescription = row.AssociatedPartDescription,
                NextDueToRunDate = row.NextDueToRunDate!.Value.Date,
                IsFutureOrTodayRun = row.IsFutureOrTodayRun,
                NextDueDateSource = row.NextDueDateSource,
                WorkOrderDisplay =
                    $"WO {row.WorkOrderType.Trim()} {row.WorkOrderBaseId}-{row.WorkOrderLotId}-{row.WorkOrderSplitId}-{row.WorkOrderSubId}",
            })
            .ToList();

        var firstRun = associatedRuns[0];
        var summaryLabel = firstRun.IsFutureOrTodayRun ? "Next run" : "Latest known run";
        var summaryText =
            $"{summaryLabel}: {firstRun.AssociatedPartNumber} {firstRun.NextRunDateDisplay}";

        return new AssociatedPartPresentation(
            associatedRuns,
            summaryText,
            firstRun.IsFutureOrTodayRun ? 0 : 1,
            firstRun.NextDueToRunDate
        );
    }

    private static IncomingLinePresentation? SelectBestIncomingDate(
        Model_InforVisualIncomingSupplyRow row
    )
    {
        var candidates = new List<(DateTime Date, int Priority, string Label)>(capacity: 4);

        if (row.LinePromiseDate is DateTime linePromiseDate)
        {
            candidates.Add((linePromiseDate.Date, 0, "Line promise"));
        }

        if (row.LineDesiredReceiveDate is DateTime lineDesiredDate)
        {
            candidates.Add((lineDesiredDate.Date, 1, "Line desired"));
        }

        if (row.HeaderPromiseDate is DateTime headerPromiseDate)
        {
            candidates.Add((headerPromiseDate.Date, 2, "PO promise"));
        }

        if (row.HeaderDesiredReceiveDate is DateTime headerDesiredDate)
        {
            candidates.Add((headerDesiredDate.Date, 3, "PO desired"));
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        var selectedCandidate = candidates
            .OrderBy(candidate => candidate.Date)
            .ThenBy(candidate => candidate.Priority)
            .First();

        return new IncomingLinePresentation(
            row,
            selectedCandidate.Date,
            selectedCandidate.Label,
            false
        );
    }

    private sealed record IncomingLinePresentation(
        Model_InforVisualIncomingSupplyRow Row,
        DateTime Date,
        string Label,
        bool IsBlanket
    );

    private sealed record IncomingPresentation(
        List<Model_Tool_MaterialAvailabilityIncomingDate> UpcomingDates,
        Model_Tool_MaterialAvailabilityRollup Rollup,
        string NextDateSummary,
        int SortBucket,
        DateTime SortDate
    )
    {
        public static IncomingPresentation Empty { get; } =
            new(
                UpcomingDates: [],
                Rollup: new Model_Tool_MaterialAvailabilityRollup(),
                NextDateSummary: "No inbound material found.",
                SortBucket: 2,
                SortDate: DateTime.MaxValue
            );
    }

    private sealed record AssociatedPartPresentation(
        List<Model_Tool_MaterialAvailabilityAssociatedPartRun> AssociatedPartRuns,
        string SummaryText,
        int SortBucket,
        DateTime SortDate
    )
    {
        public static AssociatedPartPresentation Empty(int? incomingWindowDays)
        {
            var summaryText = incomingWindowDays.HasValue
                ? $"No associated part runs were found in the next {incomingWindowDays.Value} days."
                : "No associated part runs were found.";

            return new AssociatedPartPresentation([], summaryText, 2, DateTime.MaxValue);
        }
    }
}
