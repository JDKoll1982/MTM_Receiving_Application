using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

/// <summary>
/// Covers the two things a receipt depends on: one lot keeps one marking, and a second supplier
/// using the same heat number is a different lot with a marking of its own.
/// </summary>
/// <remarks>
/// Override and lock behaviour belong to the tool's own tests, not here.
/// </remarks>
public sealed class Service_Tool_LotMarkingTests
{
    private const string TestConnectionString = "TEST-connection";

    [Fact]
    public async Task ResolveMarkingAsync_ShouldReuseTheRecordedMarking_WhenTheSameHeatAndSupplierResolveAgain()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        var first = await service.ResolveMarkingAsync(
            "TEST-HEAT-1001",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var second = await service.ResolveMarkingAsync(
            "TEST-HEAT-1001",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        first.IsSuccess.Should().BeTrue();
        first.Data.Should().NotBeNull();
        second.IsSuccess.Should().BeTrue();
        second.Data.Should().NotBeNull();

        second.Data!.Id.Should().Be(first.Data!.Id);
        second.Data.ShapeCode.Should().Be(first.Data.ShapeCode);
        second.Data.ColorCode.Should().Be(first.Data.ColorCode);
        ledger.RecordedMarkings.Should().ContainSingle();
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldReuseTheRecordedMarking_WhenTheHeatNumberIsSpelledDifferently()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        var first = await service.ResolveMarkingAsync(
            "TEST-HEAT-1001",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var respelled = await service.ResolveMarkingAsync(
            " test heat 1001 ",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        respelled.Data.Should().NotBeNull();
        respelled.Data!.ShapeCode.Should().Be(first.Data!.ShapeCode);
        respelled.Data.ColorCode.Should().Be(first.Data.ColorCode);
        ledger.RecordedMarkings.Should().ContainSingle();
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldRecordASecondMarking_WhenASecondSupplierUsesTheSameHeatNumber()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        var firstSupplier = await service.ResolveMarkingAsync(
            "TEST-HEAT-2002",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var secondSupplier = await service.ResolveMarkingAsync(
            "TEST-HEAT-2002",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-B",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        ledger.RecordedMarkings.Should().HaveCount(2);
        ledger
            .RecordedMarkings.Select(marking => marking.SupplierId)
            .Should()
            .BeEquivalentTo("TEST-SUP-A", "TEST-SUP-B");

        secondSupplier.Data.Should().NotBeNull();
        secondSupplier.Data!.Id.Should().NotBe(firstSupplier.Data!.Id);

        var sameMarking =
            secondSupplier.Data.ShapeCode == firstSupplier.Data.ShapeCode
            && secondSupplier.Data.ColorCode == firstSupplier.Data.ColorCode;
        sameMarking
            .Should()
            .BeFalse("a second supplier is a different lot and must not share its marking");
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldKeepTheWholeSupplierName_WhenTwoVendorsShareAnIdLengthPrefix()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        // The supplier group is the vendor name whenever the Infor Visual lookup is not an
        // unambiguous single match, and vendor names are far longer than a supplier id. It was
        // sized like an id once, which MySQL refused with error 1406 before any marking was
        // written. Shortening the name to make it fit is not a fix: these two vendors share their
        // first fifteen characters, so a shortened key would merge them into one lot.
        const string firstSupplier = "TEST-PARKER HANNIFIN HYDRAULICS";
        const string secondSupplier = "TEST-PARKER HANNIFIN PNEUMATICS";

        var first = await service.ResolveMarkingAsync(
            "TEST-HEAT-4004",
            FakeLotMarkingLedger.EnabledPartId,
            firstSupplier,
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var second = await service.ResolveMarkingAsync(
            "TEST-HEAT-4004",
            FakeLotMarkingLedger.EnabledPartId,
            secondSupplier,
            Enum_LotMarkingCollisionPolicy.Probe
        );

        ledger.RecordedMarkings.Should().HaveCount(2);
        ledger
            .RecordedMarkings.Select(marking => marking.SupplierId)
            .Should()
            .BeEquivalentTo(new[] { firstSupplier, secondSupplier });

        second.Data!.Id.Should().NotBe(first.Data!.Id);
    }

    [Fact]
    public async Task OverrideMarkingAsync_ShouldRecordTheReasonAndTheActingUser_WhenTheMarkingIsChangedByHand()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);
        var resolved = await service.ResolveMarkingAsync(
            "TEST-HEAT-3003",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        var result = await service.OverrideMarkingAsync(
            resolved.Data!.Id,
            "⭐",
            "RED",
            " TEST-wrong shape on the floor ",
            "TEST-USER-1"
        );

        result.IsSuccess.Should().BeTrue();
        ledger.OverrideCallCount.Should().Be(1);
        ledger.LastOverrideReason.Should().Be("TEST-wrong shape on the floor");
        ledger.LastOverrideUser.Should().Be("TEST-USER-1");
        ledger.LastOverrideShape.Should().Be("⭐");
        ledger.LastOverrideColor.Should().Be("RED");
        ledger.LastOverrideSource.Should().Be(nameof(Enum_LotMarkingSource.Manual));
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldReturnTheLockedMarkingUntouched_WhenTheLotIsLocked()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);
        var created = await service.ResolveMarkingAsync(
            "TEST-HEAT-4004",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        created.Data!.IsLocked = true;
        var shape = created.Data.ShapeCode;
        var color = created.Data.ColorCode;

        var again = await service.ResolveMarkingAsync(
            "TEST-HEAT-4004",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        again.Data!.Id.Should().Be(created.Data.Id);
        again.Data.ShapeCode.Should().Be(shape);
        again.Data.ColorCode.Should().Be(color);
        again.Data.IsLocked.Should().BeTrue();
        ledger.InsertCallCount.Should().Be(1, "a locked lot is never recomputed");
        ledger.RecordedMarkings.Should().ContainSingle();

        // The lock stops the system from recomputing a lot, not the operator from putting a
        // hand-recorded mistake right, and the row stays locked either way.
        var overridden = await service.OverrideMarkingAsync(
            again.Data.Id,
            "⭐",
            "RED",
            "TEST-the floor label is wrong",
            "TEST-USER-1"
        );

        overridden.IsSuccess.Should().BeTrue();
        ledger.LastOverrideShape.Should().Be("⭐");
        ledger.RecordedMarkings.Single().ShapeCode.Should().Be("⭐");
        ledger.RecordedMarkings.Single().IsLocked.Should().BeTrue();
    }

    [Fact]
    public void Model_LotMarkingPalette_ShouldExposeEveryShapeAndColor_WhenTheLegendIsRead()
    {
        Model_LotMarkingPalette.Shapes.Should().HaveCount(10);
        Model_LotMarkingPalette.Shapes.Select(shape => shape.Glyph).Should().OnlyHaveUniqueItems();
        Model_LotMarkingPalette.Colors.Should().HaveCount(12);
        Model_LotMarkingPalette.Colors.Select(color => color.Code).Should().OnlyHaveUniqueItems();
        Model_LotMarkingPalette.CombinationCount.Should().Be(120);

        foreach (var shape in Model_LotMarkingPalette.Shapes)
        {
            Model_LotMarkingPalette.FindShape(shape.Glyph).Should().Be(shape);
            shape.DisplayName.Should().NotBeNullOrWhiteSpace();
        }

        foreach (var color in Model_LotMarkingPalette.Colors)
        {
            Model_LotMarkingPalette.FindColor(color.Code).Should().Be(color);
            color.DisplayName.Should().NotBeNullOrWhiteSpace();
        }
    }

    private static Service_Tool_LotMarking CreateService(
        Dao_Tool_LotMarking dao,
        IService_ShipRecToolsSettings? settings = null
    ) =>
        new(dao, new Mock<IService_LoggingUtility>().Object, settings ?? CreateSettings(true));

    private static IService_ShipRecToolsSettings CreateSettings(bool isEnabled)
    {
        var settings = new Mock<IService_ShipRecToolsSettings>();
        settings.Setup(service => service.GetLotMarkingEnabledAsync()).ReturnsAsync(isEnabled);
        settings
            .Setup(service => service.GetLotMarkingCollisionPolicyAsync())
            .ReturnsAsync(Enum_LotMarkingCollisionPolicy.Probe);

        return settings.Object;
    }

    [Fact]
    public async Task PreviewUnmarkedLotMarkingsAsync_ShouldCountOneLot_WhenTwoSpellingsNormalizeToTheSameHeat()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedUnmarkedRow("queue", "TEST-HEAT-5005", "TEST-SUP-A", 2);
        ledger.SeedUnmarkedRow("history", " test heat 5005 ", "TEST-SUP-A", 3);
        var service = CreateService(ledger);

        var preview = await service.PreviewUnmarkedLotMarkingsAsync("TEST-PART-1");

        preview.IsSuccess.Should().BeTrue();
        preview.Data!.DistinctLots.Should().Be(1);
        preview.Data.TotalRows.Should().Be(5);
        ledger.AppliedMarkings.Should().BeEmpty("a preview writes nothing");
    }

    [Fact]
    public async Task FillUnmarkedLotMarkingsAsync_ShouldWriteEverySpellingOfOneLot_WhenTheyNormalizeToTheSameHeat()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedUnmarkedRow("queue", "TEST-HEAT-5005", "TEST-SUP-A", 2);
        ledger.SeedUnmarkedRow("history", " test heat 5005 ", "TEST-SUP-A", 3);
        var service = CreateService(ledger);

        var filled = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-1", "TEST-USER-1");

        filled.IsSuccess.Should().BeTrue();
        filled.Data.Should().Be(5);
        ledger.RecordedMarkings.Should().ContainSingle("one lot resolves to one marking");
        ledger.AppliedMarkings.Should().HaveCount(2, "each raw spelling is matched literally");

        var shape = ledger.RecordedMarkings.Single().ShapeCode;
        var color = ledger.RecordedMarkings.Single().ColorCode;
        ledger
            .AppliedMarkings.Should()
            .OnlyContain(apply => apply.ShapeCode == shape && apply.ColorCode == color);
    }

    [Fact]
    public async Task PreviewUnmarkedLotMarkingsAsync_ShouldNotCountARowWithNoHeat_WhenTheHeatIsBlankOrTheSentinel()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedUnmarkedRow("queue", "TEST-HEAT-6006", "TEST-SUP-A", 1);
        ledger.SeedUnmarkedRow("queue", string.Empty, "TEST-SUP-A", 4);
        ledger.SeedUnmarkedRow("history", "   ", "TEST-SUP-A", 2);
        ledger.SeedUnmarkedRow("history", "Nothing Entered", "TEST-SUP-A", 3);
        var service = CreateService(ledger);

        var preview = await service.PreviewUnmarkedLotMarkingsAsync("TEST-PART-1");

        preview.Data!.DistinctLots.Should().Be(1);
        preview.Data.TotalRows.Should().Be(1);
    }

    [Fact]
    public async Task FillUnmarkedLotMarkingsAsync_ShouldLeaveRowsWithNoHeatUnmarked_WhenTheHeatIsBlankOrTheSentinel()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedUnmarkedRow("queue", "TEST-HEAT-6006", "TEST-SUP-A", 1);
        ledger.SeedUnmarkedRow("queue", string.Empty, "TEST-SUP-A", 4);
        ledger.SeedUnmarkedRow("history", "   ", "TEST-SUP-A", 2);
        ledger.SeedUnmarkedRow("history", "Nothing Entered", "TEST-SUP-A", 3);
        var service = CreateService(ledger);

        var filled = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-1", "TEST-USER-1");

        filled.Data.Should().Be(1);
        ledger.AppliedMarkings.Should().ContainSingle();
        ledger
            .UnmarkedRows.Should()
            .HaveCount(3, "a row with no heat number is not a lot and stays unmarked");
    }

    [Fact]
    public async Task FillUnmarkedLotMarkingsAsync_ShouldWriteNothing_WhenThePartHasNothingUnmarked()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        var preview = await service.PreviewUnmarkedLotMarkingsAsync("TEST-PART-2");
        var filled = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-2", "TEST-USER-1");

        preview.IsSuccess.Should().BeTrue();
        preview.Data!.DistinctLots.Should().Be(0);
        preview.Data.TotalRows.Should().Be(0);
        filled.IsSuccess.Should().BeTrue();
        filled.Data.Should().Be(0);
        ledger.AppliedMarkings.Should().BeEmpty();
        ledger.RecordedMarkings.Should().BeEmpty();
    }

    [Fact]
    public async Task FillUnmarkedLotMarkingsAsync_ShouldWriteNothing_WhenTheFeatureSwitchIsOff()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedUnmarkedRow("queue", "TEST-HEAT-7007", "TEST-SUP-A", 3);
        var service = CreateService(ledger, CreateSettings(false));

        var filled = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-1", "TEST-USER-1");

        filled.IsSuccess.Should().BeTrue();
        filled.Data.Should().Be(0);
        ledger.AppliedMarkings.Should().BeEmpty();
        ledger.RecordedMarkings.Should().BeEmpty();
    }

    [Fact]
    public async Task FillUnmarkedLotMarkingsAsync_ShouldUseTheRecordedCodes_WhenTheLotAlreadyHasAMarking()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);
        var existing = await service.ResolveMarkingAsync(
            "TEST-HEAT-8008",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var shape = existing.Data!.ShapeCode;
        var color = existing.Data.ColorCode;
        ledger.SeedUnmarkedRow("history", "test heat 8008", "TEST-SUP-A", 4);

        var filled = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-1", "TEST-USER-1");

        filled.Data.Should().Be(4);
        ledger.RecordedMarkings.Should().ContainSingle("a marking is never recomputed");
        ledger.AppliedMarkings.Should().ContainSingle();
        ledger.AppliedMarkings.Single().ShapeCode.Should().Be(shape);
        ledger.AppliedMarkings.Single().ColorCode.Should().Be(color);
    }

    [Fact]
    public async Task FillUnmarkedLotMarkingsAsync_ShouldWriteNothingFurther_WhenTheSamePartIsFilledInTwice()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedUnmarkedRow("queue", "TEST-HEAT-9009", "TEST-SUP-A", 2);
        ledger.SeedUnmarkedRow("history", "TEST-HEAT-9009", "TEST-SUP-A", 1);
        var service = CreateService(ledger);

        var first = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-1", "TEST-USER-1");
        var second = await service.FillUnmarkedLotMarkingsAsync("TEST-PART-1", "TEST-USER-1");

        first.Data.Should().Be(3);
        second.IsSuccess.Should().BeTrue();
        second.Data.Should().Be(0);
        ledger.AppliedMarkings.Should().HaveCount(1, "the second run finds nothing unmarked");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldApplyTheRecordedMarking_WhenTheCorrectedNumberAlreadyExists()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedMarking(1, "TEST-HEAT-7007", "TESTHEAT7007", "TEST-SUP-B", "TEST-SUP-B", "⭐", "RED");
        ledger.SeedHeatLotKey("queue", "TEST-HEAT-7007", "TEST-SUP-B", "⚡", "TEAL", 2);
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-1");

        result.IsSuccess.Should().BeTrue();
        result.Data!.RowsChanged.Should().Be(2);
        ledger.Realigned.Should().ContainSingle();
        ledger.Realigned.Single().ShapeCode.Should().Be("⭐");
        ledger.Realigned.Single().ColorCode.Should().Be("RED");
        ledger.RecordedMarkings.Should().ContainSingle("a number that already exists is not given a second lot");
        ledger.RemovedIds.Should().BeEmpty("the number those rows now carry is still in use");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldCreateTheLot_WhenTheCorrectedNumberDoesNotExist()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedHeatLotKey("queue", "TEST-HEAT-7008", "TEST-SUP-A", "⚡", "TEAL", 3);
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-2");

        result.IsSuccess.Should().BeTrue();
        result.Data!.LotsCreated.Should().Be(1);
        ledger.RecordedMarkings.Should().ContainSingle();
        ledger
            .RecordedMarkings.Single()
            .ResolvedBy.Should()
            .Be("TEST-USER-2", "the lot is recorded against whoever ran the update");
        ledger.Realigned.Should().ContainSingle();
        ledger
            .Realigned.Single()
            .ShapeCode.Should()
            .Be(ledger.RecordedMarkings.Single().ShapeCode, "the rows are given the marking the lot holds");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldCreateOneLot_WhenTwoSpellingsNormalizeToOneNumber()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedHeatLotKey("queue", "TEST-HEAT-7013", "TEST-SUP-A", "⚡", "TEAL", 1);
        ledger.SeedHeatLotKey("history", " test heat 7013 ", "TEST-SUP-A", "⚡", "TEAL", 2);
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-1");

        result.Data!.LotsCreated.Should().Be(1, "two spellings are one lot");
        result.Data!.RowsChanged.Should().Be(3);
        ledger.RecordedMarkings.Should().ContainSingle();
        ledger.Realigned.Should().HaveCount(2, "every spelling of the lot has to be written");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldRemoveTheLot_WhenNoRowCarriesItsNumberAnyMore()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedMarking(1, "TEST-HEAT-7009", "TESTHEAT7009", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-1");

        result.Data!.LotsRemoved.Should().Be(1);
        ledger.RemovedIds.Should().ContainSingle().Which.Should().Be(1);
        ledger.RecordedMarkings.Should().BeEmpty("its shape and color are free again");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldLeaveAProtectedLotAlone_WhenNoRowCarriesItsNumber()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedMarking(
            1,
            "TEST-HEAT-7010",
            "TESTHEAT7010",
            "TEST-SUP-A",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            isLocked: true
        );
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-1");

        result.Data!.LotsRemoved.Should().Be(0);
        result.Data!.LotsSkipped.Should().Be(1);
        ledger.RemovedIds.Should().BeEmpty();
        ledger.RecordedMarkings.Should().ContainSingle("a protected lot is never removed");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldClearTheRow_WhenTheNumberWasClearedToNothing()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedHeatLotKey("queue", string.Empty, "TEST-SUP-A", "⚡", "TEAL", 2);
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-1");

        result.Data!.RowsCleared.Should().Be(2);
        ledger.Cleared.Should().ContainSingle();
        ledger.Realigned.Should().BeEmpty();
        ledger.RecordedMarkings.Should().BeEmpty("a blank value never becomes a lot");
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldNotIntroduceAMarking_WhenTheRowCarriesNone()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedHeatLotKey("queue", "TEST-HEAT-7012", "TEST-SUP-A", string.Empty, string.Empty, 2);
        var service = CreateService(ledger);

        var result = await service.UpdateAllLotsAsync("TEST-USER-1");

        result.Data!.RowsChanged.Should().Be(0);
        ledger.Realigned.Should().BeEmpty("putting markings right never introduces one");
        ledger.RecordedMarkings.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAllLotsAsync_ShouldReportNothingToDo_WhenItIsRunASecondTime()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedHeatLotKey("queue", "TEST-HEAT-7011", "TEST-SUP-A", "⚡", "TEAL", 1);
        var service = CreateService(ledger);

        var first = await service.UpdateAllLotsAsync("TEST-USER-1");
        var second = await service.UpdateAllLotsAsync("TEST-USER-1");

        first.Data!.LotsCreated.Should().Be(1);
        second.IsSuccess.Should().BeTrue();
        second.Data!.RowsChanged.Should().Be(0);
        second.Data!.LotsCreated.Should().Be(0);
        second.Data!.LotsRemoved.Should().Be(0);
        ledger.Realigned.Should().HaveCount(1, "the second run finds nothing to put right");
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldCreateNoMarking_WhenThePartIsNotOnTheEnabledList()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        var result = await service.ResolveMarkingAsync(
            "TEST-HEAT-6001",
            "TEST-PART-OFF-LIST",
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        result.IsSuccess.Should().BeTrue("a part that is not listed is not an error");
        result.Data.Should().BeNull();
        ledger.RecordedMarkings.Should().BeEmpty();
        ledger.InsertCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldCreateNoMarking_WhenThePartWasRemovedFromTheList()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedPart("TEST-PART-REMOVED", isEnabled: false);
        var service = CreateService(ledger);

        var result = await service.ResolveMarkingAsync(
            "TEST-HEAT-6002",
            "TEST-PART-REMOVED",
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        result.Data.Should().BeNull("a removed part receives no new marking");
        ledger.RecordedMarkings.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldCreateNoMarking_WhenTheReceiptCarriesNoPartNumber()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        var result = await service.ResolveMarkingAsync(
            "TEST-HEAT-6003",
            null,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
        ledger.RecordedMarkings.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldCreateNoMarking_WhenTheEnabledListCannotBeRead()
    {
        var ledger = new FakeLotMarkingLedger { FailEnabledPartsRead = true };
        var service = CreateService(ledger);

        var result = await service.ResolveMarkingAsync(
            "TEST-HEAT-6004",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
        ledger.RecordedMarkings.Should().BeEmpty(
            "an unreadable list must refuse the marking rather than risk one on material nobody approved"
        );
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldReturnTheRecordedMarking_WhenThePartHasSinceLeftTheList()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedMarking(
            1,
            "TEST-HEAT-6005",
            "TESTHEAT6005",
            "TEST-SUP-A",
            "TEST-SUP-A",
            "⚡",
            "TEAL"
        );
        var service = CreateService(ledger);

        var result = await service.ResolveMarkingAsync(
            "TEST-HEAT-6005",
            "TEST-PART-OFF-LIST",
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        result.Data.Should().NotBeNull("a marking a handler has already seen is never withdrawn");
        result.Data!.ShapeCode.Should().Be("⚡");
        result.Data.ColorCode.Should().Be("TEAL");
        ledger.InsertCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ResolveMarkingAsync_ShouldRecordThePartThatFirstCreatedTheLot_WhenTheMarkingIsCreated()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);

        await service.ResolveMarkingAsync(
            "TEST-HEAT-6006",
            FakeLotMarkingLedger.EnabledPartId,
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );

        ledger.RecordedMarkings.Should().ContainSingle();
        ledger.RecordedMarkings[0].FirstSeenPartNumber.Should().Be(FakeLotMarkingLedger.EnabledPartId);
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldListALot_WhenAnEnabledPartCarriesIt()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9001", "TESTHEAT9001", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9001",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            2,
            partId: FakeLotMarkingLedger.EnabledPartId
        );

        var result = await service.GetLedgerAsync(null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].PartsText.Should().Be(FakeLotMarkingLedger.EnabledPartId);
        result.Data[0].FirstSeenPartNumber.Should().BeEmpty(
            "the parts a lot belongs to come from the rows that carry it, not from the marking, "
                + "so attribution is right for lots recorded before the list was consulted"
        );
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldReportEveryPart_WhenAnOffListPartCarriesTheSameLot()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedPart("TEST-PART-OFF-LIST", isEnabled: false);
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9002", "TESTHEAT9002", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9002",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            2,
            partId: FakeLotMarkingLedger.EnabledPartId
        );
        ledger.SeedHeatLotKey(
            "history",
            "TEST-HEAT-9002",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            3,
            partId: "TEST-PART-OFF-LIST"
        );

        var result = await service.GetLedgerAsync(null);

        result.Data.Should().ContainSingle("one of the parts carrying the lot is on the list");
        result.Data![0]
            .PartsText.Should()
            .Be($"{FakeLotMarkingLedger.EnabledPartId}, TEST-PART-OFF-LIST");
        result.Data[0].LineCount.Should().Be(5, "the lines behind both parts are still the lot's lines");
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldLeaveOutALot_WhenOnlyPartsOffTheListCarryIt()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedPart("TEST-PART-OFF-LIST", isEnabled: false);
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9003", "TESTHEAT9003", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9003",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            2,
            partId: "TEST-PART-OFF-LIST"
        );

        var result = await service.GetLedgerAsync(null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldKeepALotThatNoReceivingRowCarries_WhenNothingCanAttributeIt()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9004", "TESTHEAT9004", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");

        var result = await service.GetLedgerAsync(null);

        result.Data.Should().ContainSingle(
            "a lot no row carries cannot be blamed on a part, and hiding it would hide the lot a "
                + "corrected number used to describe"
        );
        result.Data![0].PartsText.Should().Be("-");
        result.Data[0].LineCount.Should().Be(0);
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldNarrowToOnePart_WhenAPartIsChosen()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedPart("TEST-PART-2");
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9101", "TESTHEAT9101", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        ledger.SeedMarking(2, "TEST-HEAT-9102", "TESTHEAT9102", "TEST-SUP-A", "TEST-SUP-A", "⭐", "RED");
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9101",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            1,
            partId: FakeLotMarkingLedger.EnabledPartId
        );
        ledger.SeedHeatLotKey("queue", "TEST-HEAT-9102", "TEST-SUP-A", "⭐", "RED", 1, partId: "TEST-PART-2");

        var everything = await service.GetLedgerAsync(null);
        var firstOnly = await service.GetLedgerAsync(null, FakeLotMarkingLedger.EnabledPartId);
        var secondOnly = await service.GetLedgerAsync(null, "TEST-PART-2");

        everything.Data.Should().HaveCount(2);
        firstOnly.Data.Should().ContainSingle();
        firstOnly.Data![0].HeatNumber.Should().Be("TEST-HEAT-9101");
        secondOnly.Data.Should().ContainSingle();
        secondOnly.Data![0].HeatNumber.Should().Be("TEST-HEAT-9102");
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldShowARemovedPartsLots_WhenThatPartIsChosen()
    {
        var ledger = new FakeLotMarkingLedger();
        ledger.SeedPart("TEST-PART-REMOVED", isEnabled: false);
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9103", "TESTHEAT9103", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9103",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            1,
            partId: "TEST-PART-REMOVED"
        );

        var unasked = await service.GetLedgerAsync(null);
        var chosen = await service.GetLedgerAsync(null, "TEST-PART-REMOVED");

        unasked.Data.Should().BeEmpty();
        chosen.Data.Should().ContainSingle(
            "choosing a part is deliberate, and a removed part's recorded markings must stay reachable"
        );
    }

    [Fact]
    public async Task GetLedgerAsync_ShouldStillFilterByHeatNumber_WithinThePartScope()
    {
        var ledger = new FakeLotMarkingLedger();
        var service = CreateService(ledger);
        ledger.SeedMarking(1, "TEST-HEAT-9201", "TESTHEAT9201", "TEST-SUP-A", "TEST-SUP-A", "⚡", "TEAL");
        ledger.SeedMarking(2, "TEST-HEAT-9202", "TESTHEAT9202", "TEST-SUP-A", "TEST-SUP-A", "⭐", "RED");
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9201",
            "TEST-SUP-A",
            "⚡",
            "TEAL",
            1,
            partId: FakeLotMarkingLedger.EnabledPartId
        );
        ledger.SeedHeatLotKey(
            "queue",
            "TEST-HEAT-9202",
            "TEST-SUP-A",
            "⭐",
            "RED",
            1,
            partId: FakeLotMarkingLedger.EnabledPartId
        );

        var searched = await service.GetLedgerAsync("9202");

        searched.Data.Should().ContainSingle();
        searched.Data![0].HeatNumber.Should().Be("TEST-HEAT-9202");
    }

    /// <summary>
    /// An in-memory stand-in for the ledger table, so the tests can assert how many markings were
    /// recorded and read them back the way the stored procedures would.
    /// </summary>
    private sealed class FakeLotMarkingLedger : Dao_Tool_LotMarking
    {
        /// <summary>
        /// The part number the resolver tests mark against, and the one the fill-in tests pass.
        /// It is on the enabled list from the start because a marking is only ever created for a
        /// part that is, so without it every existing resolve test would be asserting the refusal
        /// path by accident.
        /// </summary>
        public const string EnabledPartId = "TEST-PART-1";

        private readonly List<Model_Tool_LotMarking> _recordedMarkings = new();

        private readonly List<Model_Tool_LotMarkingScope> _partScope = new();

        public FakeLotMarkingLedger()
            : base(TestConnectionString)
        {
            SeedPart(EnabledPartId);
        }

        public IReadOnlyList<Model_Tool_LotMarking> RecordedMarkings => _recordedMarkings;

        public IReadOnlyList<Model_Tool_LotMarkingScope> PartScope => _partScope;

        /// <summary>Makes the enabled-list read fail, so the fail-closed path can be asserted.</summary>
        public bool FailEnabledPartsRead { get; set; }

        /// <summary>
        /// Puts a part on the list. The read returns every row rather than only the enabled ones,
        /// because that is what the procedure does and the service is what filters.
        /// </summary>
        public void SeedPart(string partId, bool isEnabled = true) =>
            _partScope.Add(
                new Model_Tool_LotMarkingScope
                {
                    Id = _partScope.Count + 1,
                    PartId = partId,
                    IsEnabled = isEnabled,
                }
            );

        public override Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>>
            GetEnabledPartsAsync() =>
            Task.FromResult(
                FailEnabledPartsRead
                    ? Model_Dao_Result_Factory.Failure<List<Model_Tool_LotMarkingScope>>(
                        "TEST-enabled-list-unavailable"
                    )
                    : Model_Dao_Result_Factory.Success(_partScope.ToList())
            );

        public int InsertCallCount { get; private set; }

        public int OverrideCallCount { get; private set; }

        public string? LastOverrideShape { get; private set; }

        public string? LastOverrideColor { get; private set; }

        public string? LastOverrideSource { get; private set; }

        public string? LastOverrideReason { get; private set; }

        public string? LastOverrideUser { get; private set; }

        public override Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetAllMarkingsAsync() =>
            Task.FromResult(Model_Dao_Result_Factory.Success(_recordedMarkings.ToList()));

        public override Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> ResolveAsync(
            string heatNumberNormalized,
            string supplierId
        ) =>
            Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    _recordedMarkings
                        .Where(marking =>
                            string.Equals(
                                marking.HeatNumberNormalized,
                                heatNumberNormalized,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && string.Equals(
                                marking.SupplierId,
                                supplierId,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        .ToList()
                )
            );

        public override Task<Model_Dao_Result<int>> InsertAsync(
            string heatNumber,
            string heatNumberNormalized,
            string supplierId,
            string? supplierName,
            string shapeGlyph,
            string colorCode,
            string markingSource,
            string? firstSeenPartNumber,
            string? resolvedBy
        )
        {
            var id = _recordedMarkings.Count + 1;
            InsertCallCount++;

            _recordedMarkings.Add(
                new Model_Tool_LotMarking
                {
                    Id = id,
                    HeatNumber = heatNumber,
                    HeatNumberNormalized = heatNumberNormalized,
                    SupplierId = supplierId,
                    SupplierName = supplierName ?? string.Empty,
                    ShapeCode = shapeGlyph,
                    ColorCode = colorCode,
                    MarkingSource = markingSource,
                    FirstSeenPartNumber = firstSeenPartNumber ?? string.Empty,
                    ResolvedBy = resolvedBy ?? string.Empty,
                }
            );

            return Task.FromResult(Model_Dao_Result_Factory.Success<int>(id));
        }

        public override Task<Model_Dao_Result<int>> UpdateMarkingAsync(
            int id,
            string shapeGlyph,
            string colorCode,
            string markingSource,
            string reason,
            string actingUser
        )
        {
            OverrideCallCount++;
            LastOverrideShape = shapeGlyph;
            LastOverrideColor = colorCode;
            LastOverrideSource = markingSource;
            LastOverrideReason = reason;
            LastOverrideUser = actingUser;

            var stored = _recordedMarkings.FirstOrDefault(marking => marking.Id == id);
            if (stored is null)
            {
                return Task.FromResult(Model_Dao_Result_Factory.Failure<int>("Lot marking not found"));
            }

            stored.ShapeCode = shapeGlyph;
            stored.ColorCode = colorCode;
            stored.MarkingSource = markingSource;
            stored.ChangeReason = reason;
            stored.ResolvedBy = actingUser;

            return Task.FromResult(Model_Dao_Result_Factory.Success<int>(1));
        }

        /// <summary>One call the service made to write a lot's marking onto its unmarked rows.</summary>
        public sealed record AppliedLotMarking(
            string Heat,
            string SupplierName,
            string ShapeCode,
            string ColorCode
        );

        private readonly List<Model_Tool_UnmarkedLotRow> _unmarkedRows = new();

        private readonly List<AppliedLotMarking> _appliedMarkings = new();

        public IReadOnlyList<Model_Tool_UnmarkedLotRow> UnmarkedRows => _unmarkedRows;

        public IReadOnlyList<AppliedLotMarking> AppliedMarkings => _appliedMarkings;

        public void SeedUnmarkedRow(string source, string heat, string supplierName, int rowCount) =>
            _unmarkedRows.Add(new Model_Tool_UnmarkedLotRow(source, heat, supplierName, rowCount));

        public override Task<Model_Dao_Result<List<Model_Tool_UnmarkedLotRow>>> GetUnmarkedLotRowsAsync(
            string partId
        ) => Task.FromResult(Model_Dao_Result_Factory.Success(_unmarkedRows.ToList()));

        public override Task<Model_Dao_Result<int>> ApplyUnmarkedLotMarkingAsync(
            string partId,
            string heat,
            string supplierName,
            string shapeGlyph,
            string colorCode
        )
        {
            if (
                string.IsNullOrWhiteSpace(heat)
                || string.IsNullOrWhiteSpace(shapeGlyph)
                || string.IsNullOrWhiteSpace(colorCode)
            )
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Failure<int>(
                        "A shape and a color are both required to mark a lot."
                    )
                );
            }

            // The stored procedure matches the heat and the supplier literally, and writes every
            // matching row in one statement, so the double removes them the same way.
            var matches = _unmarkedRows
                .Where(row =>
                    string.Equals(row.Heat, heat, StringComparison.Ordinal)
                    && string.Equals(row.SupplierName, supplierName, StringComparison.Ordinal)
                )
                .ToList();

            var rowsUpdated = matches.Sum(row => row.RowCount);

            if (matches.Count > 0)
            {
                _appliedMarkings.Add(new AppliedLotMarking(heat, supplierName, shapeGlyph, colorCode));

                foreach (var match in matches)
                {
                    _unmarkedRows.Remove(match);
                }
            }

            return Task.FromResult(Model_Dao_Result_Factory.Success<int>(rowsUpdated));
        }

        /// <summary>One call the service made to write a marking onto rows that already had one.</summary>
        public sealed record RealignedRowWrite(
            string Heat,
            string SupplierName,
            string ShapeCode,
            string ColorCode
        );

        /// <summary>One call the service made to take a marking off rows.</summary>
        public sealed record ClearedRowWrite(string Heat, string SupplierName);

        private readonly List<Model_Tool_ReceivingHeatLotKey> _heatLotKeys = new();

        private readonly List<RealignedRowWrite> _realigned = new();

        private readonly List<ClearedRowWrite> _cleared = new();

        private readonly List<int> _removedIds = new();

        public IReadOnlyList<Model_Tool_ReceivingHeatLotKey> HeatLotKeys => _heatLotKeys;

        public IReadOnlyList<RealignedRowWrite> Realigned => _realigned;

        public IReadOnlyList<ClearedRowWrite> Cleared => _cleared;

        public IReadOnlyList<int> RemovedIds => _removedIds;

        public void SeedMarking(
            int id,
            string heatNumber,
            string heatNumberNormalized,
            string supplierId,
            string supplierName,
            string shapeCode,
            string colorCode,
            bool isLocked = false
        ) =>
            _recordedMarkings.Add(
                new Model_Tool_LotMarking
                {
                    Id = id,
                    HeatNumber = heatNumber,
                    HeatNumberNormalized = heatNumberNormalized,
                    SupplierId = supplierId,
                    SupplierName = supplierName,
                    ShapeCode = shapeCode,
                    ColorCode = colorCode,
                    IsLocked = isLocked,
                }
            );

        public void SeedHeatLotKey(
            string source,
            string heat,
            string supplierName,
            string shapeCode,
            string colorCode,
            int rowCount,
            DateTime? lastLineDate = null,
            string partId = ""
        ) =>
            _heatLotKeys.Add(
                new Model_Tool_ReceivingHeatLotKey(
                    source,
                    heat,
                    supplierName,
                    partId,
                    shapeCode,
                    colorCode,
                    rowCount,
                    lastLineDate
                )
            );

        public override Task<Model_Dao_Result<List<Model_Tool_ReceivingHeatLotKey>>>
            GetReceivingHeatLotKeysAsync() =>
            Task.FromResult(Model_Dao_Result_Factory.Success(_heatLotKeys.ToList()));

        public override Task<Model_Dao_Result<int>> RealignLotMarkingRowsAsync(
            string heat,
            string supplierName,
            string shapeGlyph,
            string colorCode
        )
        {
            if (string.IsNullOrWhiteSpace(shapeGlyph) || string.IsNullOrWhiteSpace(colorCode))
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Failure<int>(
                        "A shape and a color are both required to put a marking right."
                    )
                );
            }

            var rows = 0;

            // The stored procedure matches the heat and supplier literally, writes only rows that
            // already carry a marking, and leaves the table showing the new codes.
            for (var index = 0; index < _heatLotKeys.Count; index++)
            {
                var key = _heatLotKeys[index];
                if (
                    string.IsNullOrWhiteSpace(key.ShapeCode)
                    || !string.Equals(key.Heat, heat, StringComparison.Ordinal)
                    || !string.Equals(key.SupplierName, supplierName, StringComparison.Ordinal)
                )
                {
                    continue;
                }

                rows += key.RowCount;
                _heatLotKeys[index] = key with { ShapeCode = shapeGlyph, ColorCode = colorCode };
            }

            if (rows > 0)
            {
                _realigned.Add(new RealignedRowWrite(heat, supplierName, shapeGlyph, colorCode));
            }

            return Task.FromResult(Model_Dao_Result_Factory.Success<int>(rows));
        }

        public override Task<Model_Dao_Result<int>> ClearLotMarkingRowsAsync(
            string heat,
            string supplierName
        )
        {
            var rows = 0;

            for (var index = 0; index < _heatLotKeys.Count; index++)
            {
                var key = _heatLotKeys[index];
                if (
                    string.IsNullOrWhiteSpace(key.ShapeCode)
                    || !string.Equals(key.Heat, heat, StringComparison.Ordinal)
                    || !string.Equals(key.SupplierName, supplierName, StringComparison.Ordinal)
                )
                {
                    continue;
                }

                rows += key.RowCount;
                _heatLotKeys[index] = key with
                {
                    ShapeCode = string.Empty,
                    ColorCode = string.Empty,
                };
            }

            if (rows > 0)
            {
                _cleared.Add(new ClearedRowWrite(heat, supplierName));
            }

            return Task.FromResult(Model_Dao_Result_Factory.Success<int>(rows));
        }

        public override Task<Model_Dao_Result<int>> DeleteMarkingAsync(int id)
        {
            var stored = _recordedMarkings.FirstOrDefault(marking => marking.Id == id);
            if (stored is null)
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Failure<int>("That lot is no longer in the list.")
                );
            }

            // The stored procedure refuses a protected lot, so the double does too.
            if (stored.IsLocked)
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Failure<int>(
                        "That lot is protected, so it is never removed."
                    )
                );
            }

            _recordedMarkings.Remove(stored);
            _removedIds.Add(id);

            return Task.FromResult(Model_Dao_Result_Factory.Success<int>(1));
        }
    }
}
