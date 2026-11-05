using System.Drawing;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers where the shape glyph actually lands inside the circle. The marking is drawn by
/// <see cref="Graphics.DrawString"/>, which positions the text's em box rather than the shape, so the ink
/// has to be measured and the drawing origin offset — and only a rendered circle can prove that worked.
/// </summary>
public sealed class Helper_LabelOverlayPrinterGlyphTests
{
    private const int Dpi = 300;
    private const float Diameter = 1.2f;
    private const float CentreY = 1f;

    [Fact]
    public void DrawOverlay_ShouldCentreEveryShapeOnTheCircle()
    {
        var shapes = Model_LotMarkingPalette.Shapes;

        using var page = new Bitmap((int)(shapes.Count * 1.55f * Dpi), (int)(2f * Dpi));
        page.SetResolution(Dpi, Dpi);

        using (var graphics = Graphics.FromImage(page))
        {
            graphics.Clear(Color.White);
            graphics.PageUnit = GraphicsUnit.Inch;

            for (var index = 0; index < shapes.Count; index++)
            {
                Helper_LabelOverlayPrinter.DrawOverlay(
                    graphics,
                    new Model_LabelOverlayGeometry(
                        0.85f + (index * 1.55f),
                        CentreY,
                        Diameter,
                        string.Empty
                    ),
                    shapes[index].Glyph,
                    "BLUE"
                );
            }
        }

        for (var index = 0; index < shapes.Count; index++)
        {
            var centreX = 0.85f + (index * 1.55f);
            var ink = FindInkBox(page, centreX, CentreY);

            ink.Should().NotBeNull($"'{shapes[index].DisplayName}' must draw something");

            var offsetX = ((ink!.Value.Left + (ink.Value.Width / 2f)) - (centreX * Dpi)) / Dpi;
            var offsetY = ((ink.Value.Top + (ink.Value.Height / 2f)) - (CentreY * Dpi)) / Dpi;

            // A fiftieth of an inch is under half a millimetre on a two-inch marking, and well inside what
            // a printed shape needs to look centred.
            offsetX.Should().BeInRange(-0.02f, 0.02f, $"'{shapes[index].DisplayName}' across");
            offsetY.Should().BeInRange(-0.02f, 0.02f, $"'{shapes[index].DisplayName}' down");
        }
    }

    [Fact]
    public void DrawOverlay_ShouldGiveEveryShapeTheSameVisualSize()
    {
        var shapes = Model_LotMarkingPalette.Shapes;

        using var page = new Bitmap((int)(shapes.Count * 1.55f * Dpi), (int)(2f * Dpi));
        page.SetResolution(Dpi, Dpi);

        using (var graphics = Graphics.FromImage(page))
        {
            graphics.Clear(Color.White);
            graphics.PageUnit = GraphicsUnit.Inch;

            for (var index = 0; index < shapes.Count; index++)
            {
                Helper_LabelOverlayPrinter.DrawOverlay(
                    graphics,
                    new Model_LabelOverlayGeometry(
                        0.85f + (index * 1.55f),
                        CentreY,
                        Diameter,
                        string.Empty
                    ),
                    shapes[index].Glyph,
                    "BLUE"
                );
            }
        }

        for (var index = 0; index < shapes.Count; index++)
        {
            var centreX = 0.85f + (index * 1.55f);
            var ink = FindInkBox(page, centreX, CentreY)!.Value;
            var longestEdge = Math.Max(ink.Width, ink.Height) / Dpi;

            longestEdge
                .Should()
                .BeInRange(
                    Diameter * 0.5f,
                    Diameter * 0.7f,
                    $"'{shapes[index].DisplayName}' should read at the same weight as the rest"
                );
        }
    }

    /// <summary>
    /// The box the dark ink occupies inside one circle, in page pixels, ignoring the ring and the blue fill.
    /// </summary>
    private static RectangleF? FindInkBox(Bitmap page, float centreXInches, float centreYInches)
    {
        var centreX = centreXInches * Dpi;
        var centreY = centreYInches * Dpi;
        var limit = (Diameter / 2f) * Dpi * 0.93f;

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        for (var y = (int)(centreY - limit); y <= (int)(centreY + limit); y++)
        {
            for (var x = (int)(centreX - limit); x <= (int)(centreX + limit); x++)
            {
                var dx = x - centreX;
                var dy = y - centreY;

                if ((dx * dx) + (dy * dy) > limit * limit)
                {
                    continue;
                }

                var pixel = page.GetPixel(x, y);

                // The circle's fill and the white page are both brighter than the ink, so a plain
                // brightness test tells the shape apart from everything else it is drawn on.
                if (((pixel.R + pixel.G + pixel.B) / 3) > 90)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }
        }

        return maxX >= minX && maxY >= minY
            ? new RectangleF(minX, minY, maxX - minX + 1, maxY - minY + 1)
            : null;
    }
}
