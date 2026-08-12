using System.Collections.ObjectModel;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.Models;

public sealed class Model_ReportingPreviewModuleCardTests
{
    [Fact]
    public void RefreshPreviewRows_RawRowsMode_PreservesOriginalTransactionRows()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 21), 2m, "PO-1002")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.RawRows;

        card.PreviewRows.Should().HaveCount(2);
        card.PreviewRows[0].Cells[0].Value.Should().Be("PART-A");
        card.PreviewRows[0].Cells[3].Value.Should().Be("1");
        card.DetailSectionTitle.Should().Be("Detailed Activity");
    }

    [Fact]
    public void RefreshPreviewRows_UniquePartNumbersEntireDateRange_GroupsRowsByPartNumber()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 21), 2m, "PO-1002"),
            CreateReportRow("PART-B", "LOT-1", new DateTime(2026, 3, 20), 3m, "PO-1003")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        card.PreviewRows.Should().HaveCount(2);
        card.PreviewRows[0].Cells[0].Value.Should().Be("PART-A");
        card.PreviewRows[0].Cells[1].Value.Should().Be("Multiple Lots");
        card.PreviewRows[0].Cells[2].Value.Should().Be("3/20/2026 - 3/21/2026");
        card.PreviewRows[0].Cells[3].Value.Should().Be("3");
        card.PreviewRows[0].Cells[4].Value.Should().Be("Multiple POs");
        card.DetailSectionTitle.Should()
            .Be("Combined Activity - Unique Part Numbers (Entire Date Range)");
    }

    [Fact]
    public void RefreshPreviewRows_CombineMode_DoesNotForceLoadsSkidsColumnIncluded()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 21), 2m, "PO-1002")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        var loadsOrSkidsColumn = card.AvailableColumns.Single(column =>
            column.Key == nameof(Model_ReportRow.DisplayLoadsOrSkids)
        );

        loadsOrSkidsColumn.IsIncluded.Should().BeFalse();
        loadsOrSkidsColumn.CanChangeInOptions.Should().BeTrue();
    }

    [Fact]
    public void RefreshPreviewRows_CombineMode_SetsLoadsSkidsValueToCombinedRowCount_WhenIncluded()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 21), 2m, "PO-1002"),
            CreateReportRow("PART-B", "LOT-1", new DateTime(2026, 3, 21), 4m, "PO-1003")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        var loadsOrSkidsColumn = card.AvailableColumns.Single(column =>
            column.Key == nameof(Model_ReportRow.DisplayLoadsOrSkids)
        );
        loadsOrSkidsColumn.IsIncluded = true;

        GetPreviewCellValue(card, 0, nameof(Model_ReportRow.DisplayLoadsOrSkids)).Should().Be("2");
        GetPreviewCellValue(card, 1, nameof(Model_ReportRow.DisplayLoadsOrSkids)).Should().Be("1");
    }

    [Fact]
    public void RefreshPreviewRows_SelectedSortColumnDescending_SortsRawRowsPerModuleTable()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-B", "LOT-2", new DateTime(2026, 3, 21), 5m, "PO-1002"),
            CreateReportRow("PART-C", "LOT-3", new DateTime(2026, 3, 22), 3m, "PO-1003")
        );

        card.SelectedSortOptionKey = nameof(Model_ReportRow.DisplayQuantity);
        card.IsDescendingSortDirection = true;

        GetPreviewCellValue(card, 0, nameof(Model_ReportRow.PartNumber)).Should().Be("PART-B");
        GetPreviewCellValue(card, 1, nameof(Model_ReportRow.PartNumber)).Should().Be("PART-C");
        GetPreviewCellValue(card, 2, nameof(Model_ReportRow.PartNumber)).Should().Be("PART-A");
    }

    [Fact]
    public void RefreshPreviewRows_SelectedSortColumnAscending_SortsGroupedRowsPerModuleTable()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-C", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-C", "LOT-2", new DateTime(2026, 3, 21), 2m, "PO-1002"),
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 21), 4m, "PO-1003")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;
        card.SelectedSortOptionKey = nameof(Model_ReportRow.PartNumber);
        card.IsAscendingSortDirection = true;

        GetPreviewCellValue(card, 0, nameof(Model_ReportRow.PartNumber)).Should().Be("PART-A");
        GetPreviewCellValue(card, 1, nameof(Model_ReportRow.PartNumber)).Should().Be("PART-C");
    }

    [Fact]
    public void InitializeColumns_ShouldExposeSourceOrderAndColumnSortOptions()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001")
        );

        card.SortOptions.Should().Contain(option => option.Key == "__source_order");
        card.SortOptions.Should()
            .Contain(option => option.Key == nameof(Model_ReportRow.PartNumber));
        card.IsSortDirectionEnabled.Should().BeFalse();
    }

    [Fact]
    public void RefreshPreviewRows_UniquePartNumbersAndLotNumbersEntireDateRange_GroupsRowsByPartAndLot()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 21), 2m, "PO-1002"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 21), 4m, "PO-1003")
        );

        card.RowDisplayMode =
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange;

        card.PreviewRows.Should().HaveCount(2);
        card.PreviewRows[0].Cells[0].Value.Should().Be("PART-A");
        card.PreviewRows[0].Cells[1].Value.Should().Be("LOT-1");
        card.PreviewRows[0].Cells[3].Value.Should().Be("3");
    }

    [Fact]
    public void RefreshPreviewRows_UniquePartNumbersPerDay_GroupsRowsByPartNumberAndDate()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 20), 2m, "PO-1002"),
            CreateReportRow("PART-A", "LOT-3", new DateTime(2026, 3, 21), 4m, "PO-1003")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay;

        card.PreviewRows.Should().HaveCount(2);
        card.PreviewRows[0].Cells[0].Value.Should().Be("PART-A");
        card.PreviewRows[0].Cells[2].Value.Should().Be("3/20/2026");
        card.PreviewRows[0].Cells[3].Value.Should().Be("3");
        card.PreviewRows[1].Cells[2].Value.Should().Be("3/21/2026");
        card.PreviewRows[1].Cells[3].Value.Should().Be("4");
    }

    [Fact]
    public void RefreshPreviewRows_UniquePartNumbersAndLotNumbersPerDay_GroupsRowsByPartLotAndDate()
    {
        var card = CreateReceivingPreviewCard(
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001"),
            CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 2m, "PO-1002"),
            CreateReportRow("PART-A", "LOT-2", new DateTime(2026, 3, 20), 4m, "PO-1003")
        );

        card.RowDisplayMode =
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersPerDay;

        card.PreviewRows.Should().HaveCount(2);
        card.PreviewRows[0].Cells[0].Value.Should().Be("PART-A");
        card.PreviewRows[0].Cells[1].Value.Should().Be("LOT-1");
        card.PreviewRows[0].Cells[2].Value.Should().Be("3/20/2026");
        card.PreviewRows[0].Cells[3].Value.Should().Be("3");
    }

    [Fact]
    public void RefreshPreviewRows_GroupedTextFieldWithDifferentValues_ShouldRenderExplicitPlaceholder()
    {
        var firstRow = CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001");
        firstRow.PONumber = "PO-1001";

        var secondRow = CreateReportRow(
            "PART-A",
            "LOT-2",
            new DateTime(2026, 3, 21),
            2m,
            "PO-1002"
        );
        secondRow.PONumber = "PO-1002";

        var card = CreateReceivingPreviewCard(firstRow, secondRow);
        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        GetPreviewCellValue(card, 0, nameof(Model_ReportRow.PONumber)).Should().Be("Multiple POs");
    }

    [Fact]
    public void RefreshPreviewRows_GroupedDateFieldWithDifferentValues_ShouldRenderDateRange()
    {
        var firstRow = CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001");
        firstRow.TransactionDate = new DateTime(2026, 3, 21);

        var secondRow = CreateReportRow(
            "PART-A",
            "LOT-2",
            new DateTime(2026, 3, 21),
            2m,
            "PO-1002"
        );
        secondRow.TransactionDate = new DateTime(2026, 3, 22);

        var card = CreateReceivingPreviewCard(firstRow, secondRow);
        card.InitializeColumns([
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.PartNumber),
                Header = "Part Number",
                Width = 170d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.DisplayTransactionDate),
                Header = "Transaction Date",
                Width = 140d,
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
        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        GetPreviewCellValue(card, 0, nameof(Model_ReportRow.DisplayTransactionDate))
            .Should()
            .Be("3/21/2026 - 3/22/2026");
    }

    [Fact]
    public void RefreshPreviewRows_GroupedReceiverValues_ShouldRenderExplicitReceiverPlaceholder()
    {
        var firstRow = CreateReportRow("PART-A", "LOT-1", new DateTime(2026, 3, 20), 1m, "PO-1001");
        firstRow.ReceiverNumber = "RCV-1";

        var secondRow = CreateReportRow(
            "PART-A",
            "LOT-2",
            new DateTime(2026, 3, 21),
            2m,
            "PO-1002"
        );
        secondRow.ReceiverNumber = "RCV-2";

        var card = CreateReceivingPreviewCard(firstRow, secondRow);
        card.InitializeColumns([
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
                Key = nameof(Model_ReportRow.DisplayQuantity),
                Header = "Quantity",
                Width = 110d,
                IsNumeric = true,
                IsIncluded = true,
            },
        ]);
        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        GetPreviewCellValue(card, 0, nameof(Model_ReportRow.ReceiverNumber))
            .Should()
            .Be("Multiple Receivers");
    }

    [Fact]
    public void RefreshPreviewRows_PartBasedModeWithoutPartNumbers_FallsBackToRawRows()
    {
        var card = CreatePreviewCard(
            "Dunnage",
            CreateReportRow(null, null, new DateTime(2026, 3, 20), 1m, "PO-1001", "Dunnage"),
            CreateReportRow(null, null, new DateTime(2026, 3, 20), 2m, "PO-1002", "Dunnage")
        );

        card.RowDisplayMode = Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;

        card.PreviewRows.Should().HaveCount(2);
        card.DetailSectionTitle.Should().Be("Detailed Activity");
        card.DetailSectionNote.Should().NotBeNullOrWhiteSpace();
    }

    private static Model_ReportingPreviewModuleCard CreateReceivingPreviewCard(
        params Model_ReportRow[] rows
    )
    {
        return CreatePreviewCard("Receiving", rows);
    }

    private static Model_ReportingPreviewModuleCard CreatePreviewCard(
        string moduleName,
        params Model_ReportRow[] rows
    )
    {
        var card = new Model_ReportingPreviewModuleCard
        {
            ModuleName = moduleName,
            DetailSection = new Model_ReportSection
            {
                ModuleName = moduleName,
                Rows = new ObservableCollection<Model_ReportRow>(rows),
            },
        };

        card.InitializeColumns(CreateColumns());
        return card;
    }

    private static List<Model_ReportingPreviewColumnOption> CreateColumns()
    {
        return
        [
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.PartNumber),
                Header = "Part Number",
                Width = 170d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.HeatLotNumber),
                Header = "Heat/Lot",
                Width = 140d,
                IsIncluded = true,
            },
            new Model_ReportingPreviewColumnOption
            {
                Key = nameof(Model_ReportRow.CreatedDate),
                Header = "Created Date",
                Width = 120d,
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
                Key = nameof(Model_ReportRow.PONumber),
                Header = "PO Number",
                Width = 140d,
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
        ];
    }

    private static string GetPreviewCellValue(
        Model_ReportingPreviewModuleCard card,
        int rowIndex,
        string columnKey
    )
    {
        var columnIndex = card
            .IncludedColumns.Select((column, index) => new { column.Key, Index = index })
            .Single(column => column.Key == columnKey)
            .Index;

        return card.PreviewRows[rowIndex].Cells[columnIndex].Value;
    }

    private static Model_ReportRow CreateReportRow(
        string? partNumber,
        string? lotNumber,
        DateTime createdDate,
        decimal quantity,
        string poNumber,
        string sourceModule = "Receiving"
    )
    {
        return new Model_ReportRow
        {
            PartNumber = partNumber,
            HeatLotNumber = lotNumber,
            CreatedDate = createdDate,
            Quantity = quantity,
            PONumber = poNumber,
            SourceModule = sourceModule,
        };
    }
}
