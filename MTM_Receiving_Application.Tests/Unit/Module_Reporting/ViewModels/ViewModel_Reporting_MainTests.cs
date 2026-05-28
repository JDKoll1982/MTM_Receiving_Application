using System.Globalization;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.ViewModels;

public sealed class ViewModel_Reporting_MainTests
{
    [Fact]
    public void SelectingYesterdayPreset_ShouldSetBothDatesToYesterday()
    {
        var viewModel = CreateViewModel();
        var today = DateTimeOffset.Now.Date;

        viewModel.SelectedDateRangePreset = "Yesterday";

        viewModel.StartDate.Should().HaveValue();
        viewModel.EndDate.Should().HaveValue();
        viewModel.StartDate!.Value.Date.Should().Be(today.AddDays(-1));
        viewModel.EndDate!.Value.Date.Should().Be(today.AddDays(-1));
    }

    [Fact]
    public void SelectingTodayPreset_ShouldSetBothDatesToToday()
    {
        var viewModel = CreateViewModel();
        var today = DateTimeOffset.Now.Date;

        viewModel.SelectedDateRangePreset = "Today";

        viewModel.StartDate.Should().HaveValue();
        viewModel.EndDate.Should().HaveValue();
        viewModel.StartDate!.Value.Date.Should().Be(today);
        viewModel.EndDate!.Value.Date.Should().Be(today);
    }

    [Fact]
    public void SelectingThisWeekPreset_ShouldStartAtCurrentCultureWeekBoundary()
    {
        var viewModel = CreateViewModel();
        var today = DateTimeOffset.Now.Date;
        var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var delta = ((int)today.DayOfWeek - (int)firstDayOfWeek + 7) % 7;

        viewModel.SelectedDateRangePreset = "This Week";

        viewModel.StartDate.Should().HaveValue();
        viewModel.EndDate.Should().HaveValue();
        viewModel.StartDate!.Value.Date.Should().Be(today.AddDays(-delta));
        viewModel.EndDate!.Value.Date.Should().Be(today);
    }

    [Fact]
    public void ChangingDateManually_ShouldResetPresetToCustom()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedDateRangePreset = "Today";
        viewModel.StartDate.Should().HaveValue();
        viewModel.StartDate = viewModel.StartDate!.Value.AddDays(-2);

        viewModel.SelectedDateRangePreset.Should().Be("Custom");
    }

    [Fact]
    public void GetDateRangeText_ShouldReturnSingleDate_WhenStartAndEndMatch()
    {
        var viewModel = CreateViewModel();
        var singleDate = new DateTimeOffset(2026, 4, 26, 17, 45, 0, TimeSpan.Zero);

        viewModel.StartDate = singleDate;
        viewModel.EndDate = singleDate;

        var method = typeof(ViewModel_Reporting_Main).GetMethod(
            "GetDateRangeText",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();
        var result = method!.Invoke(viewModel, null);

        result.Should().Be("4/26/2026");
    }

    [Fact]
    public void RecipientCommands_ShouldOnlyBeEnabled_WhenRecipientsContainText()
    {
        var viewModel = CreateViewModel();
        viewModel.IncludedPreviewModuleCards =
        [
            new Model_ReportingPreviewModuleCard { ModuleName = "Reporting" },
        ];

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeFalse();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeFalse();

        viewModel.ToRecipients = "to@example.com";
        viewModel.CcRecipients = "cc@example.com";

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeTrue();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeTrue();

        viewModel.ToRecipients = string.Empty;
        viewModel.CcRecipients = "   ";

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeFalse();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void RowDisplayModeSettings_ShouldBeCollapsedByDefault()
    {
        var viewModel = CreateViewModel();

        viewModel.IsRowDisplayModeSettingsExpanded.Should().BeFalse();
    }

    [Fact]
    public void SelectedRowDisplayModeOption_ShouldTrackUnderlyingEnumSelection()
    {
        var viewModel = CreateViewModel();
        var option = viewModel.RowDisplayModeOptions.Single(item =>
            item.Value == Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay
        );

        viewModel.SelectedRowDisplayModeOption = option;

        viewModel
            .SelectedRowDisplayMode.Should()
            .Be(Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay);
        viewModel.SelectedRowDisplayModeOption.Should().BeSameAs(option);
    }

    [Fact]
    public void SelectedRowDisplayModeExplanation_ShouldDescribeCurrentSelection()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedRowDisplayMode =
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange;

        viewModel
            .SelectedRowDisplayModeExplanation.Should()
            .Be("Combines rows by part number and lot across the full selected date range.");
    }

    [Fact]
    public void CreateDetailColumnOptions_ForReceiving_ShouldRemoveRequestedColumnsAndDefaultRemainingOn()
    {
        var columns = InvokeCreateDetailColumnOptions(CreateSection("Receiving"));

        columns
            .Select(column => column.Header)
            .Should()
            .NotContain([
                "PO / Line",
                "PO Line #",
                "Part / Dunnage",
                "Part Description",
                "Raw Quantity",
                "Weight Lbs",
                "Created At",
                "Transaction Date",
                "Created By",
                "User ID",
                "Vendor",
                "Load #",
                "Label #",
                "Units Per Skid",
                "Packages/Load",
                "Package Type",
                "Weight/Package",
                "PO Status",
                "PO Due Date",
                "Qty Ordered",
                "UOM",
                "Remaining Qty",
                "Non-PO",
                "Quality Hold Required",
                "Quality Hold Ack",
                "Part Skid Total",
                "Source Module",
                "ID",
            ]);
        columns.Should().OnlyContain(column => column.IsIncluded);
    }

    [Fact]
    public void CreateDetailColumnOptions_ForDunnage_ShouldRemoveRequestedColumnsAndDefaultRemainingOn()
    {
        var columns = InvokeCreateDetailColumnOptions(CreateSection("Dunnage"));

        columns
            .Select(column => column.Header)
            .Should()
            .NotContain([
                "Part / Dunnage",
                "Raw Quantity",
                "Created At",
                "Created By",
                "Non-PO",
                "Quality Hold Required",
                "Quality Hold Ack",
                "Source Module",
                "ID",
            ]);
        columns.Should().OnlyContain(column => column.IsIncluded);
    }

    [Fact]
    public void CreateDetailColumnOptions_ForVolvo_ShouldRemoveRequestedColumnsAndDefaultRemainingOn()
    {
        var columns = InvokeCreateDetailColumnOptions(CreateSection("Volvo"));

        columns
            .Select(column => column.Header)
            .Should()
            .NotContain([
                "PO / Line",
                "Part / Dunnage",
                "Raw Quantity",
                "Created At",
                "Shipment #",
                "Units Per Skid",
                "Non-PO",
                "Quality Hold Required",
                "Quality Hold Ack",
                "Qty/Skid",
                "Source Module",
                "ID",
            ]);
        columns.Should().OnlyContain(column => column.IsIncluded);
    }

    private static ViewModel_Reporting_Main CreateViewModel()
    {
        return new ViewModel_Reporting_Main(
            new Mock<IService_Reporting>().Object,
            new Mock<IService_ReportingClipboard>().Object,
            new Mock<IService_ReportingRecipientSettings>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static List<Model_ReportingPreviewColumnOption> InvokeCreateDetailColumnOptions(
        Model_ReportSection section
    )
    {
        var method = typeof(ViewModel_Reporting_Main).GetMethod(
            "CreateDetailColumnOptions",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();

        var result = method!.Invoke(null, [section]);
        result.Should().BeOfType<List<Model_ReportingPreviewColumnOption>>();
        return (List<Model_ReportingPreviewColumnOption>)result!;
    }

    private static Model_ReportSection CreateSection(string moduleName)
    {
        return new Model_ReportSection
        {
            ModuleName = moduleName,
            Rows =
            [
                new Model_ReportRow
                {
                    SourceModule = moduleName,
                    Id = "ROW-1",
                    PONumber = "123456",
                    POLineNumber = "10",
                    PartNumber = "PART-1",
                    PartDescription = "Part Description",
                    Quantity = 5m,
                    WeightLbs = 12m,
                    HeatLotNumber = "LOT-1",
                    CreatedDate = new DateTime(2026, 3, 20),
                    CreatedAt = new DateTime(2026, 3, 20, 8, 30, 0),
                    TransactionDate = new DateTime(2026, 3, 21),
                    EmployeeNumber = "1001",
                    CreatedByUsername = "tester",
                    UserId = "user-1",
                    DunnageType = "Rack",
                    SpecsCombined = "Spec-A",
                    ShipmentNumber = 22,
                    ReceiverNumber = "REC-99",
                    Status = "Open",
                    PartCount = 3,
                    Location = "A-01",
                    VendorName = "Vendor A",
                    Notes = "Notes",
                    LoadNumber = 7,
                    LabelNumber = 8,
                    PackagesPerLoad = 9,
                    PackageTypeName = "Box",
                    WeightPerPackage = 1.5m,
                    PoStatus = "Due",
                    PoDueDate = new DateTime(2026, 3, 25),
                    QtyOrdered = 11m,
                    UnitOfMeasure = "EA",
                    RemainingQuantity = 4,
                    IsNonPOItem = true,
                    IsQualityHoldRequired = true,
                    IsQualityHoldAcknowledged = true,
                    QualityHoldRestrictionType = "Restricted",
                    PartSkidTotal = 6,
                    CoilsOnSkid = 2,
                    QuantityPerSkid = 10,
                    ReceivedSkidCount = 1,
                },
            ],
        };
    }
}
