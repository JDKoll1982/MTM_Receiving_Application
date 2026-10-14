using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Covers the resolver's contract: the same lot always resolves to the same marking, lots are
/// keyed on the normalized heat number and the supplier group, and a marking already recorded is
/// never moved.
/// </summary>
public sealed class Helper_LotMarkingResolverTests
{
    private const string UniformPolicy = "the same lot always resolves to one marking";

    [Fact]
    public void TryResolve_ShouldReturnTheSameMarking_WhenTheSameLotIsResolvedTwice()
    {
        var first = ResolveForMarking("TEST-HEAT-1001", "TEST-VENDOR-A", []);

        var second = ResolveForMarking("TEST-HEAT-1001", "TEST-VENDOR-A", []);

        second.Should().Be(first, UniformPolicy);
    }

    [Fact]
    public void TryResolve_ShouldReturnOneMarking_WhenTheHeatNumberIsSpelledDifferently()
    {
        var canonical = ResolveForMarking("ABC-123", "TEST-VENDOR-A", []);
        var runTogether = ResolveForMarking("abc123", "TEST-VENDOR-A", []);
        var spaced = ResolveForMarking("ABC 123", "TEST-VENDOR-A", []);

        runTogether.Should().Be(canonical, UniformPolicy);
        spaced.Should().Be(canonical, UniformPolicy);

        Helper_LotNumberNormalizer.Normalize("ABC-123").Should().Be("ABC123");
        Helper_LotNumberNormalizer.Normalize("abc123").Should().Be("ABC123");
        Helper_LotNumberNormalizer.Normalize("ABC 123").Should().Be("ABC123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void TryResolve_ShouldReturnNoMarking_WhenTheHeatNumberIsBlank(string heatNumber)
    {
        var resolved = Helper_LotMarkingResolver.TryResolve(
            heatNumber,
            "TEST-VENDOR-A",
            [],
            Enum_LotMarkingCollisionPolicy.Probe,
            out var shapeCode,
            out var colorCode
        );

        resolved.Should().BeFalse();
        shapeCode.Should().BeEmpty();
        colorCode.Should().BeEmpty();
    }

    [Fact]
    public void TryResolve_ShouldReturnNoMarking_WhenThereIsNoHeatNumberAtAll()
    {
        var resolved = Helper_LotMarkingResolver.TryResolve(
            null,
            "TEST-VENDOR-A",
            [],
            Enum_LotMarkingCollisionPolicy.Probe,
            out var shapeCode,
            out var colorCode
        );

        resolved.Should().BeFalse();
        shapeCode.Should().BeEmpty();
        colorCode.Should().BeEmpty();
    }

    [Fact]
    public void TryResolve_ShouldReturnNoMarking_WhenTheHeatNumberIsTheBlankSentinel()
    {
        var resolved = Helper_LotMarkingResolver.TryResolve(
            Helper_LotNumberNormalizer.BlankHeatSentinel,
            "TEST-VENDOR-A",
            [],
            Enum_LotMarkingCollisionPolicy.Probe,
            out _,
            out _
        );

        resolved.Should().BeFalse("the sentinel means no heat number was entered, not a lot of its own");
        Helper_LotNumberNormalizer.IsBlank(Helper_LotNumberNormalizer.BlankHeatSentinel).Should().BeTrue();
    }

    [Fact]
    public void TryResolve_ShouldKeepTwoSuppliersApart_WhenTheyShareAHeatNumber()
    {
        var heatNumber = "TEST-HEAT-3003";
        var firstSupplier = ResolveForMarking(heatNumber, "TEST-VENDOR-A", []);

        var seedForFirst = Helper_LotMarkingResolver.ComputeSeed(
            Helper_LotNumberNormalizer.Normalize(heatNumber),
            Helper_LotMarkingResolver.NormalizeSupplier("TEST-VENDOR-A")
        );
        var seedForSecond = Helper_LotMarkingResolver.ComputeSeed(
            Helper_LotNumberNormalizer.Normalize(heatNumber),
            Helper_LotMarkingResolver.NormalizeSupplier("TEST-VENDOR-B")
        );
        seedForSecond.Should().NotBe(seedForFirst);

        var markingOfFirstSupplier = CreateMarking(
            heatNumber,
            "TEST-VENDOR-A",
            firstSupplier.ShapeCode,
            firstSupplier.ColorCode
        );

        var secondSupplier = ResolveForMarking(
            heatNumber,
            "TEST-VENDOR-B",
            [markingOfFirstSupplier]
        );

        secondSupplier
            .Should()
            .NotBe(firstSupplier, "a second supplier is a different lot, so it cannot reuse the marking");
    }

    [Fact]
    public void NormalizeSupplier_ShouldUseTheSharedGroup_WhenTheReceiptNamesNoSupplier()
    {
        Helper_LotMarkingResolver
            .NormalizeSupplier(null)
            .Should()
            .Be(Helper_LotMarkingResolver.NoSupplierGroup);
        Helper_LotMarkingResolver
            .NormalizeSupplier("   ")
            .Should()
            .Be(Helper_LotMarkingResolver.NoSupplierGroup);
        Helper_LotMarkingResolver.NormalizeSupplier("  acme steel ").Should().Be("ACME STEEL");

        Helper_LotMarkingResolver
            .ComputeSeed("TESTHEAT4004", Helper_LotMarkingResolver.NormalizeSupplier(null))
            .Should()
            .Be(
                Helper_LotMarkingResolver.ComputeSeed(
                    "TESTHEAT4004",
                    Helper_LotMarkingResolver.NoSupplierGroup
                )
            );
    }

    [Fact]
    public void TryResolve_ShouldAdvancePastATakenMarking_WhenThePolicyIsProbe()
    {
        var heatNumber = "TEST-HEAT-5005";
        var seeded = ResolveForMarking(heatNumber, "TEST-VENDOR-A", []);

        var heldByAnotherLot = CreateMarking(
            "TEST-HEAT-9999",
            "TEST-VENDOR-A",
            seeded.ShapeCode,
            seeded.ColorCode
        );

        var probed = ResolveForMarking(heatNumber, "TEST-VENDOR-A", [heldByAnotherLot]);

        probed
            .Should()
            .NotBe(seeded, "probing must advance past the marking another live lot already holds");
        Helper_LotMarkingResolver
            .IsTaken([heldByAnotherLot], probed.ShapeCode, probed.ColorCode)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void TryResolve_ShouldAcceptATakenMarking_WhenThePolicyIsAllow()
    {
        var heatNumber = "TEST-HEAT-6006";
        var seeded = ResolveForMarking(heatNumber, "TEST-VENDOR-A", []);

        var heldByAnotherLot = CreateMarking(
            "TEST-HEAT-9998",
            "TEST-VENDOR-A",
            seeded.ShapeCode,
            seeded.ColorCode
        );

        var accepted = ResolveForMarking(
            heatNumber,
            "TEST-VENDOR-A",
            [heldByAnotherLot],
            Enum_LotMarkingCollisionPolicy.Allow
        );

        accepted.Should().Be(seeded, "the allow policy keeps the seeded marking even when it collides");
    }

    [Fact]
    public void TryResolve_ShouldReturnAMarkingUntouched_WhenTheLotAlreadyHasALockedOne()
    {
        var heatNumber = "TEST-HEAT-7007";
        var seeded = ResolveForMarking(heatNumber, "TEST-VENDOR-A", []);

        var lockedShape = Model_LotMarkingPalette.Shapes.First(shape => shape.Glyph != seeded.ShapeCode);
        var lockedColor = Model_LotMarkingPalette.Colors.First(color => color.Code != seeded.ColorCode);
        var locked = CreateMarking(
            heatNumber,
            "TEST-VENDOR-A",
            lockedShape.Glyph,
            lockedColor.Code
        );
        locked.IsLocked = true;

        var resolved = ResolveForMarking(heatNumber, "TEST-VENDOR-A", [locked]);

        resolved
            .Should()
            .Be((lockedShape.Glyph, lockedColor.Code), "a locked row is never recomputed or moved");
    }

    [Fact]
    public void TryResolve_ShouldStillReturnAMarking_WhenEveryPaletteCombinationIsTaken()
    {
        var taken = new List<Model_Tool_LotMarking>();
        var index = 0;

        foreach (var shape in Model_LotMarkingPalette.Shapes)
        {
            foreach (var color in Model_LotMarkingPalette.Colors)
            {
                index++;
                taken.Add(
                    CreateMarking($"TEST-HEAT-TAKEN-{index}", "TEST-VENDOR-A", shape.Glyph, color.Code)
                );
            }
        }

        var resolved = Helper_LotMarkingResolver.TryResolve(
            "TEST-HEAT-8008",
            "TEST-VENDOR-A",
            taken,
            Enum_LotMarkingCollisionPolicy.Probe,
            out var shapeCode,
            out var colorCode
        );

        resolved.Should().BeTrue("the resolver never fails, even with the palette exhausted");
        shapeCode.Should().NotBeNullOrWhiteSpace();
        colorCode.Should().NotBeNullOrWhiteSpace();
    }

    private static (string ShapeCode, string ColorCode) ResolveForMarking(
        string heatNumber,
        string? supplier,
        IReadOnlyList<Model_Tool_LotMarking> existingMarkings,
        Enum_LotMarkingCollisionPolicy policy = Enum_LotMarkingCollisionPolicy.Probe
    )
    {
        Helper_LotMarkingResolver
            .TryResolve(heatNumber, supplier, existingMarkings, policy, out var shapeCode, out var colorCode)
            .Should()
            .BeTrue();

        return (shapeCode, colorCode);
    }

    private static Model_Tool_LotMarking CreateMarking(
        string heatNumber,
        string? supplier,
        string shapeCode,
        string colorCode
    ) =>
        new()
        {
            HeatNumberNormalized = Helper_LotNumberNormalizer.Normalize(heatNumber),
            SupplierId = Helper_LotMarkingResolver.NormalizeSupplier(supplier),
            ShapeCode = shapeCode,
            ColorCode = colorCode,
        };
}
