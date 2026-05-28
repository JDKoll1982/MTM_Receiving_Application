using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.Services;

public sealed class Service_ReportingFormattingTests
{
    [Fact]
    public async Task FormatForEmailAsync_GroupedPreviewRows_UsesGroupedDetailRowsInCopiedOutput()
    {
        var service = new Service_Reporting(
            new Dao_Reporting("Server=test;Database=test;"),
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ReceivingSettings>().Object
        );

        var previewModuleCard = new Model_ReportingPreviewModuleCard
        {
            ModuleName = "Receiving",
            CardTitle = "Receiving Report",
            DetailSection = new Model_ReportSection
            {
                ModuleName = "Receiving",
                Rows = new ObservableCollection<Model_ReportRow>([
                    new Model_ReportRow
                    {
                        PartNumber = "PART-A",
                        HeatLotNumber = "LOT-1",
                        Quantity = 1m,
                        CreatedDate = new(2026, 3, 20),
                        SourceModule = "Receiving",
                    },
                    new Model_ReportRow
                    {
                        PartNumber = "PART-A",
                        HeatLotNumber = "LOT-2",
                        Quantity = 2m,
                        CreatedDate = new(2026, 3, 21),
                        SourceModule = "Receiving",
                    },
                ]),
            },
            SummaryTable = new Model_ReportSummaryTable
            {
                ModuleName = "Receiving",
                Columns = [],
                Rows = [],
            },
        };

        previewModuleCard.InitializeColumns([
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.PartNumber),
                Header = "Part Number",
                Width = 170d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayQuantity),
                Header = "Quantity",
                Width = 110d,
                IsNumeric = true,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayLoadsOrSkids),
                Header = "Loads / Skids",
                Width = 130d,
                IsNumeric = true,
                IsIncluded = false,
            },
        ]);
        previewModuleCard.RowDisplayMode =
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        var result = await service.FormatForEmailAsync([previewModuleCard], "Preview Title");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlainText.Should().Contain("PART-A\t3\t2");
        result.Data.PlainText.Should().NotContain("PART-A\t1");
        result.Data.PlainText.Should().NotContain("PART-A\t2");
    }

    [Fact]
    public async Task FormatForEmailAsync_SelectedSortOption_UsesSortedPreviewRowsInCopiedOutput()
    {
        var service = new Service_Reporting(
            new Dao_Reporting("Server=test;Database=test;"),
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ReceivingSettings>().Object
        );

        var previewModuleCard = new Model_ReportingPreviewModuleCard
        {
            ModuleName = "Receiving",
            CardTitle = "Receiving Report",
            DetailSection = new Model_ReportSection
            {
                ModuleName = "Receiving",
                Rows = new ObservableCollection<Model_ReportRow>([
                    new Model_ReportRow
                    {
                        PartNumber = "PART-A",
                        Quantity = 1m,
                        CreatedDate = new(2026, 3, 20),
                        SourceModule = "Receiving",
                    },
                    new Model_ReportRow
                    {
                        PartNumber = "PART-C",
                        Quantity = 3m,
                        CreatedDate = new(2026, 3, 21),
                        SourceModule = "Receiving",
                    },
                    new Model_ReportRow
                    {
                        PartNumber = "PART-B",
                        Quantity = 2m,
                        CreatedDate = new(2026, 3, 22),
                        SourceModule = "Receiving",
                    },
                ]),
            },
            SummaryTable = new Model_ReportSummaryTable
            {
                ModuleName = "Receiving",
                Columns = [],
                Rows = [],
            },
        };

        previewModuleCard.InitializeColumns([
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.PartNumber),
                Header = "Part Number",
                Width = 170d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayQuantity),
                Header = "Quantity",
                Width = 110d,
                IsNumeric = true,
                IsIncluded = true,
            },
        ]);
        previewModuleCard.SelectedSortOptionKey = nameof(Model_ReportRow.PartNumber);
        previewModuleCard.IsDescendingSortDirection = true;

        var result = await service.FormatForEmailAsync([previewModuleCard], "Preview Title");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var partCIndex = result.Data!.PlainText.IndexOf(
            "PART-C\t3",
            System.StringComparison.Ordinal
        );
        var partBIndex = result.Data.PlainText.IndexOf(
            "PART-B\t2",
            System.StringComparison.Ordinal
        );
        var partAIndex = result.Data.PlainText.IndexOf(
            "PART-A\t1",
            System.StringComparison.Ordinal
        );

        partCIndex.Should().BeGreaterThan(-1);
        partBIndex.Should().BeGreaterThan(partCIndex);
        partAIndex.Should().BeGreaterThan(partBIndex);
    }

    [Fact]
    public async Task FormatForEmailAsync_DunnagePreview_DoesNotIncludeSummarySection()
    {
        var service = new Service_Reporting(
            new Dao_Reporting("Server=test;Database=test;"),
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ReceivingSettings>().Object
        );

        var previewModuleCard = new Model_ReportingPreviewModuleCard
        {
            ModuleName = "Dunnage",
            CardTitle = "Dunnage Report",
            DetailSection = new Model_ReportSection
            {
                ModuleName = "Dunnage",
                Rows = new ObservableCollection<Model_ReportRow>([
                    new Model_ReportRow
                    {
                        PartNumber = "DUN-A",
                        Quantity = 4m,
                        CreatedDate = new(2026, 3, 20),
                        SourceModule = "Dunnage",
                    },
                ]),
            },
            SummaryTable = new Model_ReportSummaryTable
            {
                ModuleName = "Dunnage",
                Columns =
                [
                    new Model_ReportSummaryColumn { Header = "Date", Width = 120d },
                    new Model_ReportSummaryColumn { Header = "Other (Qty)", Width = 132d },
                ],
                Rows =
                [
                    new Model_ReportSummaryTableRow
                    {
                        Cells =
                        [
                            new Model_ReportSummaryTableCell { Value = "3/20/2026", Width = 120d },
                            new Model_ReportSummaryTableCell { Value = "4", Width = 132d },
                        ],
                    },
                ],
            },
        };

        previewModuleCard.InitializeColumns([
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.PartNumber),
                Header = "Part Number",
                Width = 170d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayQuantity),
                Header = "Quantity",
                Width = 110d,
                IsNumeric = true,
                IsIncluded = true,
            },
        ]);

        var result = await service.FormatForEmailAsync([previewModuleCard], "Preview Title");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlainText.Should().NotContain("Summary");
        result.Data.PlainText.Should().Contain("Detailed Activity");
        result.Data.PlainText.Should().Contain("DUN-A\t4");
    }

    [Fact]
    public async Task FormatForEmailAsync_GroupedMixedValues_UsesDateRangesAndExplicitPlaceholdersInCopiedOutput()
    {
        var service = new Service_Reporting(
            new Dao_Reporting("Server=test;Database=test;"),
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ReceivingSettings>().Object
        );

        var previewModuleCard = new Model_ReportingPreviewModuleCard
        {
            ModuleName = "Receiving",
            CardTitle = "Receiving Report",
            DetailSection = new Model_ReportSection
            {
                ModuleName = "Receiving",
                Rows = new ObservableCollection<Model_ReportRow>([
                    new Model_ReportRow
                    {
                        PartNumber = "PART-A",
                        HeatLotNumber = "LOT-1",
                        ReceiverNumber = "RCV-1",
                        TransactionDate = new(2026, 3, 21),
                        Quantity = 1m,
                        CreatedDate = new(2026, 3, 20),
                        SourceModule = "Receiving",
                    },
                    new Model_ReportRow
                    {
                        PartNumber = "PART-A",
                        HeatLotNumber = "LOT-2",
                        ReceiverNumber = "RCV-2",
                        TransactionDate = new(2026, 3, 22),
                        Quantity = 2m,
                        CreatedDate = new(2026, 3, 21),
                        SourceModule = "Receiving",
                    },
                ]),
            },
            SummaryTable = new Model_ReportSummaryTable
            {
                ModuleName = "Receiving",
                Columns = [],
                Rows = [],
            },
        };

        previewModuleCard.InitializeColumns([
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.PartNumber),
                Header = "Part Number",
                Width = 170d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.ReceiverNumber),
                Header = "Receiver",
                Width = 140d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayTransactionDate),
                Header = "Transaction Date",
                Width = 160d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayQuantity),
                Header = "Quantity",
                Width = 110d,
                IsNumeric = true,
                IsIncluded = true,
            },
        ]);
        previewModuleCard.RowDisplayMode =
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        var result = await service.FormatForEmailAsync([previewModuleCard], "Preview Title");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result
            .Data!.PlainText.Should()
            .Contain("PART-A\tMultiple Receivers\t3/21/2026 - 3/22/2026\t3");
    }
}
