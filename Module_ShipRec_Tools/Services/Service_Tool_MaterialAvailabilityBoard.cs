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

    private static readonly Dictionary<string, string> UsageUnitMap = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["EA"] = "Each",
        ["EACH"] = "Each",
        ["LB"] = "Pounds",
        ["LBS"] = "Pounds",
        ["FT"] = "Feet",
        ["IN"] = "Inches",
        ["KG"] = "Kilograms",
    };

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
        string lookAheadOption,
        bool useTransactionSheet
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
                            DocumentTitle = "Material Availability Board",
                            HtmlFragment = "<p>No material cards to print.</p>",
                            PlainText = "No material cards to print.",
                            PageCss = GetSummaryPrintCss(),
                        }
                    )
                );
            }

            if (useTransactionSheet)
            {
                var transactionSheetDocument = BuildTransactionSheetDocument(
                    cards,
                    safeSearchTerm,
                    warehouseCode,
                    safeLookAheadOption
                );

                return Task.FromResult(Model_Dao_Result_Factory.Success(transactionSheetDocument));
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
                html.AppendLine("<div class='material-card'>");
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
                AppendBodyCell(html, card.NextRunSummaryDisplay);
                html.AppendLine("</tr>");
                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
                html.AppendLine("</div>");

                plainText.AppendLine();
                plainText.AppendLine($"{card.PartId} - {card.PartDescription}");
                plainText.AppendLine($"{card.QuantitySummaryLabel}: {card.QuantitySummaryDisplay}");
                plainText.AppendLine($"Total on hand: {card.TotalPositiveQuantityDisplay}");
                plainText.AppendLine($"Locations: {card.LocationCountSummary}");
                plainText.AppendLine($"Next summary: {card.NextRunSummaryDisplay}");

                AppendLocationsSection(html, plainText, card);
                AppendIncomingSection(html, plainText, card);
                AppendAssociatedPartsSection(html, plainText, card);

                AppendSectionEnd(html);
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    new Model_FormattedReportDocument
                    {
                        DocumentTitle = "Material Availability Board",
                        HtmlFragment = html.ToString(),
                        PlainText = plainText.ToString(),
                        PageCss = GetSummaryPrintCss(),
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

    public Task<
        Model_Dao_Result<Model_FormattedReportDocument>
    > FormatIncomingMaterialForPrintAsync(Model_Tool_MaterialAvailabilityCard card)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(card);

            var html = new StringBuilder();
            var plainText = new StringBuilder();
            html.AppendLine(
                "<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>"
            );
            AppendSectionStart(
                html,
                $"Incoming Material - {card.PartId}",
                CardBackground,
                CardBorder,
                AccentBackground,
                AccentForeground
            );
            html.AppendLine("<div style='padding: 12px 16px;'>");
            html.AppendLine(
                $"<div style='font-size: 11pt; font-weight: 700; margin: 0 0 6px 0;'>{HtmlEncode(card.PartId)} - {HtmlEncode(card.PartDescription)}</div>"
            );
            html.AppendLine(
                $"<div style='font-size: 10pt; color: #445566; margin: 0 0 12px 0;'>Received {HtmlEncode(card.IncomingRollup.ReceivedQtyDisplay)} | Ordered {HtmlEncode(card.IncomingRollup.OrderedQtyDisplay)} | Remaining {HtmlEncode(card.IncomingRollup.RemainingQtyDisplay)}</div>"
            );

            plainText.AppendLine($"Incoming Material - {card.PartId}");
            plainText.AppendLine(
                $"Received {card.IncomingRollup.ReceivedQtyDisplay} | Ordered {card.IncomingRollup.OrderedQtyDisplay} | Remaining {card.IncomingRollup.RemainingQtyDisplay}"
            );

            if (!card.HasIncomingDetails)
            {
                html.AppendLine(
                    "<div style='font-size: 10pt; color: #6b7280; margin: 0 0 16px 0;'>No qualifying incoming material was found for the selected look-ahead window.</div>"
                );
                plainText.AppendLine(
                    "No qualifying incoming material was found for the selected look-ahead window."
                );
            }
            else
            {
                html.AppendLine(
                    "<table style='border-collapse: collapse; width: 100%; table-layout: auto; margin: 0 0 16px 0;'>"
                );
                html.AppendLine("<thead>");
                html.AppendLine(
                    $"<tr style='background-color: {AccentBackground}; color: {AccentForeground}; font-weight: 700;'>"
                );
                AppendHeaderCell(html, "PO / Line", null);
                AppendHeaderCell(html, "Date", null);
                AppendHeaderCell(html, "Vendor", null);
                AppendHeaderCell(html, "Received", null);
                AppendHeaderCell(html, "Ordered", null);
                AppendHeaderCell(html, "Remaining", null);
                html.AppendLine("</tr></thead><tbody>");

                plainText.AppendLine("PO / Line\tDate\tVendor\tReceived\tOrdered\tRemaining");

                foreach (var line in card.IncomingLines)
                {
                    html.AppendLine("<tr style='background-color: #ffffff;'>");
                    AppendBodyCell(html, line.PurchaseOrderLineDisplay);
                    AppendBodyCell(
                        html,
                        string.IsNullOrWhiteSpace(line.DateLabel)
                            ? line.DateDisplay
                            : $"{line.DateLabel} {line.DateDisplay}"
                    );
                    AppendBodyCell(html, line.VendorName);
                    AppendBodyCell(html, line.ReceivedQtyDisplay, "right");
                    AppendBodyCell(html, line.OrderedQtyDisplay, "right");
                    AppendBodyCell(html, line.RemainingQtyDisplay, "right");
                    html.AppendLine("</tr>");

                    plainText.AppendLine(
                        $"{line.PurchaseOrderLineDisplay}\t{line.DateDisplay}\t{line.VendorName}\t{line.ReceivedQtyDisplay}\t{line.OrderedQtyDisplay}\t{line.RemainingQtyDisplay}"
                    );
                }

                html.AppendLine("</tbody></table>");
            }

            html.AppendLine("</div>");
            AppendSectionEnd(html);
            html.AppendLine("</div>");

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    new Model_FormattedReportDocument
                    {
                        DocumentTitle = $"Incoming Material - {card.PartId}",
                        HtmlFragment = html.ToString(),
                        PlainText = plainText.ToString(),
                    }
                )
            );
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<Model_FormattedReportDocument>(
                    $"Failed to prepare incoming material print output: {ex.Message}",
                    ex
                )
            );
        }
    }

    public Task<
        Model_Dao_Result<Model_FormattedReportDocument>
    > FormatWorkOrderDetailsForPrintAsync(
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun,
        IReadOnlyList<Model_Tool_MaterialAvailabilityDetailSection> sections
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(associatedRun);
            ArgumentNullException.ThrowIfNull(sections);

            var html = new StringBuilder();
            var plainText = new StringBuilder();
            html.AppendLine(
                "<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>"
            );
            AppendSectionStart(
                html,
                $"Work Order Details - {associatedRun.WorkOrderDisplay}",
                CardBackground,
                CardBorder,
                AccentBackground,
                AccentForeground
            );
            html.AppendLine("<div style='padding: 12px 16px;'>");
            html.AppendLine(
                $"<div style='font-size: 11pt; font-weight: 700; margin: 0 0 6px 0;'>{HtmlEncode(associatedRun.WorkOrderDisplay)} / {HtmlEncode(associatedRun.AssociatedPartNumber)}</div>"
            );
            html.AppendLine(
                $"<div style='font-size: 10pt; color: #445566; margin: 0 0 12px 0;'>Required Parts: {HtmlEncode(associatedRun.RequiredPartsQuantityDisplay)} | Estimated Coil Use: {HtmlEncode(associatedRun.EstimatedCoilUseDisplay)} {HtmlEncode(associatedRun.NormalizedUsageUnitOfMeasure)}</div>"
            );

            plainText.AppendLine($"Work Order Details - {associatedRun.WorkOrderDisplay}");
            plainText.AppendLine($"Required Parts: {associatedRun.RequiredPartsQuantityDisplay}");
            plainText.AppendLine(
                $"Estimated Coil Use: {associatedRun.EstimatedCoilUseDisplay} {associatedRun.NormalizedUsageUnitOfMeasure}"
            );

            foreach (var section in sections.Where(section => section.Fields.Count > 0))
            {
                html.AppendLine(
                    $"<div style='font-size: 11pt; font-weight: 700; color: #1f2937; margin: 12px 0 8px 0;'>{HtmlEncode(section.Title)}</div>"
                );
                html.AppendLine(
                    "<table style='border-collapse: collapse; width: 100%; table-layout: auto; margin: 0 0 8px 0;'><tbody>"
                );
                plainText.AppendLine();
                plainText.AppendLine(section.Title);

                foreach (var field in section.Fields)
                {
                    html.AppendLine("<tr style='background-color: #ffffff;'>");
                    AppendBodyCell(html, field.Label);
                    AppendBodyCell(html, field.Value);
                    html.AppendLine("</tr>");
                    plainText.AppendLine($"{field.Label}: {field.Value}");
                }

                html.AppendLine("</tbody></table>");
            }

            html.AppendLine("</div>");
            AppendSectionEnd(html);
            html.AppendLine("</div>");

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    new Model_FormattedReportDocument
                    {
                        DocumentTitle = $"Work Order Details - {associatedRun.WorkOrderDisplay}",
                        HtmlFragment = html.ToString(),
                        PlainText = plainText.ToString(),
                    }
                )
            );
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<Model_FormattedReportDocument>(
                    $"Failed to prepare work-order detail print output: {ex.Message}",
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
                    IncomingLines = incomingPresentation.IncomingLines,
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
            if (selectedDate is not null)
            {
                qualifyingRollupRows.Add(row);
                qualifyingLines.Add(selectedDate);
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

        var incomingLines = qualifyingLines
            .GroupBy(line => new
            {
                PONumber = (line.Row.PONumber ?? string.Empty).Trim().ToUpperInvariant(),
                POLineNumber = (line.Row.POLineNumber ?? string.Empty).Trim().ToUpperInvariant(),
            })
            .Select(group =>
                group.OrderBy(line => line.IsHistorical ? 1 : 0).ThenBy(line => line.Date).First()
            )
            .OrderBy(line => line.IsHistorical ? 1 : 0)
            .ThenBy(line => line.Date)
            .ThenBy(line => line.Row.PONumber, StringComparer.OrdinalIgnoreCase)
            .Select(line => new Model_Tool_MaterialAvailabilityIncomingLine
            {
                PONumber = line.Row.PONumber,
                POLineNumber = line.Row.POLineNumber,
                VendorName = line.Row.VendorName,
                DateLabel = line.Label,
                Date = line.Date,
                OrderedQty = line.Row.OrderedQty,
                ReceivedQty = line.Row.ReceivedQty,
                RemainingQty = line.Row.RemainingQty,
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

        var earliestDisplayDate = distinctDateLines
            .Where(line => line.IsHistorical is false)
            .Min(line => (DateTime?)line.Date);
        var latestHistoricalDate = distinctDateLines
            .Where(line => line.IsHistorical)
            .Max(line => (DateTime?)line.Date);

        var sortBucket =
            earliestDisplayDate.HasValue ? 0
            : latestHistoricalDate.HasValue ? 1
            : 2;
        var sortDate = earliestDisplayDate ?? latestHistoricalDate ?? DateTime.MaxValue;
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
            incomingLines,
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
            return AssociatedPartPresentation.Empty();
        }

        var horizon = incomingWindowDays.HasValue
            ? DateTime.Today.AddDays(incomingWindowDays.Value)
            : DateTime.MaxValue;

        var associatedRuns = partAssociatedRuns
            .GroupBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                group
                    .OrderBy(row => GetAssociatedPartPriority(row, incomingWindowDays, horizon))
                    .ThenByDescending(row => GetAssociatedPartCurrentSortDate(row))
                    .ThenBy(row => GetAssociatedPartFutureSortDate(row))
                    .ThenByDescending(row => GetAssociatedPartHistoricalSortDate(row))
                    .ThenBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(row => row.WorkOrderBaseId, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(row => row.OperationSeqNo)
                    .ThenBy(row => row.RequirementPieceNo)
                    .First()
            )
            .OrderBy(row => GetAssociatedPartPriority(row, incomingWindowDays, horizon))
            .ThenByDescending(row => GetAssociatedPartCurrentSortDate(row))
            .ThenBy(row => GetAssociatedPartFutureSortDate(row))
            .ThenByDescending(row => GetAssociatedPartHistoricalSortDate(row))
            .ThenBy(row => row.AssociatedPartNumber, StringComparer.OrdinalIgnoreCase)
            .Select(row => new Model_Tool_MaterialAvailabilityAssociatedPartRun
            {
                AssociatedPartNumber = row.AssociatedPartNumber,
                AssociatedPartDescription = row.AssociatedPartDescription,
                ComponentPartNumber = row.ComponentPartNumber,
                NextDueToRunDate = row.NextDueToRunDate?.Date,
                IsFutureOrTodayRun = row.IsFutureOrTodayRun,
                IsCurrentlyRunning = row.IsCurrentlyRunning,
                IsPastJob = row.IsPastJob,
                IsOutsideSelectedLookAheadWindow =
                    row.NextDueToRunDate.HasValue
                    && row.IsFutureOrTodayRun
                    && incomingWindowDays.HasValue
                    && row.NextDueToRunDate.Value.Date > horizon,
                NextDueDateSource = row.NextDueDateSource,
                WorkOrderDisplay = FormatInforVisualWorkOrder(row.WorkOrderBaseId),
                WorkOrderStatus = row.WorkOrderStatus,
                WorkOrderStatusEffectiveDate = row.WorkOrderStatusEffectiveDate,
                SiteId = row.SiteId,
                ScheduledStartDate = row.ScheduledStartDate,
                ScheduledFinishDate = row.ScheduledFinishDate,
                RequiredDate = row.RequiredDate,
                OperationSequence = row.OperationSeqNo,
                QtyPer = row.QtyPer,
                FixedQty = row.FixedQty,
                CalculatedQty = row.CalcQty,
                IssuedQty = row.IssuedQty,
                AllocatedQty = row.AllocatedQty,
                FulfilledQty = row.FulfilledQty,
                UsageUnitOfMeasure = row.UsageUm,
                ScrapPercent = row.ScrapPercent,
                OperationType = row.OperationType,
                ResourceId = row.ResourceId,
                ServiceId = row.ServiceId,
                OperationWarehouseId = row.OperationWarehouseId,
                RunQtyPerCycle = row.RunQtyPerCycle,
                SetupHours = row.SetupHours,
                RunHours = row.RunHours,
                Dimensions = row.Dimensions,
                DimensionExpression = row.DimensionExpression,
                Length = row.Length,
                Width = row.Width,
                Height = row.Height,
                DrawingId = row.DrawingId,
                DrawingRevision = row.DrawingRevision,
                RequiredPartsQuantity = CalculateRequiredPartsQuantity(row),
                EstimatedCoilUse = CalculateEstimatedCoilUse(row),
                NormalizedUsageUnitOfMeasure = NormalizeUsageUnit(row.UsageUm),
            })
            .ToList();

        if (associatedRuns.Count == 0)
        {
            return AssociatedPartPresentation.Empty();
        }

        var firstRun = associatedRuns[0];
        var usageUnit = string.IsNullOrWhiteSpace(firstRun.NormalizedUsageUnitOfMeasure)
            ? string.Empty
            : $" {firstRun.NormalizedUsageUnitOfMeasure}";
        var summarySubject = string.IsNullOrWhiteSpace(firstRun.WorkOrderDisplay)
            ? firstRun.AssociatedPartNumber
            : $"{firstRun.WorkOrderDisplay} / {firstRun.AssociatedPartNumber}";
        var summaryDate = firstRun.NextDueToRunDate.HasValue
            ? $" on {firstRun.NextRunDateDisplay}"
            : string.Empty;
        var summaryText =
            $"{firstRun.RunTimingLabel}: {summarySubject}{summaryDate} | Required Parts: {firstRun.RequiredPartsQuantityDisplay} | Estimated Coil Use: {firstRun.EstimatedCoilUseDisplay}{usageUnit}";

        return new AssociatedPartPresentation(
            associatedRuns,
            summaryText,
            GetAssociatedPartSortBucket(firstRun),
            firstRun.NextDueToRunDate ?? DateTime.MaxValue
        );
    }

    private static IncomingLinePresentation? SelectBestIncomingDate(
        Model_InforVisualIncomingSupplyRow row
    )
    {
        if (row.LinePromiseDate is DateTime linePromiseDate)
        {
            return new IncomingLinePresentation(
                row,
                linePromiseDate.Date,
                "Line promise delivery",
                false
            );
        }

        if (row.LinePromiseShipDate is DateTime linePromiseShipDate)
        {
            return new IncomingLinePresentation(
                row,
                linePromiseShipDate.Date,
                "Line promise ship",
                false
            );
        }

        if (row.HeaderPromiseDate is DateTime headerPromiseDate)
        {
            return new IncomingLinePresentation(
                row,
                headerPromiseDate.Date,
                "PO promise delivery",
                false
            );
        }

        if (row.HeaderPromiseShipDate is DateTime headerPromiseShipDate)
        {
            return new IncomingLinePresentation(
                row,
                headerPromiseShipDate.Date,
                "PO promise ship",
                false
            );
        }

        if (row.LineDesiredReceiveDate is DateTime lineDesiredDate)
        {
            return new IncomingLinePresentation(
                row,
                lineDesiredDate.Date,
                "Line desired receive",
                false
            );
        }

        if (row.HeaderDesiredReceiveDate is DateTime headerDesiredDate)
        {
            return new IncomingLinePresentation(
                row,
                headerDesiredDate.Date,
                "PO desired receive",
                false
            );
        }

        if (row.OrderDate is DateTime orderDate)
        {
            return new IncomingLinePresentation(row, orderDate.Date, "PO order date", false);
        }

        return null;
    }

    private static decimal CalculateRequiredPartsQuantity(Model_InforVisualAssociatedPartRunRow row)
    {
        var componentRequirementQuantity = GetComponentRequirementQuantity(row);
        if (componentRequirementQuantity <= 0)
        {
            return 0;
        }

        var qtyPer = row.QtyPer.GetValueOrDefault();
        if (qtyPer <= 0)
        {
            return componentRequirementQuantity;
        }

        var fixedQty = row.FixedQty.GetValueOrDefault();
        var variableRequirementQuantity = decimal.Max(componentRequirementQuantity - fixedQty, 0);
        if (variableRequirementQuantity <= 0)
        {
            return 0;
        }

        return decimal.Round(
            variableRequirementQuantity / qtyPer,
            2,
            MidpointRounding.AwayFromZero
        );
    }

    private static decimal CalculateEstimatedCoilUse(Model_InforVisualAssociatedPartRunRow row)
    {
        var componentRequirementQuantity = GetComponentRequirementQuantity(row);
        if (componentRequirementQuantity <= 0)
        {
            return 0;
        }

        var scrapMultiplier = 1 + (row.ScrapPercent.GetValueOrDefault() / 100M);
        return decimal.Round(
            componentRequirementQuantity * scrapMultiplier,
            2,
            MidpointRounding.AwayFromZero
        );
    }

    private static decimal GetComponentRequirementQuantity(
        Model_InforVisualAssociatedPartRunRow row
    )
    {
        if (row.CalcQty.HasValue && row.CalcQty.Value > 0)
        {
            return row.CalcQty.Value;
        }

        var fixedQty = row.FixedQty.GetValueOrDefault();
        var qtyPer = row.QtyPer.GetValueOrDefault();
        if (fixedQty > 0 || qtyPer > 0)
        {
            return fixedQty + qtyPer;
        }

        if (row.AllocatedQty.HasValue && row.AllocatedQty.Value > 0)
        {
            return row.AllocatedQty.Value;
        }

        if (row.FulfilledQty.HasValue && row.FulfilledQty.Value > 0)
        {
            return row.FulfilledQty.Value;
        }

        if (row.IssuedQty.HasValue && row.IssuedQty.Value > 0)
        {
            return row.IssuedQty.Value;
        }

        return 0;
    }

    private static string NormalizeUsageUnit(string? usageUnit)
    {
        if (string.IsNullOrWhiteSpace(usageUnit))
        {
            return string.Empty;
        }

        var trimmedValue = usageUnit.Trim();
        return UsageUnitMap.TryGetValue(trimmedValue, out var normalizedValue)
            ? normalizedValue
            : trimmedValue;
    }

    private static string FormatInforVisualWorkOrder(string? workOrderBaseId)
    {
        var normalizedBaseId = NormalizeWorkOrderBaseId(workOrderBaseId);
        if (string.IsNullOrWhiteSpace(normalizedBaseId))
        {
            return string.Empty;
        }

        var formattedBaseId = long.TryParse(
            normalizedBaseId,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out _
        )
            ? normalizedBaseId.PadLeft(6, '0')
            : normalizedBaseId;

        return $"WO-{formattedBaseId}";
    }

    private static string NormalizeWorkOrderBaseId(string? workOrderBaseId)
    {
        if (string.IsNullOrWhiteSpace(workOrderBaseId))
        {
            return string.Empty;
        }

        var normalizedValue = workOrderBaseId.Trim();
        while (TryTrimKnownWorkOrderPrefix(normalizedValue, out var trimmedValue))
        {
            normalizedValue = trimmedValue;
        }

        return normalizedValue.Trim();
    }

    private static bool TryTrimKnownWorkOrderPrefix(string value, out string trimmedValue)
    {
        trimmedValue = value;

        if (
            value.StartsWith("WO", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("CO", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("PO", StringComparison.OrdinalIgnoreCase)
        )
        {
            trimmedValue = value[2..].TrimStart('-', ' ');
            return true;
        }

        return false;
    }

    private static int GetAssociatedPartPriority(
        Model_InforVisualAssociatedPartRunRow row,
        int? incomingWindowDays,
        DateTime horizon
    )
    {
        if (row.IsCurrentlyRunning)
        {
            return 0;
        }

        if (!row.NextDueToRunDate.HasValue)
        {
            return 4;
        }

        var runDate = row.NextDueToRunDate.Value.Date;
        if (row.IsFutureOrTodayRun)
        {
            return incomingWindowDays.HasValue && runDate > horizon ? 2 : 1;
        }

        if (row.IsPastJob)
        {
            return 3;
        }

        return 4;
    }

    private static DateTime GetAssociatedPartCurrentSortDate(
        Model_InforVisualAssociatedPartRunRow row
    )
    {
        return row.IsCurrentlyRunning
            ? (row.ScheduledStartDate ?? row.NextDueToRunDate ?? DateTime.MinValue).Date
            : DateTime.MinValue;
    }

    private static DateTime GetAssociatedPartFutureSortDate(
        Model_InforVisualAssociatedPartRunRow row
    )
    {
        return row.NextDueToRunDate.HasValue && row.IsFutureOrTodayRun
            ? row.NextDueToRunDate.Value.Date
            : DateTime.MaxValue;
    }

    private static DateTime GetAssociatedPartHistoricalSortDate(
        Model_InforVisualAssociatedPartRunRow row
    )
    {
        return row.IsPastJob
            ? (row.ScheduledFinishDate ?? row.NextDueToRunDate ?? DateTime.MinValue).Date
            : DateTime.MinValue;
    }

    private static int GetAssociatedPartSortBucket(
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun
    )
    {
        if (associatedRun.NextDueToRunDate.HasValue is false)
        {
            return 4;
        }

        if (associatedRun.IsCurrentlyRunning)
        {
            return 0;
        }

        if (associatedRun.IsFutureOrTodayRun && !associatedRun.IsOutsideSelectedLookAheadWindow)
        {
            return 1;
        }

        if (associatedRun.IsFutureOrTodayRun)
        {
            return 2;
        }

        if (associatedRun.IsPastJob)
        {
            return 3;
        }

        return 4;
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
                "<div style='padding: 0 16px 16px 16px; font-size: 10pt; color: #6b7280;'>No associated parts were found for this material.</div>"
            );
            plainText.AppendLine("No associated parts were found for this material.");
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

    private static Model_FormattedReportDocument BuildTransactionSheetDocument(
        IReadOnlyList<Model_Tool_MaterialAvailabilityCard> cards,
        string locationId,
        string warehouseCode,
        string lookAheadOption
    )
    {
        var html = new StringBuilder();
        var plainText = new StringBuilder();

        html.AppendLine("<div class='transaction-sheet-wrapper'>");
        html.AppendLine(
            "<div class='transaction-sheet-title'>Material Availability Transaction Sheet</div>"
        );
        html.AppendLine(
            $"<div class='transaction-sheet-subtitle'>Warehouse Location: {HtmlEncode(locationId)} | Warehouse scope: {HtmlEncode(warehouseCode)} | Look Ahead: {HtmlEncode(lookAheadOption)}</div>"
        );
        html.AppendLine("<table class='transaction-sheet'>");
        html.AppendLine(
            "<thead><tr><th class='identity-header'>Part Number / Taken From</th><th class='entries-header'>Coil Transfer Entries</th></tr></thead>"
        );
        html.AppendLine("<tbody>");

        plainText.AppendLine("Material Availability Transaction Sheet");
        plainText.AppendLine(
            $"Warehouse Location: {locationId} | Warehouse scope: {warehouseCode} | Look Ahead: {lookAheadOption}"
        );
        plainText.AppendLine();

        foreach (var card in cards.OrderBy(card => card.PartId, StringComparer.OrdinalIgnoreCase))
        {
            var takenFrom = string.IsNullOrWhiteSpace(card.SearchLocationId)
                ? locationId
                : card.SearchLocationId;

            html.AppendLine("<tr class='transaction-row'>");
            html.AppendLine("<td class='identity-cell'>");
            html.AppendLine("<div class='identity-card'>");
            html.AppendLine("<div class='identity-label'>Part Number</div>");
            html.AppendLine($"<div class='identity-part-value'>{HtmlEncode(card.PartId)}</div>");
            html.AppendLine("<div class='identity-divider'></div>");
            html.AppendLine("<div class='identity-label'>Taken From</div>");
            html.AppendLine($"<div class='identity-from-value'>{HtmlEncode(takenFrom)}</div>");
            html.AppendLine("</div>");
            html.AppendLine("</td>");
            html.AppendLine("<td class='entries-cell'>");
            AppendTransactionEntryGrid(html);
            html.AppendLine("</td>");
            html.AppendLine("</tr>");

            plainText.AppendLine($"Part Number: {card.PartId}");
            plainText.AppendLine($"Taken From: {takenFrom}");
            plainText.AppendLine("Coil Transfer Entries: 3 rows with 5 quantity/to pairs each.");
            plainText.AppendLine();
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
        html.AppendLine("</div>");

        return new Model_FormattedReportDocument
        {
            DocumentTitle = "Material Availability Transaction Sheet",
            HtmlFragment = html.ToString(),
            PlainText = plainText.ToString(),
            PageCss = GetTransactionSheetPrintCss(),
        };
    }

    private static void AppendTransactionEntryGrid(StringBuilder html)
    {
        html.AppendLine("<table class='entry-grid'>");
        html.AppendLine("<thead><tr>");
        for (var columnIndex = 1; columnIndex <= 5; columnIndex++)
        {
            html.AppendLine($"<th>Qty {columnIndex}</th>");
            html.AppendLine($"<th>Take To {columnIndex}</th>");

            if (columnIndex < 5)
            {
                html.AppendLine("<th class='entry-spacer-header'></th>");
            }
        }
        html.AppendLine("</tr></thead>");
        html.AppendLine("<tbody>");

        for (var rowIndex = 0; rowIndex < 3; rowIndex++)
        {
            html.AppendLine("<tr>");
            for (var columnIndex = 1; columnIndex <= 5; columnIndex++)
            {
                html.AppendLine("<td class='entry-cell'>&nbsp;</td>");
                html.AppendLine("<td class='entry-cell'>&nbsp;</td>");

                if (columnIndex < 5)
                {
                    html.AppendLine("<td class='entry-spacer-cell'></td>");
                }
            }
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody>");
        html.AppendLine("</table>");
    }

    private static string GetSummaryPrintCss()
    {
        return """
@page { margin: 0.35in; }
body { margin: 0; background: #ffffff; }
.material-card { break-after: page; page-break-after: always; }
.material-card:last-child { break-after: auto; page-break-after: auto; }
@media print {
    .material-card { break-inside: avoid; page-break-inside: avoid; }
}
""";
    }

    private static string GetTransactionSheetPrintCss()
    {
        return """
@page { size: Letter portrait; margin: 0.2in; }
body { margin: 0; background: #ffffff; }
.transaction-sheet-wrapper { font-family: Calibri, Arial, sans-serif; font-size: 10pt; color: #111827; }
.transaction-sheet-title { font-size: 15pt; font-weight: 700; text-align: center; margin: 0 0 4px 0; }
.transaction-sheet-subtitle { font-size: 9pt; text-align: center; margin: 0 0 8px 0; }
.transaction-sheet { width: 100%; border-collapse: collapse; table-layout: auto; }
.transaction-sheet thead { display: table-header-group; }
.transaction-sheet th, .transaction-sheet td { border: 1px solid #111827; padding: 3px 4px; vertical-align: top; }
.transaction-sheet th { background: #f3f4f6; font-weight: 700; text-align: left; }
.transaction-row { break-inside: avoid; page-break-inside: avoid; }
.identity-header { width: 1%; white-space: nowrap; }
.entries-header { width: 99%; }
.identity-cell { width: 1%; white-space: nowrap; background: #faf5ff; padding: 5px; }
.identity-card {
    min-width: 1.9in;
    background: linear-gradient(180deg, #fcfaff 0%, #f3e8ff 100%);
    border: 1px solid #c4b5fd;
    border-radius: 8px;
    padding: 8px 10px;
    box-sizing: border-box;
}
.identity-label {
    font-size: 7.5pt;
    font-weight: 700;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    color: #6b21a8;
    margin: 0 0 2px 0;
}
.identity-part-value {
    font-size: 12pt;
    font-weight: 700;
    color: #111827;
    margin: 0;
}
.identity-from-value {
    font-size: 10.5pt;
    font-weight: 600;
    color: #1f2937;
    margin: 0;
}
.identity-divider {
    height: 1px;
    margin: 8px 0 7px 0;
    background: rgba(107, 33, 168, 0.22);
}
.entries-cell { width: auto; padding: 2px; }
.entry-grid { width: 100%; border-collapse: collapse; table-layout: fixed; }
.entry-grid th, .entry-grid td { border: 1px solid #9ca3af; padding: 2px 3px; }
.entry-grid th { background: #ffffff; font-size: 8pt; font-weight: 600; }
.entry-spacer-header, .entry-spacer-cell {
    border: none !important;
    background: transparent !important;
    padding: 0 !important;
    width: 10px;
}
.entry-cell { height: 28px; }
""";
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
        List<Model_Tool_MaterialAvailabilityIncomingLine> IncomingLines,
        Model_Tool_MaterialAvailabilityRollup Rollup,
        string NextDateSummary,
        int SortBucket,
        DateTime SortDate
    )
    {
        public static IncomingPresentation Empty { get; } =
            new(
                UpcomingDates: [],
                IncomingLines: [],
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
        public static AssociatedPartPresentation Empty()
        {
            var summaryText = "No associated parts were found.";

            return new AssociatedPartPresentation([], summaryText, 2, DateTime.MaxValue);
        }
    }
}
