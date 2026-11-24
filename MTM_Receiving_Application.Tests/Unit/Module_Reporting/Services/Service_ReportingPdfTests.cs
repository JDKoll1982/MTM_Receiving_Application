using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.Services;

public sealed class Service_ReportingPdfTests : IDisposable
{
    private readonly string _workingDirectory = Path.Combine(
        Path.GetTempPath(),
        $"mtm-reporting-pdf-tests-{Guid.NewGuid():N}"
    );

    public Service_ReportingPdfTests()
    {
        Directory.CreateDirectory(_workingDirectory);
    }

    [Fact]
    public async Task CreateReportPdfAsync_ShouldWritePdfFile_WhenCardsHaveSummaryAndDetailRows()
    {
        var service = new Service_ReportingPdf(new Mock<IService_LoggingUtility>().Object);
        var filePath = Path.Combine(_workingDirectory, "report.pdf");

        var result = await service.CreateReportPdfAsync(
            [CreateReceivingCard()],
            "End of Day Report for 11/24/2026",
            new DateTime(2026, 11, 24, 8, 30, 0),
            "jkoll",
            filePath
        );

        result.IsSuccess.Should().BeTrue(result.ErrorMessage);
        result.Data.Should().Be(filePath);
        File.Exists(filePath).Should().BeTrue();
        new FileInfo(filePath).Length.Should().BeGreaterThan(1000);
        Encoding.ASCII.GetString(File.ReadAllBytes(filePath)[..4]).Should().Be("%PDF");
    }

    [Fact]
    public async Task CreateReportPdfAsync_ShouldFail_WhenNoDestinationPathIsGiven()
    {
        var service = new Service_ReportingPdf(new Mock<IService_LoggingUtility>().Object);

        var result = await service.CreateReportPdfAsync(
            [CreateReceivingCard()],
            "End of Day Report",
            DateTime.Now,
            "jkoll",
            "   "
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("destination file path");
    }

    public void Dispose()
    {
        if (Directory.Exists(_workingDirectory))
        {
            Directory.Delete(_workingDirectory, recursive: true);
        }
    }

    private static Model_ReportingPreviewModuleCard CreateReceivingCard()
    {
        var card = new Model_ReportingPreviewModuleCard
        {
            ModuleName = "Receiving",
            CardTitle = "Receiving Report for 11/24/2026",
        };

        card.SummaryTable = new Model_ReportSummaryTable { ModuleName = "Receiving" };
        card.SummaryTable.Columns.Add(new Model_ReportSummaryColumn { Header = "PO", Width = 120 });
        card.SummaryTable.Columns.Add(new Model_ReportSummaryColumn { Header = "Qty", Width = 80 });

        var summaryRow = new Model_ReportSummaryTableRow();
        summaryRow.Cells.Add(new Model_ReportSummaryTableCell { Value = "PO-1001" });
        summaryRow.Cells.Add(new Model_ReportSummaryTableCell { Value = "40" });
        card.SummaryTable.Rows.Add(summaryRow);

        card.DetailSection = new Model_ReportSection
        {
            ModuleName = "Receiving",
            Title = "Receiving Activity for 11/24/2026",
            Description = "Rows received on the selected day",
        };

        card.IncludedColumns.Add(
            new Model_ReportingPreviewColumnOption
            {
                Key = "po",
                Header = "PO Number",
                Width = 120,
            }
        );
        card.IncludedColumns.Add(
            new Model_ReportingPreviewColumnOption
            {
                Key = "qty",
                Header = "Qty",
                Width = 80,
                IsNumeric = true,
            }
        );

        var detailRow = new Model_ReportingPreviewRow();
        detailRow.Cells.Add(new Model_ReportingPreviewCell { Value = "PO-1001" });
        detailRow.Cells.Add(new Model_ReportingPreviewCell { Value = "40", IsNumeric = true });
        card.PreviewRows.Add(detailRow);

        return card;
    }
}
