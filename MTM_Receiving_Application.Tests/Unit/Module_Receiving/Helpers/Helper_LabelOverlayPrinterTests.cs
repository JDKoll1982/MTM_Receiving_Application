using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers the overlay's readable text and the guard that keeps a circle on the tag. Both decide what a
/// sheet of pre-printed tags comes out looking like, and neither needs a printer to check.
/// </summary>
public sealed class Helper_LabelOverlayPrinterTests
{
    [Theory]
    [InlineData("BLUE", "⭐", "Blue Star")]
    [InlineData("RED", "⭕", "Red Circle")]
    [InlineData("TEAL", "🔺", "Teal Triangle")]
    public void DescribeMarking_ShouldJoinTheColorAndShape_WhenBothAreKnown(
        string colorCode,
        string shapeCode,
        string expected
    )
    {
        Helper_LabelOverlayPrinter.DescribeMarking(colorCode, shapeCode).Should().Be(expected);
    }

    [Fact]
    public void DescribeMarking_ShouldUseTheShapeAlone_WhenTheColorIsNotInThePalette()
    {
        Helper_LabelOverlayPrinter
            .DescribeMarking("PLAID", "⭐")
            .Should()
            .Be("Star", "a color the palette no longer offers must not print as its raw code");
    }

    [Fact]
    public void DescribeMarking_ShouldBeEmpty_WhenNeitherHalfIsKnown()
    {
        Helper_LabelOverlayPrinter.DescribeMarking(null, "  ").Should().BeEmpty();
    }

    [Fact]
    public void ResolveFillColor_ShouldUseThePaletteColour_WhenTheCodeIsKnown()
    {
        var color = Helper_LabelOverlayPrinter.ResolveFillColor("RED");

        color.R.Should().Be(0xD9);
        color.G.Should().Be(0x30);
        color.B.Should().Be(0x25);
    }

    [Fact]
    public void ResolveFillColor_ShouldFallBackToGrey_WhenTheCodeIsUnknown()
    {
        var unknown = Helper_LabelOverlayPrinter.ResolveFillColor("PLAID");
        var grey = Helper_LabelOverlayPrinter.ResolveFillColor(null);

        unknown.Should().Be(grey, "an unknown color must still print a visible circle");
    }

    [Fact]
    public void DefaultGeometry_ShouldSitInsideTheTag()
    {
        Model_LabelOverlayGeometry
            .Default.IsInsideTag(
                Helper_LabelOverlayPrinter.PageWidthInches,
                Helper_LabelOverlayPrinter.PageHeightInches
            )
            .Should()
            .BeTrue("the default is measured from the tag's own artwork");
    }

    [Fact]
    public void IsInsideTag_ShouldBeFalse_WhenTheCircleRunsOffTheLeftEdge()
    {
        var geometry = new Model_LabelOverlayGeometry(0.2f, 4f, 2f, "Lot Code");

        geometry
            .IsInsideTag(
                Helper_LabelOverlayPrinter.PageWidthInches,
                Helper_LabelOverlayPrinter.PageHeightInches
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsInsideTag_ShouldBeFalse_WhenThereIsNoRoomForTheHeading()
    {
        var geometry = new Model_LabelOverlayGeometry(1.2f, 0.8f, 2f, "Lot Code");

        geometry
            .IsInsideTag(
                Helper_LabelOverlayPrinter.PageWidthInches,
                Helper_LabelOverlayPrinter.PageHeightInches
            )
            .Should()
            .BeFalse();
    }
}
