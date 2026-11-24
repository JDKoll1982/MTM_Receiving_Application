using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Helpers;
using MTM_Receiving_Application.Module_Reporting.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfColor = QuestPDF.Infrastructure.Color;

namespace MTM_Receiving_Application.Module_Reporting.Services;

/// <summary>
/// Renders the included preview module cards (summary table plus detail rows) into a PDF copy of
/// the end-of-day report.
/// </summary>
public class Service_ReportingPdf : IService_ReportingPdf
{
    private const string CardBorder = "#C8D6E5";
    private const string HeaderText = "#1F2937";
    private const string MutedText = "#5B6B7C";

    static Service_ReportingPdf()
    {
        // QuestPDF requires an explicit license selection; MTM qualifies for the community
        // license (under $1M USD annual revenue).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly IService_LoggingUtility _logger;

    public Service_ReportingPdf(IService_LoggingUtility logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Model_Dao_Result<string>> CreateReportPdfAsync(
        IReadOnlyList<Model_ReportingPreviewModuleCard> previewModuleCards,
        string documentTitle,
        DateTime generatedAt,
        string generatedBy,
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        if (previewModuleCards is null)
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<string>("Report data cannot be null.")
            );
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<string>("A destination file path is required.")
            );
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Document
                .Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(18);
                        page.DefaultTextStyle(style => style.FontSize(8));

                        page.Header().Element(header => ComposeHeader(header, documentTitle));
                        page.Content()
                            .Element(content => ComposeContent(content, previewModuleCards));
                        page.Footer()
                            .Element(footer => ComposeFooter(footer, generatedAt, generatedBy));
                    });
                })
                .GeneratePdf(filePath);

            _logger.LogInfo(
                $"Report PDF created: {filePath} ({previewModuleCards.Count} module sections)"
            );

            return Task.FromResult(Model_Dao_Result_Factory.Success(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error creating report PDF: {ex.Message}",
                ex,
                nameof(Service_ReportingPdf)
            );

            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<string>(
                    $"Failed to create the report PDF: {ex.Message}",
                    ex
                )
            );
        }
    }

    private static void ComposeHeader(IContainer container, string documentTitle)
    {
        container
            .PaddingBottom(6)
            .BorderBottom(1)
            .BorderColor(CardBorder)
            .Text(string.IsNullOrWhiteSpace(documentTitle) ? "End of Day Report" : documentTitle)
            .FontSize(14)
            .SemiBold()
            .FontColor(HeaderText);
    }

    private static void ComposeContent(
        IContainer container,
        IReadOnlyList<Model_ReportingPreviewModuleCard> previewModuleCards
    )
    {
        container.Column(column =>
        {
            column.Spacing(10);

            if (previewModuleCards.Count == 0)
            {
                column.Item().Text("No report data to display.").FontColor(MutedText);
                return;
            }

            foreach (var previewModuleCard in previewModuleCards)
            {
                column.Item().Element(item => ComposeModuleSection(item, previewModuleCard));
            }
        });
    }

    private static void ComposeModuleSection(
        IContainer container,
        Model_ReportingPreviewModuleCard previewModuleCard
    )
    {
        var (accentBackground, accentForeground) = Helper_ReportingModuleAccent.GetAccent(
            previewModuleCard.ModuleName
        );

        container
            .Border(1)
            .BorderColor(CardBorder)
            .Column(section =>
            {
                section
                    .Item()
                    .Background(PdfColor.FromHex(accentBackground))
                    .PaddingVertical(4)
                    .PaddingHorizontal(6)
                    .Text(BuildSectionTitle(previewModuleCard))
                    .FontSize(10)
                    .SemiBold()
                    .FontColor(PdfColor.FromHex(accentForeground));

                section
                    .Item()
                    .Padding(6)
                    .Column(body =>
                    {
                        body.Spacing(6);

                        if (
                            previewModuleCard.ShowsSummaryTable
                            && previewModuleCard.SummaryTable.Rows.Count > 0
                        )
                        {
                            body.Item().Text("Summary").SemiBold().FontColor(HeaderText);
                            body.Item()
                                .Element(item =>
                                    ComposeSummaryTable(
                                        item,
                                        previewModuleCard.SummaryTable,
                                        accentBackground,
                                        accentForeground
                                    )
                                );
                        }

                        body.Item().Text("Detailed Activity").SemiBold().FontColor(HeaderText);

                        if (!string.IsNullOrWhiteSpace(previewModuleCard.DetailSection.Description))
                        {
                            body.Item()
                                .Text(previewModuleCard.DetailSection.Description)
                                .FontSize(7)
                                .FontColor(MutedText);
                        }

                        body.Item().Element(item => ComposeDetailTable(item, previewModuleCard));
                    });
            });
    }

    private static void ComposeSummaryTable(
        IContainer container,
        Model_ReportSummaryTable summaryTable,
        string accentBackground,
        string accentForeground
    )
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var column in summaryTable.Columns)
                {
                    columns.RelativeColumn((float)Math.Max(column.Width, 60d));
                }
            });

            table.Header(header =>
            {
                foreach (var column in summaryTable.Columns)
                {
                    header
                        .Cell()
                        .Background(PdfColor.FromHex(accentBackground))
                        .Padding(3)
                        .Text(column.Header)
                        .SemiBold()
                        .FontColor(PdfColor.FromHex(accentForeground));
                }
            });

            foreach (var row in summaryTable.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    table
                        .Cell()
                        .BorderBottom(1)
                        .BorderColor(CardBorder)
                        .Padding(3)
                        .Text(cell.Value);
                }
            }
        });
    }

    private static void ComposeDetailTable(
        IContainer container,
        Model_ReportingPreviewModuleCard previewModuleCard
    )
    {
        var includedColumns = previewModuleCard.IncludedColumns.ToList();
        if (includedColumns.Count == 0)
        {
            container.Text("No detail columns selected for this module.").FontColor(MutedText);
            return;
        }

        var (accentBackground, accentForeground) = Helper_ReportingModuleAccent.GetAccent(
            previewModuleCard.ModuleName
        );

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var includedColumn in includedColumns)
                {
                    columns.RelativeColumn((float)Math.Max(includedColumn.Width, 50d));
                }
            });

            table.Header(header =>
            {
                foreach (var includedColumn in includedColumns)
                {
                    header
                        .Cell()
                        .Background(PdfColor.FromHex(accentBackground))
                        .Padding(3)
                        .Text(includedColumn.Header)
                        .SemiBold()
                        .FontColor(PdfColor.FromHex(accentForeground));
                }
            });

            foreach (var previewRow in previewModuleCard.PreviewRows)
            {
                for (var index = 0; index < includedColumns.Count; index++)
                {
                    var value =
                        index < previewRow.Cells.Count
                            ? previewRow.Cells[index].Value
                            : string.Empty;

                    table.Cell().BorderBottom(1).BorderColor(CardBorder).Padding(3).Text(value);
                }
            }
        });
    }

    private static void ComposeFooter(
        IContainer container,
        DateTime generatedAt,
        string generatedBy
    )
    {
        var timestamp = generatedAt.ToString("M/d/yyyy h:mm tt", CultureInfo.InvariantCulture);
        var user = string.IsNullOrWhiteSpace(generatedBy) ? Environment.UserName : generatedBy;

        container
            .PaddingTop(4)
            .BorderTop(1)
            .BorderColor(CardBorder)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text($"Generated {timestamp} by {user}")
                    .FontSize(7)
                    .FontColor(MutedText);

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(style => style.FontSize(7).FontColor(MutedText));
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
    }

    private static string BuildSectionTitle(Model_ReportingPreviewModuleCard previewModuleCard)
    {
        var moduleName = string.IsNullOrWhiteSpace(previewModuleCard.ModuleName)
            ? "Report"
            : previewModuleCard.ModuleName;

        return string.IsNullOrWhiteSpace(previewModuleCard.CardTitle)
            ? $"{moduleName} Report"
            : previewModuleCard.CardTitle;
    }
}
