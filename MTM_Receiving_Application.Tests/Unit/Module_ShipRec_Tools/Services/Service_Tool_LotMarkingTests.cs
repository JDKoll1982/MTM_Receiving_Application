using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
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

    private static Service_Tool_LotMarking CreateService(Dao_Tool_LotMarking dao) =>
        new(dao, new Mock<IService_LoggingUtility>().Object);

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
    }
}
