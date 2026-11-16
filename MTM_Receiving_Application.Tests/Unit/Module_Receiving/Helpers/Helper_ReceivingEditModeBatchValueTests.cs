using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers the batch edit value reader, which is the part that decides whether a typed value is safe
/// to write onto every ticked row.
/// </summary>
public sealed class Helper_ReceivingEditModeBatchValueTests
{
    private static readonly Model_EditModeBatchField IntegerField = new()
    {
        Key = "PackagesPerLoad",
        Label = "Packages per load",
        Kind = Enum_EditModeBatchFieldKind.Integer,
    };

    private static readonly Model_EditModeBatchField DecimalField = new()
    {
        Key = "WeightPerPackage",
        Label = "Weight per package",
        Kind = Enum_EditModeBatchFieldKind.Decimal,
    };

    private static readonly Model_EditModeBatchField PackageTypeField = new()
    {
        Key = "PackageTypeName",
        Label = "Package type",
        Kind = Enum_EditModeBatchFieldKind.PackageType,
    };

    private static readonly Model_EditModeBatchField TextField = new()
    {
        Key = "HeatLotNumber",
        Label = "Heat/Lot number",
        Kind = Enum_EditModeBatchFieldKind.Text,
    };

    [Fact]
    public void TryRead_ShouldTrimText_ForATextField()
    {
        Helper_ReceivingEditModeBatchValue
            .TryRead(TextField, "  25-1043  ", out var value, out var error)
            .Should()
            .BeTrue();

        value.Should().Be("25-1043");
        error.Should().BeEmpty();
    }

    [Fact]
    public void TryRead_ShouldRefuseAWord_ForAWholeNumberField()
    {
        Helper_ReceivingEditModeBatchValue
            .TryRead(IntegerField, "two", out var value, out var error)
            .Should()
            .BeFalse();

        value.Should().BeNull("a refused value must never be written as zero");
        error.Should().Contain("whole number");
    }

    [Fact]
    public void TryRead_ShouldReadDecimals_WithTheCurrentCulture()
    {
        Helper_ReceivingEditModeBatchValue
            .TryRead(DecimalField, "1250.5", out var value, out _)
            .Should()
            .BeTrue();

        value.Should().Be(1250.5m);
    }

    [Theory]
    [InlineData("Skid", "Skid")]
    [InlineData("skids", "Skid")]
    [InlineData("COIL", "Coil")]
    [InlineData("Sheets", "Sheet")]
    public void TryRead_ShouldAcceptAPackageTypeNameOrItsPlural(
        string typed,
        string expected)
    {
        Helper_ReceivingEditModeBatchValue
            .TryRead(PackageTypeField, typed, out var value, out _)
            .Should()
            .BeTrue();

        value.Should().Be(expected);
    }

    [Fact]
    public void TryRead_ShouldNameTheAllowedPackageTypes_WhenTheValueIsNotOne()
    {
        Helper_ReceivingEditModeBatchValue
            .TryRead(PackageTypeField, "Trolley", out _, out var error)
            .Should()
            .BeFalse();

        error.Should().Contain("Package type must be one of");
        error.Should().Contain("Skid");
    }

    [Fact]
    public void TryRead_ShouldRefuseAnEmptyValue_ForEveryField()
    {
        foreach (var field in new[] { TextField, IntegerField, DecimalField, PackageTypeField })
        {
            Helper_ReceivingEditModeBatchValue
                .TryRead(field, "   ", out _, out var error)
                .Should()
                .BeFalse($"{field.Label} must not be blanked by accident");

            error.Should().Contain("first");
        }
    }

    [Fact]
    public void BatchFields_ShouldOfferOnlyTheFieldsThatGetCorrectedInBulk()
    {
        var keys = new[]
        {
            "HeatLotNumber",
            "InitialLocation",
            "PackageTypeName",
            "PackagesPerLoad",
            "WeightPerPackage",
        };

        keys.Should().OnlyHaveUniqueItems();
        keys.Should().NotContain("PartID", "an identifier is never rewritten in bulk");
        keys.Should().NotContain("WeightQuantity", "the received amount belongs to the receipt");
    }

    [Fact]
    public void BatchField_ShouldAnnounceItsLabel_RatherThanItsTypeName()
    {
        PackageTypeField.ToString().Should().Be("Package type");
    }
}
