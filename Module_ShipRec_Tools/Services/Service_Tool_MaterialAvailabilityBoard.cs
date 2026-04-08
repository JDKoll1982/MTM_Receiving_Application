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
    private const int IncomingWindowDays = 30;

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
    > GetBoardByLocationAsync(string locationId, string warehouseCode)
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

        var cards = BuildCards(
            searchLocationId: locationId,
            requestedPart: null,
            currentStockResult.Data,
            incomingSupplyResult.Data
        );

        return Model_Dao_Result_Factory.Success(cards);
    }

    public async Task<
        Model_Dao_Result<List<Model_Tool_MaterialAvailabilityCard>>
    > GetBoardByPartAsync(string partId, string warehouseCode)
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

        var cards = BuildCards(
            searchLocationId: null,
            requestedPart: partLookupResult.Data,
            currentStockResult.Data,
            incomingSupplyResult.Data
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
        IReadOnlyList<Model_InforVisualMaterialLocationRow> currentStockRows,
        IReadOnlyList<Model_InforVisualIncomingSupplyRow> incomingSupplyRows
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

            var incomingPresentation = BuildIncomingPresentation(partIncomingRows);
            var searchLocationQuantity = currentLocations
                .Where(location => location.IsSearchLocation)
                .Sum(location => location.Quantity);
            var totalPositiveQuantity = currentLocations.Sum(location => location.Quantity);
            var partDescription =
                partCurrentRows
                    .Select(row => row.PartDescription)
                    .Concat(partIncomingRows.Select(row => row.PartDescription))
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
                    NextDateSummary = incomingPresentation.NextDateSummary,
                    EarliestRelevantDate =
                        incomingPresentation.SortDate == DateTime.MaxValue
                            ? null
                            : incomingPresentation.SortDate,
                    SortBucket = incomingPresentation.SortBucket,
                    SortDate = incomingPresentation.SortDate,
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
        IReadOnlyList<Model_InforVisualIncomingSupplyRow> partIncomingRows
    )
    {
        var today = DateTime.Today;
        var horizon = today.AddDays(IncomingWindowDays);

        var qualifyingLines = new List<IncomingLinePresentation>();
        foreach (var row in partIncomingRows)
        {
            if (row.IsBlanketOrder)
            {
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
                continue;
            }

            if (selectedDate.Date < today || selectedDate.Date > horizon)
            {
                continue;
            }

            qualifyingLines.Add(selectedDate);
        }

        if (qualifyingLines.Count == 0)
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

        var distinctPoLines = qualifyingLines
            .GroupBy(line => new
            {
                PONumber = (line.Row.PONumber ?? string.Empty).Trim().ToUpperInvariant(),
                POLineNumber = line.Row.POLineNumber,
            })
            .Select(group => group.First().Row)
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

        var sortBucket =
            earliestFutureDate.HasValue ? 0
            : earliestBlanketDate.HasValue ? 1
            : 2;
        var sortDate = earliestFutureDate ?? earliestBlanketDate ?? DateTime.MaxValue;
        var nextDateSummary =
            distinctDates.Count > 0
                ? distinctDates[0].DisplayText
                : "No inbound material due in next 30 days.";

        return new IncomingPresentation(
            distinctDates,
            rollup,
            nextDateSummary,
            sortBucket,
            sortDate
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
                NextDateSummary: "No inbound material due in next 30 days.",
                SortBucket: 2,
                SortDate: DateTime.MaxValue
            );
    }
}
