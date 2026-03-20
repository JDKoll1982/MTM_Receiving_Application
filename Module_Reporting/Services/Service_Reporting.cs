using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Data;

namespace MTM_Receiving_Application.Module_Reporting.Services;

public class Service_Reporting : IService_Reporting
{
    private const string CardBackground = "#FFFFFF";
    private const string CardBorder = "#D0DAE5";
    private const string SummaryAccentBackground = "#0B6157";
    private const string SummaryAccentForeground = "#FFFFFF";
    private const string DunnageAccentBackground = "#8A5E00";
    private const string DunnageAccentForeground = "#FFFFFF";
    private const string DetailAccentBackground = "#1E4F7A";
    private const string DetailAccentForeground = "#FFFFFF";

    private readonly Dao_Reporting _dao;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ReceivingSettings _receivingSettings;

    public Service_Reporting(
        Dao_Reporting dao,
        IService_LoggingUtility logger,
        IService_ReceivingSettings receivingSettings
    )
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _receivingSettings =
            receivingSettings ?? throw new ArgumentNullException(nameof(receivingSettings));
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        _logger.LogInfo(
            $"Retrieving Receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
        );
        var result = await _dao.GetReceivingHistoryAsync(startDate, endDate);

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var row in result.Data)
            {
                row.PONumber = NormalizePONumber(row.PONumber);
            }
        }

        return result;
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        _logger.LogInfo(
            $"Retrieving Dunnage history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
        );
        return await _dao.GetDunnageHistoryAsync(startDate, endDate);
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        _logger.LogInfo(
            $"Retrieving Volvo history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
        );
        return await _dao.GetVolvoHistoryAsync(startDate, endDate);
    }

    public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        _logger.LogInfo(
            $"Checking module availability from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
        );
        return await _dao.CheckAvailabilityAsync(startDate, endDate);
    }

    public string NormalizePONumber(string? poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return string.Empty;
        }

        poNumber = poNumber.Trim();

        if (poNumber.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
        {
            return poNumber;
        }

        if (poNumber.Equals("Customer Supplied", StringComparison.OrdinalIgnoreCase))
        {
            return "Customer Supplied";
        }

        if (!char.IsDigit(poNumber[0]))
        {
            return poNumber;
        }

        string numericPart = new string(poNumber.TakeWhile(char.IsDigit).ToArray());
        string suffix = poNumber.Substring(numericPart.Length);

        if (numericPart.Length < 5)
        {
            return poNumber;
        }

        if (numericPart.Length == 5)
        {
            numericPart = "0" + numericPart;
        }

        return "PO-" + numericPart + suffix;
    }

    public async Task<Model_Dao_Result<List<Model_ReportSummaryTable>>> BuildSummaryTablesAsync(
        List<Model_ReportSection> sections
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sections);

            var prefixRules = await GetEnabledPrefixRulesAsync();
            var summaryTables = sections
                .Where(section => section.Rows.Count > 0)
                .Select(section => BuildSummaryTable(section, prefixRules))
                .Where(table => table.Rows.Count > 0)
                .ToList();

            return Model_Dao_Result_Factory.Success(summaryTables);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error building summary tables: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_ReportSummaryTable>>(
                $"Failed to build summary tables: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<Model_FormattedReportDocument>> FormatForEmailAsync(
        List<Model_ReportSection> sections,
        List<Model_ReportSummaryTable> summaryTables,
        string summaryTitle
    )
    {
        try
        {
            _logger.LogInfo($"Formatting {sections.Count} report sections for email");

            if (sections.Count == 0)
            {
                return Model_Dao_Result_Factory.Success(
                    new Model_FormattedReportDocument
                    {
                        HtmlFragment = "<p>No data to display</p>",
                        PlainText = "No data to display",
                    }
                );
            }

            var html = new StringBuilder();
            var plainText = new StringBuilder();

            html.AppendLine(
                "<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>"
            );

            html.AppendLine(
                $"<div style='font-size: 16pt; font-weight: 700; text-align: center; margin-bottom: 8px;'>{System.Net.WebUtility.HtmlEncode(summaryTitle)}</div>"
            );
            plainText.AppendLine(summaryTitle);

            var summaryLookup = summaryTables.ToDictionary(
                table => table.ModuleName,
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var section in sections)
            {
                var summaryTable = summaryLookup.TryGetValue(
                    section.ModuleName,
                    out var matchedSummaryTable
                )
                    ? matchedSummaryTable
                    : null;
                var (accentBackground, accentForeground) = GetModuleAccent(section.ModuleName);

                AppendSectionStart(
                    html,
                    $"{section.ModuleName} Report for {GetDateRangeText(section.Title)}",
                    CardBackground,
                    CardBorder,
                    accentBackground,
                    accentForeground
                );

                plainText.AppendLine();
                plainText.AppendLine(
                    $"{section.ModuleName} Report for {GetDateRangeText(section.Title)}"
                );

                if (summaryTable is not null)
                {
                    html.AppendLine(
                        "<div style='font-size: 11pt; font-weight: 700; color: #1f2937; margin: 0 0 8px 0;'>Summary</div>"
                    );
                    html.AppendLine(
                        "<table style='border-collapse: collapse; width: 100%; table-layout: auto; margin: 6px 0 16px 0;'>"
                    );
                    html.AppendLine("<thead>");
                    html.AppendLine(
                        $"<tr style='background-color: {accentBackground}; color: {accentForeground}; font-weight: 700;'>"
                    );

                    foreach (var column in summaryTable.Columns)
                    {
                        AppendHeaderCell(html, column.Header, null);
                    }

                    html.AppendLine("</tr>");
                    html.AppendLine("</thead>");
                    html.AppendLine("<tbody>");

                    plainText.AppendLine("Summary");
                    plainText
                        .AppendJoin('\t', summaryTable.Columns.Select(column => column.Header))
                        .AppendLine();

                    foreach (var row in summaryTable.Rows)
                    {
                        html.AppendLine("<tr style='background-color: #ffffff;'>");

                        foreach (var cell in row.Cells)
                        {
                            AppendBodyCell(html, cell.Value, "right");
                        }

                        html.AppendLine("</tr>");
                        plainText
                            .AppendJoin('\t', row.Cells.Select(cell => cell.Value))
                            .AppendLine();
                    }

                    html.AppendLine("</tbody>");
                    html.AppendLine("</table>");
                }

                html.AppendLine(
                    "<div style='font-size: 11pt; font-weight: 700; color: #1f2937; margin: 0 0 8px 0;'>Detailed Activity</div>"
                );

                if (!string.IsNullOrWhiteSpace(section.Description))
                {
                    html.AppendLine(
                        $"<div style='font-size: 10pt; color: #445566; text-align: center; margin: 0 0 12px 0;'>{System.Net.WebUtility.HtmlEncode(section.Description)}</div>"
                    );
                    plainText.AppendLine(section.Description);
                }

                html.AppendLine(
                    "<table style='border-collapse: collapse; width: 100%; table-layout: fixed; margin: 6px 0 10px 0;'>"
                );
                html.AppendLine("<thead>");
                html.AppendLine(
                    $"<tr style='background-color: {accentBackground}; color: {accentForeground}; font-weight: 700;'>"
                );
                AppendHeaderCell(html, "Quantity", "14%");
                AppendHeaderCell(html, "Item", "22%");
                AppendHeaderCell(html, "Reference", "18%");
                AppendHeaderCell(html, "Employee", "10%");
                AppendHeaderCell(html, "Date", "14%");
                AppendHeaderCell(html, "Location", "12%");
                AppendHeaderCell(html, "Packages/Loads", "10%");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");

                plainText.AppendLine("Detailed Activity");
                plainText.AppendLine(
                    "Quantity\tItem\tReference\tEmployee\tDate\tLocation\tPackages/Loads"
                );

                foreach (var row in section.Rows)
                {
                    html.AppendLine("<tr style='background-color: #ffffff;'>");
                    AppendBodyCell(html, row.DisplayQuantity, "right");
                    AppendBodyCell(html, row.DisplayPartOrDunnage);
                    AppendBodyCell(html, row.DisplayPo);
                    AppendBodyCell(html, row.EmployeeNumber);
                    AppendBodyCell(
                        html,
                        row.CreatedDate.ToString("M/d/yyyy", CultureInfo.InvariantCulture)
                    );
                    AppendBodyCell(html, row.DisplayLocation);
                    AppendBodyCell(html, row.DisplayLoadsOrSkids, "right");
                    html.AppendLine("</tr>");

                    plainText.AppendLine(
                        $"{row.DisplayQuantity}\t{row.DisplayPartOrDunnage}\t{row.DisplayPo}\t{row.EmployeeNumber}\t{row.CreatedDate:M/d/yyyy}\t{row.DisplayLocation}\t{row.DisplayLoadsOrSkids}"
                    );
                }

                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
                AppendSectionEnd(html);
            }

            html.AppendLine("</div>");

            return Model_Dao_Result_Factory.Success(
                new Model_FormattedReportDocument
                {
                    HtmlFragment = html.ToString(),
                    PlainText = plainText.ToString(),
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error formatting email: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_FormattedReportDocument>(
                $"Email formatting failed: {ex.Message}",
                ex
            );
        }
    }

    private async Task<List<Model_PartNumberPrefixRule>> GetEnabledPrefixRulesAsync()
    {
        var rulesJson = await _receivingSettings.GetStringAsync(
            ReceivingSettingsKeys.PartNumberPadding.RulesJson
        );
        if (string.IsNullOrWhiteSpace(rulesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer
                    .Deserialize<List<Model_PartNumberPrefixRule>>(rulesJson)
                    ?.Where(rule => rule.IsEnabled && !string.IsNullOrWhiteSpace(rule.Prefix))
                    .ToList()
                ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                $"Error parsing prefix rules for reporting summaries: {ex.Message}",
                ex
            );
            return [];
        }
    }

    private static Model_ReportSummaryTable BuildSummaryTable(
        Model_ReportSection section,
        IReadOnlyList<Model_PartNumberPrefixRule> configuredRules
    )
    {
        return section.ModuleName switch
        {
            "Receiving" => BuildReceivingSummaryTable(section, configuredRules),
            "Dunnage" => BuildDunnageSummaryTable(section),
            "Volvo" => BuildVolvoSummaryTable(section),
            _ => BuildGenericSummaryTable(section),
        };
    }

    private static Model_ReportSummaryTable BuildReceivingSummaryTable(
        Model_ReportSection section,
        IReadOnlyList<Model_PartNumberPrefixRule> configuredRules
    )
    {
        var rows = section.Rows.ToList();
        var includeNonPoColumns = rows.Any(row => row.IsNonPOItem);
        var activeRules = configuredRules
            .Where(rule =>
                rows.Any(row =>
                    !row.IsNonPOItem
                    && ReferenceEquals(FindMatchingRule(row, configuredRules), rule)
                )
            )
            .ToList();

        var includeOtherColumns =
            rows.Any(row => !row.IsNonPOItem && FindMatchingRule(row, configuredRules) is null)
            || (activeRules.Count == 0 && !includeNonPoColumns);

        var columns = new List<Model_ReportSummaryColumn>
        {
            new() { Header = "Date", Width = 120d },
        };

        foreach (var rule in activeRules)
        {
            var label = GetRuleLabel(rule);
            columns.Add(
                new Model_ReportSummaryColumn { Header = $"{label} (Qty/Lbs)", Width = 132d }
            );
            columns.Add(new Model_ReportSummaryColumn { Header = $"{label} Count", Width = 110d });
        }

        if (includeNonPoColumns)
        {
            columns.Add(
                new Model_ReportSummaryColumn { Header = "Non-PO (Qty/Lbs)", Width = 132d }
            );
            columns.Add(new Model_ReportSummaryColumn { Header = "Non-PO Count", Width = 110d });
        }

        if (includeOtherColumns)
        {
            columns.Add(new Model_ReportSummaryColumn { Header = "Other (Qty/Lbs)", Width = 132d });
            columns.Add(new Model_ReportSummaryColumn { Header = "Other Count", Width = 110d });
        }

        columns.Add(new Model_ReportSummaryColumn { Header = "Total Rows", Width = 120d });

        var summaryRows = rows.GroupBy(row => row.CreatedDate.Date)
            .OrderBy(group => group.Key)
            .Select(group =>
                CreateSummaryTableRow(
                    group.Key.ToString("M/d/yyyy", CultureInfo.InvariantCulture),
                    group.ToList(),
                    activeRules,
                    configuredRules,
                    includeNonPoColumns,
                    includeOtherColumns,
                    columns
                )
            )
            .ToList();

        summaryRows.Add(
            CreateSummaryTableRow(
                "GRAND TOTAL",
                rows,
                activeRules,
                configuredRules,
                includeNonPoColumns,
                includeOtherColumns,
                columns,
                isGrandTotal: true
            )
        );

        return new Model_ReportSummaryTable
        {
            ModuleName = section.ModuleName,
            Title =
                $"{section.ModuleName} Summary by Prefix for {section.Title.Replace(section.ModuleName + " Activity for ", string.Empty, StringComparison.OrdinalIgnoreCase)}",
            Columns = [.. columns],
            Rows = [.. summaryRows],
            TableWidth = columns.Sum(column => column.Width),
        };
    }

    private static Model_ReportSummaryTable BuildDunnageSummaryTable(Model_ReportSection section)
    {
        var categories = section
            .Rows.Select(row => NormalizeDunnageType(row.DunnageType))
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(label => label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return BuildCategorySummaryTable(
            section,
            categories,
            row => NormalizeDunnageType(row.DunnageType),
            amountHeaderSuffix: "Qty",
            titlePrefix: "Summary by Type"
        );
    }

    private static Model_ReportSummaryTable BuildVolvoSummaryTable(Model_ReportSection section)
    {
        var rows = section.Rows.ToList();
        var columns = new List<Model_ReportSummaryColumn>
        {
            new() { Header = "Date", Width = 150d },
            new() { Header = "Unique Part IDs", Width = 170d },
            new() { Header = "Total Skids", Width = 150d },
            new() { Header = "Total Rows", Width = 130d },
        };

        var summaryRows = rows.GroupBy(row => row.CreatedDate.Date)
            .OrderBy(group => group.Key)
            .Select(group =>
                CreateVolvoSummaryTableRow(
                    group.Key.ToString("M/d/yyyy", CultureInfo.InvariantCulture),
                    group.ToList(),
                    columns
                )
            )
            .ToList();

        summaryRows.Add(
            CreateVolvoSummaryTableRow("GRAND TOTAL", rows, columns, isGrandTotal: true)
        );

        return new Model_ReportSummaryTable
        {
            ModuleName = section.ModuleName,
            Title =
                $"{section.ModuleName} Summary for {section.Title.Replace(section.ModuleName + " Activity for ", string.Empty, StringComparison.OrdinalIgnoreCase)}",
            Columns = [.. columns],
            Rows = [.. summaryRows],
            TableWidth = columns.Sum(column => column.Width),
        };
    }

    private static Model_ReportSummaryTableRow CreateVolvoSummaryTableRow(
        string label,
        IReadOnlyCollection<Model_ReportRow> rows,
        IReadOnlyList<Model_ReportSummaryColumn> columns,
        bool isGrandTotal = false
    )
    {
        var uniquePartIds = rows.Select(row => row.PartNumber?.Trim())
            .Where(partNumber => !string.IsNullOrWhiteSpace(partNumber))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var totalSkids = rows.Sum(row => row.ReceivedSkidCount ?? 0);

        return new Model_ReportSummaryTableRow
        {
            Cells =
            [
                new Model_ReportSummaryTableCell { Value = label, Width = columns[0].Width },
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(uniquePartIds),
                    Width = columns[1].Width,
                },
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(totalSkids),
                    Width = columns[2].Width,
                },
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(rows.Count),
                    Width = columns[3].Width,
                },
            ],
            IsGrandTotal = isGrandTotal,
        };
    }

    private static Model_ReportSummaryTable BuildGenericSummaryTable(Model_ReportSection section)
    {
        return BuildCategorySummaryTable(
            section,
            [],
            _ => string.Empty,
            amountHeaderSuffix: "Qty/Lbs",
            titlePrefix: "Summary"
        );
    }

    private static Model_ReportSummaryTable BuildCategorySummaryTable(
        Model_ReportSection section,
        IReadOnlyList<string> categories,
        Func<Model_ReportRow, string> categorySelector,
        string amountHeaderSuffix,
        string titlePrefix
    )
    {
        var rows = section.Rows.ToList();
        var includeOtherColumns =
            rows.Any(row => string.IsNullOrWhiteSpace(categorySelector(row)))
            || categories.Count == 0;

        var columns = new List<Model_ReportSummaryColumn>
        {
            new() { Header = "Date", Width = 120d },
        };

        foreach (var category in categories)
        {
            columns.Add(
                new Model_ReportSummaryColumn
                {
                    Header = $"{category} ({amountHeaderSuffix})",
                    Width = 132d,
                }
            );
            columns.Add(
                new Model_ReportSummaryColumn { Header = $"{category} Count", Width = 110d }
            );
        }

        if (includeOtherColumns)
        {
            columns.Add(
                new Model_ReportSummaryColumn
                {
                    Header = $"Other ({amountHeaderSuffix})",
                    Width = 132d,
                }
            );
            columns.Add(new Model_ReportSummaryColumn { Header = "Other Count", Width = 110d });
        }

        columns.Add(new Model_ReportSummaryColumn { Header = "Total Rows", Width = 120d });

        var summaryRows = rows.GroupBy(row => row.CreatedDate.Date)
            .OrderBy(group => group.Key)
            .Select(group =>
                CreateCategorySummaryTableRow(
                    group.Key.ToString("M/d/yyyy", CultureInfo.InvariantCulture),
                    group.ToList(),
                    categories,
                    categorySelector,
                    includeOtherColumns,
                    columns
                )
            )
            .ToList();

        summaryRows.Add(
            CreateCategorySummaryTableRow(
                "GRAND TOTAL",
                rows,
                categories,
                categorySelector,
                includeOtherColumns,
                columns,
                isGrandTotal: true
            )
        );

        return new Model_ReportSummaryTable
        {
            ModuleName = section.ModuleName,
            Title =
                $"{section.ModuleName} {titlePrefix} for {section.Title.Replace(section.ModuleName + " Activity for ", string.Empty, StringComparison.OrdinalIgnoreCase)}",
            Columns = [.. columns],
            Rows = [.. summaryRows],
            TableWidth = columns.Sum(column => column.Width),
        };
    }

    private static Model_ReportSummaryTableRow CreateSummaryTableRow(
        string label,
        IReadOnlyCollection<Model_ReportRow> rows,
        IReadOnlyList<Model_PartNumberPrefixRule> activeRules,
        IReadOnlyList<Model_PartNumberPrefixRule> configuredRules,
        bool includeNonPoColumns,
        bool includeOtherColumns,
        IReadOnlyList<Model_ReportSummaryColumn> columns,
        bool isGrandTotal = false
    )
    {
        var cells = new List<Model_ReportSummaryTableCell>
        {
            new() { Value = label, Width = columns[0].Width },
        };

        var columnIndex = 1;
        foreach (var rule in activeRules)
        {
            var matchedRows = rows.Where(row =>
                    ReferenceEquals(FindMatchingRule(row, configuredRules), rule)
                )
                .ToList();
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatDecimal(matchedRows.Sum(GetSummaryValue)),
                    Width = columns[columnIndex++].Width,
                }
            );
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(matchedRows.Count),
                    Width = columns[columnIndex++].Width,
                }
            );
        }

        if (includeNonPoColumns)
        {
            var nonPoRows = rows.Where(row => row.IsNonPOItem).ToList();
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatDecimal(nonPoRows.Sum(GetSummaryValue)),
                    Width = columns[columnIndex++].Width,
                }
            );
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(nonPoRows.Count),
                    Width = columns[columnIndex++].Width,
                }
            );
        }

        if (includeOtherColumns)
        {
            var otherRows = rows.Where(row =>
                    !row.IsNonPOItem && FindMatchingRule(row, configuredRules) is null
                )
                .ToList();
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatDecimal(otherRows.Sum(GetSummaryValue)),
                    Width = columns[columnIndex++].Width,
                }
            );
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(otherRows.Count),
                    Width = columns[columnIndex++].Width,
                }
            );
        }

        cells.Add(
            new Model_ReportSummaryTableCell
            {
                Value = FormatInt(rows.Count),
                Width = columns[columnIndex].Width,
            }
        );

        return new Model_ReportSummaryTableRow { Cells = [.. cells], IsGrandTotal = isGrandTotal };
    }

    private static Model_ReportSummaryTableRow CreateCategorySummaryTableRow(
        string label,
        IReadOnlyCollection<Model_ReportRow> rows,
        IReadOnlyList<string> categories,
        Func<Model_ReportRow, string> categorySelector,
        bool includeOtherColumns,
        IReadOnlyList<Model_ReportSummaryColumn> columns,
        bool isGrandTotal = false
    )
    {
        var cells = new List<Model_ReportSummaryTableCell>
        {
            new() { Value = label, Width = columns[0].Width },
        };

        var columnIndex = 1;
        foreach (var category in categories)
        {
            var matchedRows = rows.Where(row =>
                    categorySelector(row).Equals(category, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatDecimal(matchedRows.Sum(GetSummaryValue)),
                    Width = columns[columnIndex++].Width,
                }
            );
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(matchedRows.Count),
                    Width = columns[columnIndex++].Width,
                }
            );
        }

        if (includeOtherColumns)
        {
            var otherRows = rows.Where(row => string.IsNullOrWhiteSpace(categorySelector(row)))
                .ToList();
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatDecimal(otherRows.Sum(GetSummaryValue)),
                    Width = columns[columnIndex++].Width,
                }
            );
            cells.Add(
                new Model_ReportSummaryTableCell
                {
                    Value = FormatInt(otherRows.Count),
                    Width = columns[columnIndex++].Width,
                }
            );
        }

        cells.Add(
            new Model_ReportSummaryTableCell
            {
                Value = FormatInt(rows.Count),
                Width = columns[columnIndex].Width,
            }
        );

        return new Model_ReportSummaryTableRow { Cells = [.. cells], IsGrandTotal = isGrandTotal };
    }

    private static Model_PartNumberPrefixRule? FindMatchingRule(
        Model_ReportRow row,
        IReadOnlyList<Model_PartNumberPrefixRule> rules
    )
    {
        if (string.IsNullOrWhiteSpace(row.PartNumber))
        {
            return null;
        }

        var partNumber = row.PartNumber.Trim();
        return rules.FirstOrDefault(rule =>
            partNumber.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static string GetRuleLabel(Model_PartNumberPrefixRule rule)
    {
        return !string.IsNullOrWhiteSpace(rule.Name)
            ? rule.Name.Trim()
            : rule.Prefix.Trim().ToUpperInvariant();
    }

    private static string NormalizeDunnageType(string? dunnageType)
    {
        return dunnageType?.Trim() ?? string.Empty;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatInt(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static decimal GetSummaryValue(Model_ReportRow row)
    {
        if (row.SourceModule.Equals("Receiving", StringComparison.OrdinalIgnoreCase))
        {
            return row.Quantity ?? row.WeightLbs ?? 0m;
        }

        if (row.SourceModule.Equals("Dunnage", StringComparison.OrdinalIgnoreCase))
        {
            return row.Quantity ?? 0m;
        }

        if (row.SourceModule.Equals("Volvo", StringComparison.OrdinalIgnoreCase))
        {
            return row.Quantity ?? 0m;
        }

        return row.Quantity ?? row.WeightLbs ?? 0m;
    }

    private static void AppendHeaderCell(StringBuilder html, string text, string? width)
    {
        var widthStyle = string.IsNullOrWhiteSpace(width) ? string.Empty : $" width: {width};";
        html.AppendLine(
            $"<th style='border: 1px solid #9fbad0; padding: 8px 10px; text-align: left;{widthStyle} white-space: nowrap;'>{text}</th>"
        );
    }

    private static (string AccentBackground, string AccentForeground) GetModuleAccent(
        string moduleName
    )
    {
        return moduleName switch
        {
            "Receiving" => (SummaryAccentBackground, SummaryAccentForeground),
            "Dunnage" => (DunnageAccentBackground, DunnageAccentForeground),
            "Volvo" => (DetailAccentBackground, DetailAccentForeground),
            _ => ("#4A5568", "#FFFFFF"),
        };
    }

    private static string GetDateRangeText(string title)
    {
        const string separator = " for ";
        var separatorIndex = title.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
        return separatorIndex >= 0 ? title[(separatorIndex + separator.Length)..] : title;
    }

    private void AppendSectionStart(
        StringBuilder html,
        string title,
        string cardBackground,
        string cardBorder,
        string accentBackground,
        string accentForeground
    )
    {
        var safeTitle = System.Net.WebUtility.HtmlEncode(title);
        html.AppendLine(
            $"<div style='margin: 0 0 16px 0; border: 1px solid {cardBorder}; border-radius: 4px; overflow: hidden; background-color: {cardBackground};'>"
        );
        html.AppendLine(
            $"<div style='background-color: {accentBackground}; color: {accentForeground}; padding: 10px 12px; font-weight: 700; display: block;'>"
                + safeTitle
                + "</div>"
        );
        html.AppendLine("<div style='padding: 10px 12px 12px 12px;'>");
    }

    private static void AppendSectionEnd(StringBuilder html)
    {
        html.AppendLine("</div>");
        html.AppendLine("</div>");
    }

    private static void AppendBodyCell(StringBuilder html, string? value, string alignment = "left")
    {
        var safeValue = System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        html.AppendLine(
            $"<td style='border: 1px solid #c8d6e5; padding: 6px 10px; text-align: {alignment}; vertical-align: top; word-wrap: break-word;'>{safeValue}</td>"
        );
    }
}
