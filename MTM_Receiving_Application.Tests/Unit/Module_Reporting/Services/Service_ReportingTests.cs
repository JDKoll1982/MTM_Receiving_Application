using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Reporting.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.Services;

public class Service_ReportingTests
{
    private readonly Mock<IService_LoggingUtility> _logger = new();
    private readonly Mock<IService_ReceivingSettings> _receivingSettings = new();

    [Fact]
    public async Task BuildSummaryTablesAsync_ReceivingRowsWithNonPo_CreatesDedicatedNonPoColumns()
    {
        var sut = CreateSut([
            new Model_PartNumberPrefixRule
            {
                Name = "Coils",
                Prefix = "MMC",
                IsEnabled = true,
            },
        ]);

        var section = new Model_ReportSection
        {
            ModuleName = "Receiving",
            Title = "Receiving Activity for 3/20/2026 - 3/20/2026",
            Rows =
            [
                new Model_ReportRow
                {
                    SourceModule = "Receiving",
                    CreatedDate = new DateTime(2026, 3, 20),
                    PartNumber = "MMC0001000",
                    Quantity = 10,
                    IsNonPOItem = false,
                },
                new Model_ReportRow
                {
                    SourceModule = "Receiving",
                    CreatedDate = new DateTime(2026, 3, 20),
                    PartNumber = "FREEFORM-01",
                    Quantity = 5,
                    IsNonPOItem = true,
                },
            ],
        };

        var result = await sut.BuildSummaryTablesAsync([section]);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var table = result.Data!.Single();
        table
            .Columns.Select(column => column.Header)
            .Should()
            .Contain([
                "Coils (Qty/Lbs)",
                "Coils Count",
                "Non-PO (Qty/Lbs)",
                "Non-PO Count",
                "Total Rows",
            ]);

        var grandTotalRow = table.Rows.Last();
        grandTotalRow
            .Cells.Select(cell => cell.Value)
            .Should()
            .ContainInOrder("GRAND TOTAL", "10", "1", "5", "1", "2");
    }

    [Fact]
    public async Task BuildSummaryTablesAsync_ReceivingRowsWithNonPoAndUnmatchedPo_LeavesOtherBucketForPoRowsOnly()
    {
        var sut = CreateSut([
            new Model_PartNumberPrefixRule
            {
                Name = "Coils",
                Prefix = "MMC",
                IsEnabled = true,
            },
        ]);

        var section = new Model_ReportSection
        {
            ModuleName = "Receiving",
            Title = "Receiving Activity for 3/20/2026 - 3/20/2026",
            Rows =
            [
                new Model_ReportRow
                {
                    SourceModule = "Receiving",
                    CreatedDate = new DateTime(2026, 3, 20),
                    PartNumber = "MMC0001000",
                    Quantity = 10,
                    IsNonPOItem = false,
                },
                new Model_ReportRow
                {
                    SourceModule = "Receiving",
                    CreatedDate = new DateTime(2026, 3, 20),
                    PartNumber = "FREEFORM-01",
                    Quantity = 5,
                    IsNonPOItem = true,
                },
                new Model_ReportRow
                {
                    SourceModule = "Receiving",
                    CreatedDate = new DateTime(2026, 3, 20),
                    PartNumber = "ZZZ0001000",
                    Quantity = 7,
                    IsNonPOItem = false,
                },
            ],
        };

        var result = await sut.BuildSummaryTablesAsync([section]);

        result.IsSuccess.Should().BeTrue();
        var table = result.Data!.Single();
        table
            .Columns.Select(column => column.Header)
            .Should()
            .Contain(["Non-PO (Qty/Lbs)", "Non-PO Count", "Other (Qty/Lbs)", "Other Count"]);

        var grandTotalRow = table.Rows.Last();
        grandTotalRow
            .Cells.Select(cell => cell.Value)
            .Should()
            .ContainInOrder("GRAND TOTAL", "10", "1", "5", "1", "7", "1", "3");
    }

    private Service_Reporting CreateSut(IEnumerable<Model_PartNumberPrefixRule> rules)
    {
        _receivingSettings
            .Setup(service =>
                service.GetStringAsync(
                    ReceivingSettingsKeys.PartNumberPadding.RulesJson,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(JsonSerializer.Serialize(rules.ToList()));

        return new Service_Reporting(
            new Dao_Reporting("Server=unused;Database=unused;Uid=unused;Pwd=unused;"),
            _logger.Object,
            _receivingSettings.Object
        );
    }
}
