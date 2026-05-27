using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Query_CustomerPullPackPrintContextHandlerTests
{
    private readonly Query_CustomerPullPackPrintContextHandler _handler = new();

    [Fact]
    public async Task Handle_ShouldKeepAllDemandLines_ForCurrentView()
    {
        var demandLines = CreateDemandLines();

        var result = await _handler.Handle(
            new Query_CustomerPullPackPrintContext(
                Enum_CustomerPullPackPrintMode.CurrentView,
                "VOLVO",
                "Volvo",
                "Current filters",
                demandLines,
                []
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Customer Pull n' Pack - Current View");
        result.Data.DemandLines.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterShortageDemandLines_ForShortageOnly()
    {
        var demandLines = CreateDemandLines();

        var result = await _handler.Handle(
            new Query_CustomerPullPackPrintContext(
                Enum_CustomerPullPackPrintMode.ShortageOnly,
                "VOLVO",
                "Volvo",
                "Shortages only",
                demandLines,
                []
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Customer Pull n' Pack - Shortage Only");
        result.Data.DemandLines.Should().ContainSingle();
        result.Data.DemandLines[0].ShortageFlag.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnWaitlistEntries_ForWaitlistOnly()
    {
        var waitlistEntries = CreateWaitlistEntries();

        var result = await _handler.Handle(
            new Query_CustomerPullPackPrintContext(
                Enum_CustomerPullPackPrintMode.WaitlistOnly,
                "VOLVO",
                "Volvo",
                "Queue filters",
                [],
                waitlistEntries
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Customer Pull n' Pack - Waitlist Only");
        result.Data.WaitlistEntries.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldBuildGroupedTotals_FromWaitlistEntries_ForPullList()
    {
        var waitlistEntries = CreateWaitlistEntries();

        var result = await _handler.Handle(
            new Query_CustomerPullPackPrintContext(
                Enum_CustomerPullPackPrintMode.PullList,
                "VOLVO",
                "Volvo",
                "Pull list",
                [],
                waitlistEntries
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Customer Pull n' Pack - Pull List");
        result.Data.GroupedSubPartTotals.Should().ContainSingle();
        result.Data.GroupedSubPartTotals[0].SubPartId.Should().Be("PART-100");
        result.Data.GroupedSubPartTotals[0].TotalQuantityNeeded.Should().Be(8);
        result
            .Data.GroupedSubPartTotals[0]
            .ChosenLocations.Should()
            .BeEquivalentTo(["A-01", "A-02"]);
    }

    [Fact]
    public async Task Handle_ShouldOnlyKeepSelectedDemandLines_ForSelectedContext()
    {
        var demandLines = CreateDemandLines();

        var result = await _handler.Handle(
            new Query_CustomerPullPackPrintContext(
                Enum_CustomerPullPackPrintMode.SelectedContext,
                "VOLVO",
                "Volvo",
                "Selected rows",
                demandLines,
                []
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Customer Pull n' Pack - Selected Context");
        result.Data.DemandLines.Should().ContainSingle();
        result.Data.DemandLines[0].SourceLineKey.Should().Be("LINE-1");
    }

    private static List<Model_CustomerPullPack_DemandLine> CreateDemandLines()
    {
        return
        [
            new Model_CustomerPullPack_DemandLine
            {
                SourceLineKey = "LINE-1",
                CustomerId = "VOLVO",
                CustomerName = "Volvo",
                ParentPartId = "PART-100",
                QuantityToPack = 5,
                ShortageFlag = true,
                IsSelected = true,
                LocationOptions =
                [
                    new Model_CustomerPullPack_LocationOption
                    {
                        LocationId = "A-01",
                        Selected = true,
                        OnHandQuantity = 3,
                    },
                ],
            },
            new Model_CustomerPullPack_DemandLine
            {
                SourceLineKey = "LINE-2",
                CustomerId = "VOLVO",
                CustomerName = "Volvo",
                ParentPartId = "PART-200",
                QuantityToPack = 2,
                ShortageFlag = false,
                IsSelected = false,
                LocationOptions =
                [
                    new Model_CustomerPullPack_LocationOption
                    {
                        LocationId = "B-01",
                        Selected = false,
                        OnHandQuantity = 7,
                    },
                ],
            },
        ];
    }

    private static List<Model_CustomerPullPack_WaitlistEntry> CreateWaitlistEntries()
    {
        return
        [
            new Model_CustomerPullPack_WaitlistEntry
            {
                WaitlistId = "WL-1",
                ParentPartId = "PART-100",
                RequestedQuantity = 5,
                SelectedLocations = ["A-01"],
            },
            new Model_CustomerPullPack_WaitlistEntry
            {
                WaitlistId = "WL-2",
                ParentPartId = "PART-100",
                RequestedQuantity = 3,
                SelectedLocations = ["A-02"],
            },
        ];
    }
}
