using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models;

public sealed class Model_ReceivingLoadTests
{
    [Fact]
    public void WeightPerPackageValueDisplay_ShouldUseCeilingAndUnitOfMeasure_WhenPackagesPerLoadChanges()
    {
        var load = new Model_ReceivingLoad { UnitOfMeasure = "ea", WeightQuantity = 10 };

        load.PackagesPerLoad = 3;

        load.WeightPerPackage.Should().Be(4);
        load.WeightPerPackageLabel.Should().Be("EA per Package");
        load.WeightPerPackageValueDisplay.Should().Be("4 EA");
    }

    [Fact]
    public void WeightPerPackageValueDisplay_ShouldUseUnitsFallback_WhenUnitOfMeasureMissing()
    {
        var load = new Model_ReceivingLoad
        {
            WeightQuantity = 9,
            PackagesPerLoad = 2,
            UnitOfMeasure = string.Empty,
        };

        load.WeightPerPackage.Should().Be(5);
        load.WeightPerPackageLabel.Should().Be("Units per Package");
        load.WeightPerPackageValueDisplay.Should().Be("5 Units");
    }

    [Fact]
    public void PartID_ShouldLeavePackagesPerLoadBlank_WhenPartIDIsAssigned()
    {
        var load = new Model_ReceivingLoad();

        load.PartID = "MMC0000056";

        load.PackagesPerLoad.Should().Be(0);
    }

    [Fact]
    public void PoNumber_WhenBlanketOrderSuffixIsEntered_ShouldNormalizeToCanonicalUppercaseFormat()
    {
        var load = new Model_ReceivingLoad();

        load.PoNumber = "po-65421b";

        load.PoNumber.Should().Be("PO-065421B");
    }

    [Fact]
    public void HeatLotNumber_WhenLowercaseTextIsEntered_ShouldSaveAsTrimmedUppercase()
    {
        var load = new Model_ReceivingLoad();

        load.HeatLotNumber = "  heat-12ab  ";

        load.HeatLotNumber.Should().Be("HEAT-12AB");
    }

    [Fact]
    public void HeatLotNumber_WhenPresetFillerIsEntered_ShouldPreserveConfiguredCasing()
    {
        var load = new Model_ReceivingLoad();

        load.HeatLotNumber = "  Refer to Vendor Tag  ";

        load.HeatLotNumber.Should().Be("Refer to Vendor Tag");
    }

    [Fact]
    public void ReceivedDateOffset_WhenDateIsChanged_ShouldKeepTimeOfDay()
    {
        var load = new Model_ReceivingLoad
        {
            ReceivedDate = new DateTime(2026, 10, 2, 13, 45, 12),
        };

        load.ReceivedDateOffset = new DateTimeOffset(new DateTime(2026, 10, 5));

        load.ReceivedDate.Should().Be(new DateTime(2026, 10, 5, 13, 45, 12));
    }

    [Fact]
    public void ReceivedDateOffset_ShouldExposeReceivedDateDate()
    {
        var load = new Model_ReceivingLoad
        {
            ReceivedDate = new DateTime(2026, 10, 2, 13, 45, 12),
        };

        load.ReceivedDateOffset.Should().NotBeNull();
        load.ReceivedDateOffset!.Value.Date.Should().Be(new DateTime(2026, 10, 2));
    }

    [Fact]
    public void ReceivedDateOffset_WhenCleared_ShouldKeepReceivedDate()
    {
        var original = new DateTime(2026, 10, 2, 13, 45, 12);
        var load = new Model_ReceivingLoad { ReceivedDate = original };

        load.ReceivedDateOffset = null;

        load.ReceivedDate.Should().Be(original);
    }

    [Fact]
    public void BatchKey_WhenRowsSharePartAndSaveSecond_ShouldMatch()
    {
        var savedAt = new DateTime(2026, 10, 23, 9, 14, 7);
        var firstSkid = new Model_ReceivingLoad { PartID = "MMC000659", CreatedAt = savedAt };
        var sixthSkid = new Model_ReceivingLoad
        {
            PartID = "MMC000659",
            CreatedAt = savedAt.AddMilliseconds(400),
        };

        sixthSkid.BatchKey.Should().Be(firstSkid.BatchKey);
    }

    [Fact]
    public void BatchKey_WhenSamePartIsSavedInAnotherSecond_ShouldDiffer()
    {
        var firstBatch = new Model_ReceivingLoad
        {
            PartID = "MMC000659",
            CreatedAt = new DateTime(2026, 10, 23, 9, 14, 7),
        };
        var secondBatch = new Model_ReceivingLoad
        {
            PartID = "MMC000659",
            CreatedAt = new DateTime(2026, 10, 23, 9, 14, 8),
        };

        secondBatch.BatchKey.Should().NotBe(firstBatch.BatchKey);
    }

    [Fact]
    public void BatchKey_WhenUnsaved_ShouldGroupByPartOnly()
    {
        var firstSkid = new Model_ReceivingLoad { PartID = "MMC000659" };
        var secondSkid = new Model_ReceivingLoad { PartID = "MMC000659" };
        var otherPart = new Model_ReceivingLoad { PartID = "MMC000767" };

        secondSkid.BatchKey.Should().Be(firstSkid.BatchKey);
        otherPart.BatchKey.Should().NotBe(firstSkid.BatchKey);
    }
}
