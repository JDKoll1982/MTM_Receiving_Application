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
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var second = await service.ResolveMarkingAsync(
            "TEST-HEAT-1001",
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
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var respelled = await service.ResolveMarkingAsync(
            " test heat 1001 ",
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
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var secondSupplier = await service.ResolveMarkingAsync(
            "TEST-HEAT-2002",
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
            firstSupplier,
            Enum_LotMarkingCollisionPolicy.Probe
        );
        var second = await service.ResolveMarkingAsync(
            "TEST-HEAT-4004",
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
            "TEST-SUP-A",
            Enum_LotMarkingCollisionPolicy.Probe
        );
        created.Data!.IsLocked = true;
        var shape = created.Data.ShapeCode;
        var color = created.Data.ColorCode;

        var again = await service.ResolveMarkingAsync(
            "TEST-HEAT-4004",
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

    /// <summary>
    /// An in-memory stand-in for the ledger table, so the tests can assert how many markings were
    /// recorded and read them back the way the stored procedures would.
    /// </summary>
    private sealed class FakeLotMarkingLedger : Dao_Tool_LotMarking
    {
        private readonly List<Model_Tool_LotMarking> _recordedMarkings = new();

        public FakeLotMarkingLedger()
            : base(TestConnectionString) { }

        public IReadOnlyList<Model_Tool_LotMarking> RecordedMarkings => _recordedMarkings;

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
    }
}
