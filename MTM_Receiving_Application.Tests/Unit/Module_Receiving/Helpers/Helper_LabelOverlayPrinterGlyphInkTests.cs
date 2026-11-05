using System.Drawing;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers the ink colour of the marking glyph that is printed inside the circle. A marking is a black
/// shape, so every palette colour prints a black glyph — except the near-black one, where black ink would
/// be invisible and the shape would disappear from the tag entirely.
/// </summary>
public sealed class Helper_LabelOverlayPrinterGlyphInkTests
{
    [Fact]
    public void GlyphInkFor_ShouldPrintBlack_ForEveryPaletteColorButTheBlackOne()
    {
        var offenders = Model_LotMarkingPalette
            .Colors.Where(color => color.Code != "BLACK")
            .Where(color => Helper_LabelOverlayPrinter.GlyphInkFor(color.Code) != Color.Black)
            .Select(color => $"{color.Code} ({color.DisplayName})")
            .ToList();

        offenders.Should().BeEmpty("every marking glyph is a black silhouette");
    }

    [Fact]
    public void GlyphInkFor_ShouldPrintWhite_OnTheBlackCircle()
    {
        Helper_LabelOverlayPrinter.GlyphInkFor("BLACK").Should().Be(Color.White);
    }

    [Fact]
    public void GlyphInkFor_ShouldPrintBlack_WhenTheColorCodeIsUnknown()
    {
        Helper_LabelOverlayPrinter
            .GlyphInkFor("TEST-NOT-A-COLOR")
            .Should()
            .Be(Color.Black, "an unknown code falls back to a mid gray fill, which black ink shows on");
    }

    [Fact]
    public void GlyphInkFor_ShouldPrintBlack_WhenNoColorIsRecorded()
    {
        Helper_LabelOverlayPrinter.GlyphInkFor(null).Should().Be(Color.Black);
    }
}
