using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Orchestrates read-only Infor Visual data into card models for the material availability board.
/// </summary>
public class Service_Tool_MaterialAvailabilityBoard : IService_Tool_MaterialAvailabilityBoard
{
    private const string CardBackground = "#FFFFFF";
    private const string CardBorder = "#D0DAE5";
    private const string AccentBackground = "#E9D5FF";
    private const string AccentForeground = "#1F1633";

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

    public Task<Model_Dao_Result<Model_FormattedReportDocument>> FormatBoardForPrintAsync(
        IReadOnlyList<Model_Tool_MaterialAvailabilityCard> cards,
        string searchLabel,
        string searchTerm,
        string warehouseCode,
        string lookAheadOption
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(cards);

            var safeSearchLabel = string.IsNullOrWhiteSpace(searchLabel)
                ? "Search"
                : searchLabel.Trim();
            var safeSearchTerm = string.IsNullOrWhiteSpace(searchTerm)
                ? "Current results"
                : searchTerm.Trim();
            var safeLookAheadOption = string.IsNullOrWhiteSpace(lookAheadOption)
                ? "30"
                : lookAheadOption.Trim();

            _logger.LogInfo(
                $"Formatting {cards.Count} material availability card(s) for print output"
            );

            if (cards.Count == 0)
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Success(
                        new Model_FormattedReportDocument
                        {
                            HtmlFragment = "<p>No material cards to print.</p>",
                            PlainText = "No material cards to print.",
                        }
                    )
                );
            }

            var html = new StringBuilder();
            var plainText = new StringBuilder();
            var subtitle =
                $"{safeSearchLabel}: {safeSearchTerm} | Warehouse scope: {warehouseCode} | Look Ahead: {safeLookAheadOption}";

            html.AppendLine(
                "<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>"
            );
            html.AppendLine(
                "<div style='font-size: 16pt; font-weight: 700; text-align: center; margin-bottom: 8px;'>Material Availability Board</div>"
            );
            html.AppendLine(
                $"<div style='font-size: 10pt; color: #445566; text-align: center; margin: 0 0 16px 0;'>{HtmlEncode(subtitle)}</div>"
            );

            plainText.AppendLine("Material Availability Board");
            plainText.AppendLine(subtitle);

            foreach (var card in cards)
            {
                AppendSectionStart(
                    html,
                    $"{card.PartId} - {card.PartDescription}",
                    CardBackground,
                    CardBorder,
                    AccentBackground,
                    AccentForeground
                );

                html.AppendLine("<div style='padding: 12px 16px 0 16px;'>");
                html.AppendLine(
                    "<table style='border-collapse: collapse; width: 100%; table-layout: auto; margin: 0 0 16px 0;'>"
                );
                html.AppendLine("<thead>");
                html.AppendLine(
                    $"<tr style='background-color: {AccentBackground}; color: {AccentForeground}; font-weight: 700;'>"
                );
                AppendHeaderCell(html, card.QuantitySummaryLabel, null);
                AppendHeaderCell(html, "Total on hand", null);
                AppendHeaderCell(html, "Locations", null);
                AppendHeaderCell(html, "Next summary", null);
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");
                html.AppendLine("<tr style='background-color: #ffffff;'>");
                AppendBodyCell(html, card.QuantitySummaryDisplay, "right");
                AppendBodyCell(html, card.TotalPositiveQuantityDisplay, "right");
                AppendBodyCell(html, card.LocationCountSummary);
                AppendBodyCell(html, card.NextDateSummary);
                html.AppendLine("</tr>");
                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
                html.AppendLine("</div>");

                plainText.AppendLine();
                plainText.AppendLine($"{card.PartId} - {card.PartDescription}");
                plainText.AppendLine($"{card.QuantitySummaryLabel}: {card.QuantitySummaryDisplay}");
                plainText.AppendLine($"Total on hand: {card.TotalPositiveQuantityDisplay}");
                plainText.AppendLine($"Locations: {card.LocationCountSummary}");
                plainText.AppendLine($"Next summary: {card.NextDateSummary}");

                AppendLocationsSection(html, plainText, card);
                AppendIncomingSection(html, plainText, card);
                AppendAssociatedPartsSection(html, plainText, card);

                AppendSectionEnd(html);
            }

            html.AppendLine("</div>");

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    new Model_FormattedReportDocument
                    {
                        HtmlFragment = html.ToString(),
                        PlainText = plainText.ToString(),
                    }
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error formatting material availability print output: {ex.Message}",
                ex
            );
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<Model_FormattedReportDocument>(
                    $"Failed to prepare printable material availability output: {ex.Message}",
                    ex
                )
            );
        }
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
            var hasQualifyingFutureShipment =
                selectedDate is not null
                && selectedDate.Date >= today
                && (incomingWindowDays.HasValue is false || selectedDate.Date <= horizon);

            if (hasQualifyingFutureShipment)
            {
                qualifyingRollupRows.Add(row);
                qualifyingLines.Add(selectedDate!);
                continue;
            }

            if (row.LineLastReceivedDate is DateTime lastShipmentDate)
            {
                qualifyingRollupRows.Add(row);
                qualifyingLines.Add(
                    new IncomingLinePresentation(
                        row,
                        lastShipmentDate.Date,
                        "Last received in shipment",
                        true
                    )
                );
                continue;
            }

            if (selectedDate is null)
            {
                qualifyingRollupRows.Add(row);
                continue;
            }
        }

        if (qualifyingRollupRows.Count == 0)
        {
            return IncomingPresentation.Empty;
        }

        var distinctDateLines = qualifyingLines
            .GroupBy(line => new { line.Label, line.Date })
            .Select(group => group.First())
            .OrderBy(line => line.IsHistorical ? 1 : 0)
            .ThenBy(line => line.IsHistorical ? DateTime.MaxValue : line.Date)
            .ThenByDescending(line => line.IsHistorical ? line.Date : DateTime.MinValue)
            .ThenBy(line => line.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var distinctDates = distinctDateLines.ConvertAll(
            line => new Model_Tool_MaterialAvailabilityIncomingDate
            {
                Label = line.Label,
                Date = line.Date,
            }
        );

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

        var earliestFutureDate = distinctDateLines
            .Where(line => line.IsHistorical is false)
            .Min(line => (DateTime?)line.Date);
        var latestHistoricalDate = distinctDateLines
            .Where(line => line.IsHistorical)
            .Max(line => (DateTime?)line.Date);

        var sortBucket =
            earliestFutureDate.HasValue ? 0
            : latestHistoricalDate.HasValue ? 1
            : 2;
        var sortDate = earliestFutureDate ?? latestHistoricalDate ?? DateTime.MaxValue;
        var firstDisplayLine = distinctDateLines.FirstOrDefault();
        var nextDateSummary = firstDisplayLine is not null
            ? firstDisplayLine.IsHistorical
                ? $"{firstDisplayLine.Label}: {firstDisplayLine.Date:MM/dd/yyyy}"
                : $"Next material date: {firstDisplayLine.Label} {firstDisplayLine.Date:MM/dd/yyyy}"
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
                WorkOrderDisplay = FormatInforVisualWorkOrder(row.WorkOrderBaseId),
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

    private static string FormatInforVisualWorkOrder(string? workOrderBaseId)
    {
        var trimmedValue = workOrderBaseId?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return string.Empty;
        }

        if (
            trimmedValue.StartsWith("WO-", StringComparison.OrdinalIgnoreCase)
            && trimmedValue.Length > 3
        )
        {
            var existingSuffix = trimmedValue[3..].Trim();
            return string.IsNullOrWhiteSpace(existingSuffix)
                ? string.Empty
                : $"WO-{existingSuffix}";
        }

        if (long.TryParse(trimmedValue, NumberStyles.None, CultureInfo.InvariantCulture, out _))
        {
            return $"WO-{trimmedValue.PadLeft(6, '0')}";
        }

        return $"WO-{trimmedValue}";
    }

    private static void AppendLocationsSection(
        StringBuilder html,
        StringBuilder plainText,
        Model_Tool_MaterialAvailabilityCard card
    )
    {
        html.AppendLine(
            "<div style='padding: 0 16px 12px 16px; font-size: 11pt; font-weight: 700; color: #1f2937;'>Current Locations</div>"
        );

        plainText.AppendLine("Current Locations");

        if (!card.HasCurrentLocations)
        {
            html.AppendLine(
                "<div style='padding: 0 16px 16px 16px; font-size: 10pt; color: #6b7280;'>No positive-quantity locations were found for this part in warehouse 002.</div>"
            );
            plainText.AppendLine(
                "No positive-quantity locations were found for this part in warehouse 002."
            );
            return;
        }

        html.AppendLine(
            "<div style='padding: 0 16px 16px 16px;'><table style='border-collapse: collapse; width: 100%; table-layout: auto;'>"
        );
        html.AppendLine("<thead>");
        html.AppendLine(
            $"<tr style='background-color: {AccentBackground}; color: {AccentForeground}; font-weight: 700;'>"
        );
        AppendHeaderCell(html, "Location", null);
        AppendHeaderCell(html, "Qty", null);
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        plainText.AppendLine("Location\tQty");

        foreach (var location in card.CurrentLocations)
        {
            html.AppendLine("<tr style='background-color: #ffffff;'>");
            AppendBodyCell(html, location.LocationLabel);
            AppendBodyCell(html, location.QuantityDisplay, "right");
            html.AppendLine("</tr>");
            plainText.AppendLine($"{location.LocationLabel}\t{location.QuantityDisplay}");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table></div>");
    }

    private static void AppendIncomingSection(
        StringBuilder html,
        StringBuilder plainText,
        Model_Tool_MaterialAvailabilityCard card
    )
    {
        html.AppendLine(
            "<div style='padding: 0 16px 12px 16px; font-size: 11pt; font-weight: 700; color: #1f2937;'>Incoming Material</div>"
        );
        plainText.AppendLine("Incoming Material");

        if (!card.HasIncomingSupply)
        {
            html.AppendLine(
                "<div style='padding: 0 16px 16px 16px; font-size: 10pt; color: #6b7280;'>No qualifying incoming material was found for the selected look-ahead window.</div>"
            );
            plainText.AppendLine(
                "No qualifying incoming material was found for the selected look-ahead window."
            );
            return;
        }

        html.AppendLine(
            "<div style='padding: 0 16px 8px 16px;'><table style='border-collapse: collapse; width: 100%; table-layout: auto;'>"
        );
        html.AppendLine("<thead>");
        html.AppendLine(
            $"<tr style='background-color: {AccentBackground}; color: {AccentForeground}; font-weight: 700;'>"
        );
        AppendHeaderCell(html, "Received", null);
        AppendHeaderCell(html, "Ordered", null);
        AppendHeaderCell(html, "Remaining", null);
        AppendHeaderCell(html, "Purchase orders", null);
        AppendHeaderCell(html, "PO lines", null);
        AppendHeaderCell(html, "% complete", null);
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        html.AppendLine("<tr style='background-color: #ffffff;'>");
        AppendBodyCell(html, card.IncomingRollup.ReceivedQtyDisplay, "right");
        AppendBodyCell(html, card.IncomingRollup.OrderedQtyDisplay, "right");
        AppendBodyCell(html, card.IncomingRollup.RemainingQtyDisplay, "right");
        AppendBodyCell(html, card.IncomingRollup.PurchaseOrderCountSummary, "right");
        AppendBodyCell(html, card.IncomingRollup.POLineCountSummary, "right");
        AppendBodyCell(html, card.IncomingRollup.PercentCompleteSummary, "right");
        html.AppendLine("</tr>");
        html.AppendLine("</tbody>");
        html.AppendLine("</table></div>");

        plainText.AppendLine(
            $"Received: {card.IncomingRollup.ReceivedQtyDisplay}\tOrdered: {card.IncomingRollup.OrderedQtyDisplay}\tRemaining: {card.IncomingRollup.RemainingQtyDisplay}\tPurchase orders: {card.IncomingRollup.PurchaseOrderCountSummary}\tPO lines: {card.IncomingRollup.POLineCountSummary}\t% complete: {card.IncomingRollup.PercentCompleteSummary}"
        );

        if (card.UpcomingDates.Count == 0)
        {
            return;
        }

        html.AppendLine(
            "<div style='padding: 0 16px 16px 16px;'><table style='border-collapse: collapse; width: 100%; table-layout: auto;'>"
        );
        html.AppendLine("<thead>");
        html.AppendLine(
            $"<tr style='background-color: {AccentBackground}; color: {AccentForeground}; font-weight: 700;'>"
        );
        AppendHeaderCell(html, "Date label", null);
        AppendHeaderCell(html, "Date", null);
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        plainText.AppendLine("Date label\tDate");

        foreach (var upcomingDate in card.UpcomingDates)
        {
            html.AppendLine("<tr style='background-color: #ffffff;'>");
            AppendBodyCell(html, upcomingDate.Label);
            AppendBodyCell(
                html,
                upcomingDate.Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
            );
            html.AppendLine("</tr>");
            plainText.AppendLine(
                $"{upcomingDate.Label}\t{upcomingDate.Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}"
            );
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table></div>");
    }

    private static void AppendAssociatedPartsSection(
        StringBuilder html,
        StringBuilder plainText,
        Model_Tool_MaterialAvailabilityCard card
    )
    {
        html.AppendLine(
            "<div style='padding: 0 16px 12px 16px; font-size: 11pt; font-weight: 700; color: #1f2937;'>Associated Parts</div>"
        );
        plainText.AppendLine("Associated Parts");

        if (!card.HasAssociatedPartRuns)
        {
            html.AppendLine(
                "<div style='padding: 0 16px 16px 16px; font-size: 10pt; color: #6b7280;'>No associated part runs were found for the selected look-ahead window.</div>"
            );
            plainText.AppendLine(
                "No associated part runs were found for the selected look-ahead window."
            );
            return;
        }

        html.AppendLine(
            "<div style='padding: 0 16px 16px 16px;'><table style='border-collapse: collapse; width: 100%; table-layout: auto;'>"
        );
        html.AppendLine("<thead>");
        html.AppendLine(
            $"<tr style='background-color: {AccentBackground}; color: {AccentForeground}; font-weight: 700;'>"
        );
        AppendHeaderCell(html, "Associated part", null);
        AppendHeaderCell(html, "Description", null);
        AppendHeaderCell(html, "Work order", null);
        AppendHeaderCell(html, "Timing", null);
        AppendHeaderCell(html, "Run date", null);
        html.AppendLine("</tr>");
        html.AppendLine("</thead>");
        html.AppendLine("<tbody>");
        plainText.AppendLine("Associated part\tDescription\tWork order\tTiming\tRun date");

        foreach (var associatedRun in card.AssociatedPartRuns)
        {
            html.AppendLine("<tr style='background-color: #ffffff;'>");
            AppendBodyCell(html, associatedRun.AssociatedPartNumber);
            AppendBodyCell(html, associatedRun.AssociatedPartDescription);
            AppendBodyCell(html, associatedRun.WorkOrderDisplay);
            AppendBodyCell(html, associatedRun.RunTimingLabel);
            AppendBodyCell(html, associatedRun.NextRunDateDisplay);
            html.AppendLine("</tr>");
            plainText.AppendLine(
                $"{associatedRun.AssociatedPartNumber}\t{associatedRun.AssociatedPartDescription}\t{associatedRun.WorkOrderDisplay}\t{associatedRun.RunTimingLabel}\t{associatedRun.NextRunDateDisplay}"
            );
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table></div>");
    }

    private static void AppendHeaderCell(StringBuilder html, string text, string? width)
    {
        var widthStyle = string.IsNullOrWhiteSpace(width) ? string.Empty : $" width: {width};";
        html.AppendLine(
            $"<th style='border: 1px solid {CardBorder}; padding: 8px 10px; text-align: left; vertical-align: top;{widthStyle}'>{HtmlEncode(text)}</th>"
        );
    }

    private static void AppendBodyCell(StringBuilder html, string? value, string alignment = "left")
    {
        html.AppendLine(
            $"<td style='border: 1px solid {CardBorder}; padding: 8px 10px; text-align: {alignment}; vertical-align: top;'>{HtmlEncode(value)}</td>"
        );
    }

    private static void AppendSectionStart(
        StringBuilder html,
        string heading,
        string cardBackground,
        string cardBorder,
        string accentBackground,
        string accentForeground
    )
    {
        html.AppendLine(
            $"<div style='margin: 0 0 16px 0; border: 1px solid {cardBorder}; border-radius: 4px; overflow: hidden; background-color: {cardBackground};'>"
        );
        html.AppendLine(
            $"<div style='padding: 12px 16px; background-color: {accentBackground}; color: {accentForeground}; font-size: 12pt; font-weight: 700;'>{HtmlEncode(heading)}</div>"
        );
    }

    private static void AppendSectionEnd(StringBuilder html)
    {
        html.AppendLine("</div>");
    }

    private static string HtmlEncode(string? value)
    {
        return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }

    private sealed record IncomingLinePresentation(
        Model_InforVisualIncomingSupplyRow Row,
        DateTime Date,
        string Label,
        bool IsHistorical
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
