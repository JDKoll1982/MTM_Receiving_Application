using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.Services;

/// <summary>
/// Covers the folder handling behind the preview's Save PDF action: the folder is created when
/// missing, the PDF lands inside it, and an unreachable path is reported instead of throwing.
/// </summary>
public sealed class Service_ReportingOutputTests : IDisposable
{
    private readonly string _workingDirectory = Path.Combine(
        Path.GetTempPath(),
        $"mtm-reporting-output-tests-{Guid.NewGuid():N}"
    );

    public Service_ReportingOutputTests()
    {
        Directory.CreateDirectory(_workingDirectory);
    }

    [Fact]
    public async Task WriteReportPdfAsync_ShouldCreateTheFolder_ThenWriteThePdf()
    {
        // The destination deliberately does not exist yet, mirroring a fresh share path.
        var nestedFolder = Path.Combine(_workingDirectory, "RECEIVING", "EoD Reports");
        Directory.Exists(nestedFolder).Should().BeFalse();

        var service = new Service_ReportingOutput(
            new Service_ReportingPdf(new Mock<IService_LoggingUtility>().Object),
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.WriteReportPdfAsync(
            [CreateCard()],
            "End of Day Report for 11-24-2026",
            nestedFolder,
            "End of Day Report for 11-24-2026.pdf",
            new DateTime(2026, 11, 24, 16, 0, 0),
            "jkoll"
        );

        result.IsSuccess.Should().BeTrue(result.ErrorMessage);
        Directory.Exists(nestedFolder).Should().BeTrue();
        result.Data.Should().Be(Path.Combine(nestedFolder, "End of Day Report for 11-24-2026.pdf"));
        File.Exists(result.Data).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task WriteReportPdfAsync_ShouldFail_WhenNoFolderIsConfigured(string folderPath)
    {
        var service = CreateService();

        var result = await service.WriteReportPdfAsync(
            [CreateCard()],
            "End of Day Report",
            folderPath,
            "report.pdf",
            DateTime.Now,
            "jkoll"
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Settings");
    }

    [Fact]
    public async Task WriteReportPdfAsync_ShouldFail_WhenTheFileNameIsBlank()
    {
        var service = CreateService();

        var result = await service.WriteReportPdfAsync(
            [CreateCard()],
            "End of Day Report",
            _workingDirectory,
            " ",
            DateTime.Now,
            "jkoll"
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("file name");
    }

    [Fact]
    public async Task WriteReportPdfAsync_ShouldReportTheFolder_WhenThePathCannotBeReached()
    {
        // A file sitting where the folder should be makes CreateDirectory fail, which is the same
        // shape as an unreachable share: the operator must learn which path to check.
        var blockingFile = Path.Combine(_workingDirectory, "not-a-folder");
        await File.WriteAllTextAsync(blockingFile, "blocker");
        var unusableFolder = Path.Combine(blockingFile, "EoD Reports");

        var service = CreateService();

        var result = await service.WriteReportPdfAsync(
            [CreateCard()],
            "End of Day Report",
            unusableFolder,
            "report.pdf",
            DateTime.Now,
            "jkoll"
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(unusableFolder);
        result.ErrorMessage.Should().Contain("Settings");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RevealSavedFileAsync_ShouldFail_WhenThereIsNoFilePath(string filePath)
    {
        var service = CreateService();

        var result = await service.RevealSavedFileAsync(filePath);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no saved report file");
    }

    public void Dispose()
    {
        if (Directory.Exists(_workingDirectory))
        {
            Directory.Delete(_workingDirectory, recursive: true);
        }
    }

    private static Service_ReportingOutput CreateService()
    {
        var logger = new Mock<IService_LoggingUtility>().Object;
        return new Service_ReportingOutput(new Service_ReportingPdf(logger), logger);
    }

    private static Model_ReportingPreviewModuleCard CreateCard()
    {
        var card = new Model_ReportingPreviewModuleCard
        {
            ModuleName = "Receiving",
            CardTitle = "Receiving Report for 11/24/2026",
        };

        card.SummaryTable = new Model_ReportSummaryTable { ModuleName = "Receiving" };
        card.SummaryTable.Columns.Add(new Model_ReportSummaryColumn { Header = "PO", Width = 120 });

        var summaryRow = new Model_ReportSummaryTableRow();
        summaryRow.Cells.Add(new Model_ReportSummaryTableCell { Value = "PO-1001" });
        card.SummaryTable.Rows.Add(summaryRow);

        return card;
    }
}
