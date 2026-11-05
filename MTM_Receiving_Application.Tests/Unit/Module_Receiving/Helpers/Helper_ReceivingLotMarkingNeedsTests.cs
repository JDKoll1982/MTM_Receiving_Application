using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers the reminder that names the labels whose part gets the lot shape treatment. The same list drives
/// the Review page and the finish page, so a part wrongly matched either sends an operator hunting for a
/// tag that needs nothing or leaves a tag without its symbol.
/// </summary>
public sealed class Helper_ReceivingLotMarkingNeedsTests
{
    private static Model_ReceivingLoad Load(
        string partId,
        string heatLot,
        int loadNumber,
        string shape = "",
        string color = ""
    ) =>
        new()
        {
            PartID = partId,
            HeatLotNumber = heatLot,
            LoadNumber = loadNumber,
            HeatShapeCode = shape,
            HeatColorCode = color,
        };

    [Fact]
    public void Build_ShouldNameOnlyTheLoadsWhosePartIsOnTheList()
    {
        var loads = new List<Model_ReceivingLoad>
        {
            Load("MMC0000848", "344934", 1),
            Load("OTHER-PART", "999999", 2),
        };

        var needs = Helper_ReceivingLotMarkingNeeds.Build(loads, ["MMC0000848"]);

        needs.Should().HaveCount(1);
        needs[0].PartId.Should().Be("MMC0000848");
        needs[0].HeatLotNumber.Should().Be("344934");
        needs[0].LabelNumber.Should().Be(1);
    }

    [Fact]
    public void Build_ShouldMatchThePartWhateverTheCaseOrSpacing()
    {
        var loads = new List<Model_ReceivingLoad> { Load(" mmc0000848 ", "L1", 1) };

        Helper_ReceivingLotMarkingNeeds
            .Build(loads, ["MMC0000848"])
            .Should()
            .ContainSingle("the enabled list and the receipt are keyed on different screens");
    }

    [Fact]
    public void Build_ShouldCarryTheMarking_WhenItHasAlreadyBeenResolved()
    {
        var loads = new List<Model_ReceivingLoad> { Load("MMC0000848", "L1", 1, "⭐", "BLUE") };

        Helper_ReceivingLotMarkingNeeds
            .Build(loads, ["MMC0000848"])
            .Single()
            .MarkingText.Should()
            .Be("⭐ BLUE");
    }

    [Fact]
    public void Build_ShouldLeaveTheMarkingEmpty_UntilTheReceiptIsSaved()
    {
        var loads = new List<Model_ReceivingLoad> { Load("MMC0000848", "L1", 1) };

        Helper_ReceivingLotMarkingNeeds
            .Build(loads, ["MMC0000848"])
            .Single()
            .MarkingText.Should()
            .BeEmpty("a marking is resolved when the receipt is saved");
    }

    [Fact]
    public void Build_ShouldReturnNothing_WhenTheEnabledListIsEmpty()
    {
        Helper_ReceivingLotMarkingNeeds
            .Build([Load("MMC0000848", "L1", 1)], [])
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Build_ShouldSkipLoadsWithNoPartNumber()
    {
        Helper_ReceivingLotMarkingNeeds
            .Build([Load("   ", "L1", 1)], ["MMC0000848"])
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Build_ShouldSurviveMissingInputs()
    {
        Helper_ReceivingLotMarkingNeeds.Build(null, ["MMC0000848"]).Should().BeEmpty();
        Helper_ReceivingLotMarkingNeeds.Build([Load("MMC0000848", "L1", 1)], null).Should().BeEmpty();
    }

    [Fact]
    public void Build_ShouldListEveryLoad_WhenSeveralShareTheSamePart()
    {
        var loads = new List<Model_ReceivingLoad>
        {
            Load("MMC0000848", "L1", 1),
            Load("MMC0000848", "L1", 2),
        };

        Helper_ReceivingLotMarkingNeeds
            .Build(loads, ["MMC0000848"])
            .Select(need => need.LabelNumber)
            .Should()
            .BeEquivalentTo([1, 2]);
    }

    [Fact]
    public void Summarize_ShouldNameOnePartAndItsLots_WhenSeveralLabelsShareThePart()
    {
        var needs = new List<Model_ReceivingLotMarkingNeed>
        {
            Need("MMC0000848", "XRC123"),
            Need("MMC0000848", "XRC321"),
            Need("MMC0000848", "XRC123"),
        };

        Helper_ReceivingLotMarkingNeeds
            .Summarize(needs)
            .Should()
            .Be("MMC0000848 · lots XRC123, XRC321");
    }

    [Fact]
    public void Summarize_ShouldSayLotInTheSingular_WhenThePartHasOneLot()
    {
        Helper_ReceivingLotMarkingNeeds
            .Summarize([Need("MMC0000848", "XRC123")])
            .Should()
            .Be("MMC0000848 · lot XRC123");
    }

    [Fact]
    public void Summarize_ShouldNameThePartAlone_WhenTheLoadCarriedNoLot()
    {
        Helper_ReceivingLotMarkingNeeds.Summarize([Need("MMC0000848", "")]).Should().Be("MMC0000848");
    }

    [Fact]
    public void Summarize_ShouldListEachPartOnceInTheOrderTheyAppear()
    {
        var needs = new List<Model_ReceivingLotMarkingNeed>
        {
            Need("TEST-B", "L2"),
            Need("TEST-A", "L1"),
            Need("TEST-B", "L3"),
        };

        Helper_ReceivingLotMarkingNeeds
            .Summarize(needs)
            .Should()
            .Be("TEST-B · lots L2, L3; TEST-A · lot L1");
    }

    [Fact]
    public void Summarize_ShouldIgnoreCase_WhenDecidingWhetherTwoLotsAreTheSame()
    {
        var needs = new List<Model_ReceivingLotMarkingNeed>
        {
            Need("MMC0000848", "xrc123"),
            Need("MMC0000848", "XRC123"),
        };

        Helper_ReceivingLotMarkingNeeds
            .Summarize(needs)
            .Should()
            .Be("MMC0000848 · lot xrc123");
    }

    [Fact]
    public void Summarize_ShouldReturnNothing_WhenThereIsNothingToName()
    {
        Helper_ReceivingLotMarkingNeeds.Summarize(null).Should().BeEmpty();
        Helper_ReceivingLotMarkingNeeds.Summarize([]).Should().BeEmpty();
    }

    private static Model_ReceivingLotMarkingNeed Need(string partId, string heatLot) =>
        new(partId, heatLot, 1, string.Empty);
}
