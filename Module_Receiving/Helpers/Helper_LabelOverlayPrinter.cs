using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// Where the marking lands on the receiving tag, measured in inches from the tag's top-left corner.
/// </summary>
/// <param name="CenterX">Circle centre, across from the left edge.</param>
/// <param name="CenterY">Circle centre, down from the top edge.</param>
/// <param name="Diameter">Circle diameter.</param>
/// <param name="Heading">The underlined heading above the circle.</param>
public sealed record Model_LabelOverlayGeometry(
    float CenterX,
    float CenterY,
    float Diameter,
    string Heading
)
{
    /// <summary>
    /// The geometry the tag's own artwork leaves room for: a two-inch circle in the blank gutter left
    /// of the ID block, which is where the reference sheet shows it.
    /// </summary>
    public static Model_LabelOverlayGeometry Default { get; } =
        new(1.185f, 4.075f, 1.975f, "Lot Code");

    /// <summary>
    /// True when the circle sits inside the tag with room for the heading above it and the caption
    /// below, so a mistyped offset cannot print off the edge of the stock.
    /// </summary>
    public bool IsInsideTag(float pageWidth, float pageHeight) =>
        Diameter > 0.2f
        && CenterX - (Diameter / 2f) >= 0f
        && CenterY - (Diameter / 2f) >= 0.6f
        && CenterX + (Diameter / 2f) <= pageWidth
        && CenterY + (Diameter / 2f) + 0.6f <= pageHeight;
}

/// <summary>
/// Prints the lot marking onto a receiving tag that was already printed. Only the marking is drawn, in
/// the blank gutter the tag leaves free, so a sheet of pre-printed tags can be fed back through the
/// printer and the marking added to each one.
/// </summary>
/// <remarks>
/// The tag is 11 x 8.5 inches, landscape, and every position here is in inches measured from its
/// top-left corner so the numbers can be checked against a printed sheet with a rule.
/// The circle is filled with the lot's palette color and ringed in black, the shape glyph is drawn
/// black inside it, and the color and shape are written under it in words ("Red Star") so the marking
/// still reads correctly on a printer that renders color poorly.
/// </remarks>
public static class Helper_LabelOverlayPrinter
{
    /// <summary>The receiving tag's width, in inches.</summary>
    public const float PageWidthInches = 11f;

    /// <summary>The receiving tag's height, in inches.</summary>
    public const float PageHeightInches = 8.5f;

    /// <summary>How many cards a cut-out sheet holds across.</summary>
    public const int CardColumns = 4;

    /// <summary>How many cards a cut-out sheet holds down.</summary>
    public const int CardRows = 2;

    /// <summary>How many cards fit on one cut-out sheet, which is what a page of eight means.</summary>
    public static int CardsPerSheet => CardColumns * CardRows;

    private const string GlyphFontFamily = "Segoe UI Symbol";
    private const string TextFontFamily = "Segoe UI";

    /// <summary>The point size the glyph ink is probed at.</summary>
    private const float GlyphProbeEmPoints = 96f;

    /// <summary>
    /// The resolution the ink probe is rendered at. High enough that the probe's grid fitting is a small
    /// fraction of the glyph, so the ink it measures matches the ink a much larger printed glyph lays down.
    /// </summary>
    private const int GlyphProbeDpi = 300;

    /// <summary>
    /// Where each glyph's ink sits, in probe pixels from the drawing origin, so the shape can be centred
    /// by what it actually draws rather than by the box the font reserves for it.
    /// </summary>
    private static readonly ConcurrentDictionary<
        string,
        RectangleF?
    > GlyphInkProbes = new(StringComparer.Ordinal);

    /// <summary>
    /// The circle's border and the heading's underline, in points. Pen widths are in page units, and the
    /// page is measured in inches, so these are divided by 72 before use — otherwise a "3 point" line
    /// would be three inches wide.
    /// </summary>
    private const float BorderWidthPoints = 3f;

    private const float UnderlineWidthPoints = 2f;

    /// <summary>The scissor line between two cards, in points.</summary>
    private const float CutLineWidthPoints = 1f;

    /// <summary>
    /// Room inside a card, in inches, that no text or artwork may enter. It keeps a cut-out from
    /// clipping the part number that sits above it.
    /// </summary>
    private const float CardPaddingInches = 0.14f;

    /// <summary>
    /// A hair of space kept between the scissor lines and the edge of what the printer can reach, so a
    /// one-point dashed line is not halved by the clip boundary.
    /// </summary>
    private const float CutClearanceInches = 0.02f;

    /// <summary>
    /// Below this the resolved cut-out area is not worth drawing into, and the requested margin is used
    /// plainly instead of the printer's reported reach.
    /// </summary>
    private const float MinimumCardSheetInches = 2f;

    /// <summary>
    /// How far a printer's media may fall short of the page the layout needs before it counts as the wrong
    /// paper. A hundredth of an inch absorbs the rounding in what drivers report.
    /// </summary>
    private const float MediaToleranceInches = 0.01f;

    /// <summary>
    /// The gap between the circle's bottom edge and the words under it, as a fraction of the diameter.
    /// Small, because the tag's own description line starts just under the circle's gutter and a wider
    /// gap crowds it.
    /// </summary>
    private const float CaptionGapFraction = 4f / 100f;

    /// <summary>
    /// How much of the circle's diameter the shape's own ink may take, measured across its larger edge.
    /// The ink rather than the em box, because an em box carries leading that would otherwise be counted
    /// as part of the shape and shrink it.
    /// </summary>
    private const float GlyphInkFraction = 60f / 100f;

    /// <summary>
    /// The words written under the circle, e.g. "Red Star". Falls back to whichever half is known, and
    /// to an empty string when neither is, so a lot with no marking prints nothing rather than a
    /// meaningless caption.
    /// </summary>
    public static string DescribeMarking(string? colorCode, string? shapeCode)
    {
        var colorName = Model_LotMarkingPalette.FindColor(colorCode)?.DisplayName ?? string.Empty;
        var shapeName = Model_LotMarkingPalette.FindShape(shapeCode)?.DisplayName ?? string.Empty;

        if (colorName.Length == 0)
        {
            return shapeName;
        }

        return shapeName.Length == 0 ? colorName : $"{colorName} {shapeName}";
    }

    /// <summary>
    /// The brush that fills the circle. A color code the palette no longer offers still prints a
    /// visible circle, in gray, rather than an invisible one.
    /// </summary>
    public static Color ResolveFillColor(string? colorCode)
    {
        var color = Model_LotMarkingPalette.FindColor(colorCode);

        return color is null ? Color.FromArgb(128, 134, 139) : ParseHex(color.HexToken);
    }

    /// <summary>Every printer this machine can print to, sorted for a picker.</summary>
    public static IReadOnlyList<string> GetInstalledPrinters()
    {
        var names = new List<string>();

        foreach (string name in PrinterSettings.InstalledPrinters)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                names.Add(name.Trim());
            }
        }

        names.Sort(StringComparer.CurrentCultureIgnoreCase);
        return names;
    }

    /// <summary>
    /// The paper sizes a printer offers, with their edges reduced to inches. Read from the driver rather
    /// than assumed, because a paper size the driver does not know is silently ignored when printing.
    /// </summary>
    public static IReadOnlyList<Model_LabelOverlayPaperOption> GetPaperSizes(string? printerName)
    {
        var options = new List<Model_LabelOverlayPaperOption>();

        try
        {
            using var document = new PrintDocument();

            if (!string.IsNullOrWhiteSpace(printerName))
            {
                document.PrinterSettings.PrinterName = printerName.Trim();
            }

            if (!document.PrinterSettings.IsValid)
            {
                return options;
            }

            foreach (PaperSize paper in document.PrinterSettings.PaperSizes)
            {
                var width = paper.Width / 100f;
                var height = paper.Height / 100f;

                if (width <= 0f || height <= 0f)
                {
                    continue;
                }

                options.Add(
                    new Model_LabelOverlayPaperOption(
                        paper.PaperName,
                        Math.Min(width, height),
                        Math.Max(width, height)
                    )
                );
            }
        }
        catch (Exception ex) when (ex is InvalidPrinterException or SystemException)
        {
            return options;
        }

        return options
            .GroupBy(option => option.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(option => option.HeightInches)
            .ThenBy(option => option.WidthInches)
            .ToList();
    }

    /// <summary>
    /// The trays a printer offers, read from the driver rather than assumed, because a tray number the
    /// driver does not know is ignored and the job then feeds from the printer's default.
    /// </summary>
    public static IReadOnlyList<Model_LabelOverlayPaperSourceOption> GetPaperSources(
        string? printerName
    )
    {
        var options = new List<Model_LabelOverlayPaperSourceOption>();

        try
        {
            using var document = new PrintDocument();

            if (!string.IsNullOrWhiteSpace(printerName))
            {
                document.PrinterSettings.PrinterName = printerName.Trim();
            }

            if (!document.PrinterSettings.IsValid)
            {
                return options;
            }

            foreach (PaperSource source in document.PrinterSettings.PaperSources)
            {
                options.Add(
                    new Model_LabelOverlayPaperSourceOption(source.SourceName, (short)source.RawKind)
                );
            }
        }
        catch (Exception ex) when (ex is InvalidPrinterException or SystemException)
        {
            return options;
        }

        return options
            .Where(option => option.RawKind != 0)
            .GroupBy(option => option.RawKind)
            .Select(group => group.First())
            .OrderBy(option => option.RawKind)
            .ToList();
    }

    /// <summary>
    /// The paper size to default to for a printer: Letter when the driver offers it, because that is what
    /// the receiving tags are printed on, otherwise the first size it reports.
    /// </summary>
    public static Model_LabelOverlayPaperOption? FindDefaultPaper(
        IReadOnlyList<Model_LabelOverlayPaperOption> papers
    )
    {
        ArgumentNullException.ThrowIfNull(papers);

        return papers.FirstOrDefault(paper =>
                Math.Abs(paper.WidthInches - Model_LabelOverlayPrintSettings.DefaultPaperWidthInches)
                    < 0.02f
                && Math.Abs(paper.HeightInches - Model_LabelOverlayPrintSettings.DefaultPaperHeightInches)
                    < 0.02f
            ) ?? papers.FirstOrDefault();
    }

    /// <summary>
    /// Draws one marking onto <paramref name="graphics"/>, whose origin is the tag's top-left corner and
    /// whose units are inches.
    /// </summary>
    public static void DrawOverlay(
        Graphics graphics,
        Model_LabelOverlayGeometry geometry,
        string? shapeCode,
        string? colorCode
    )
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(geometry);

        var previousSmoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        try
        {
            var radius = geometry.Diameter / 2f;
            var circle = new RectangleF(
                geometry.CenterX - radius,
                geometry.CenterY - radius,
                geometry.Diameter,
                geometry.Diameter
            );

            DrawCircle(graphics, circle, shapeCode, colorCode);
            DrawCentredText(
                graphics,
                DescribeMarking(colorCode, shapeCode),
                geometry.CenterX,
                geometry.CenterY + radius + (geometry.Diameter * CaptionGapFraction),
                CaptionEmSize(geometry.Diameter)
            );
            DrawHeading(graphics, geometry);
        }
        finally
        {
            graphics.SmoothingMode = previousSmoothing;
        }
    }

    /// <summary>
    /// Where the cards sit on a cut-out sheet, in inches from the drawable area's top-left corner. The
    /// tiles share edges so the cut lines between them are single lines rather than a pair a hair apart.
    /// </summary>
    /// <param name="pageWidth">The drawable width, in inches.</param>
    /// <param name="pageHeight">The drawable height, in inches.</param>
    /// <param name="columns">Cards across; four by default.</param>
    /// <param name="rows">Cards down; two by default.</param>
    public static IReadOnlyList<RectangleF> BuildCardRects(
        float pageWidth,
        float pageHeight,
        int columns = CardColumns,
        int rows = CardRows
    )
    {
        if (pageWidth <= 0f || pageHeight <= 0f || columns <= 0 || rows <= 0)
        {
            return [];
        }

        var cardWidth = pageWidth / columns;
        var cardHeight = pageHeight / rows;
        var tiles = new List<RectangleF>(columns * rows);

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                tiles.Add(
                    new RectangleF(column * cardWidth, row * cardHeight, cardWidth, cardHeight)
                );
            }
        }

        return tiles;
    }

    /// <summary>
    /// Where the cut-out grid is allowed to draw, in inches measured from the page's top-left corner.
    /// </summary>
    /// <remarks>
    /// A page rectangle the driver says it can reach does not mean it will print there: every printer has
    /// a hard margin it cannot mark, and content laid over it is silently clipped. That clipping is what
    /// dropped the right and bottom scissor lines when the grid was drawn to the page edge. This takes the
    /// printable area the driver reports, backs off by the requested margin and a hair of clearance, and
    /// refuses to hand back anything the printer cannot actually put ink on.
    /// The printable area is only believed when it agrees with the page it was reported for. Several
    /// drivers — the Microsoft PDF printer among them — answer with the paper's own orientation even when
    /// the job is landscape, and a landscape sheet taking an unrotated answer at face value comes back
    /// 8.5 inches wide instead of 11, which is worse than the clipping it was meant to prevent.
    /// </remarks>
    /// <param name="pageBounds">The page rect in hundredths of an inch, as the print settings report it.</param>
    /// <param name="printableArea">The printable rect in hundredths of an inch, as the driver reports it.</param>
    /// <param name="marginInches">The trimming margin the user asked for, in inches.</param>
    public static RectangleF ResolveCardSheetArea(
        RectangleF pageBounds,
        RectangleF printableArea,
        float marginInches
    )
    {
        var pageWidth = pageBounds.Width / 100f;
        var pageHeight = pageBounds.Height / 100f;

        if (pageWidth <= 0f || pageHeight <= 0f)
        {
            pageWidth = PageWidthInches;
            pageHeight = PageHeightInches;
        }

        var inset = Math.Clamp(marginInches, 0f, 2f);

        // Failing a believable answer, the driver is taken at its word that it can reach the whole page.
        // The requested inset still applies, which is the ordinary case on a plain laser printer.
        var reachable = IsBelievable(printableArea, pageBounds)
            ? printableArea
            : new RectangleF(pageBounds.X, pageBounds.Y, pageWidth * 100f, pageHeight * 100f);

        var left = Math.Max(inset, (reachable.Left / 100f) + CutClearanceInches);
        var top = Math.Max(inset, (reachable.Top / 100f) + CutClearanceInches);
        var right = Math.Min(pageWidth - inset, (reachable.Right / 100f) - CutClearanceInches);
        var bottom = Math.Min(pageHeight - inset, (reachable.Bottom / 100f) - CutClearanceInches);

        if (
            right - left < MinimumCardSheetInches
            || bottom - top < MinimumCardSheetInches
        )
        {
            left = inset;
            top = inset;
            right = pageWidth - inset;
            bottom = pageHeight - inset;
        }

        return new RectangleF(
            left,
            top,
            Math.Max(right - left, 0f),
            Math.Max(bottom - top, 0f)
        );
    }

    /// <summary>
    /// The same area, narrowed to what the page being drawn on can actually be marked on.
    /// </summary>
    /// <remarks>
    /// This is what makes the sheet fit the printed page: the drawable area the live graphics reports is
    /// the page the driver is about to put ink on, and it is a size the grid is then stretched to fill. A
    /// driver that reports one printable area and clips at another can no longer leave the grid short of
    /// the right edge, and the area is kept a hair inside the clip so the outer borders are not halved.
    /// </remarks>
    /// <param name="pageBounds">The page rect in hundredths of an inch, as the print settings report it.</param>
    /// <param name="printableArea">The printable rect in hundredths of an inch, as the driver reports it.</param>
    /// <param name="visibleClipInches">The live graphics clip, in inches from the page's corner.</param>
    /// <param name="marginInches">The trimming margin the user asked for, in inches.</param>
    public static RectangleF ResolveCardSheetArea(
        RectangleF pageBounds,
        RectangleF printableArea,
        RectangleF visibleClipInches,
        float marginInches
    )
    {
        var area = ResolveCardSheetArea(pageBounds, printableArea, marginInches);

        if (visibleClipInches.Width <= 0f || visibleClipInches.Height <= 0f)
        {
            return area;
        }

        var left = Math.Max(area.Left, visibleClipInches.Left + CutClearanceInches);
        var top = Math.Max(area.Top, visibleClipInches.Top + CutClearanceInches);
        var right = Math.Min(area.Right, visibleClipInches.Right - CutClearanceInches);
        var bottom = Math.Min(area.Bottom, visibleClipInches.Bottom - CutClearanceInches);

        if (right - left < MinimumCardSheetInches || bottom - top < MinimumCardSheetInches)
        {
            return area;
        }

        return new RectangleF(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Whether a reported printable area describes the same sheet the job is going to print on: it has a
    /// size, it fits inside the page, and it is the same way round.
    /// </summary>
    private static bool IsBelievable(RectangleF printableArea, RectangleF pageBounds)
    {
        if (printableArea.Width <= 0f || printableArea.Height <= 0f)
        {
            return false;
        }

        var fitsThePage =
            printableArea.Right <= pageBounds.Right + 1f
            && printableArea.Bottom <= pageBounds.Bottom + 1f
            && printableArea.Left >= pageBounds.Left - 1f
            && printableArea.Top >= pageBounds.Top - 1f;

        if (!fitsThePage)
        {
            return false;
        }

        return (printableArea.Width > printableArea.Height) == (pageBounds.Width > pageBounds.Height);
    }

    /// <summary>
    /// The cards one sheet carries, in tile order. A sheet is one lot's page: the same card in every tile,
    /// so cutting it up gives eight identical cards rather than a sheet of mixed lots that cannot be told
    /// apart once it is cut.
    /// </summary>
    /// <param name="card">The card this sheet is for.</param>
    /// <param name="tileCount">How many cards the sheet holds; eight when omitted.</param>
    public static IReadOnlyList<Model_LabelOverlayCard> BuildSheetCards(
        Model_LabelOverlayCard card,
        int tileCount = 0
    )
    {
        ArgumentNullException.ThrowIfNull(card);

        var tiles = tileCount > 0 ? tileCount : CardsPerSheet;

        return tiles <= 0 ? [] : Enumerable.Repeat(card, tiles).ToList();
    }

    /// <summary>
    /// How many sheets a run needs: one full page per selected card, repeated as many times as the
    /// operator asked for duplicates.
    /// </summary>
    /// <param name="cardCount">How many lots the run holds.</param>
    /// <param name="duplicates">How many pages each lot gets.</param>
    public static int CardSheetPageCount(int cardCount, int duplicates) =>
        cardCount <= 0 ? 1 : cardCount * Math.Max(1, duplicates);

    /// <summary>
    /// Which lot a given sheet is for. Lots are printed one after another, each on as many pages as
    /// duplicates were asked for, so a sheet never carries two lots.
    /// </summary>
    /// <param name="cards">Every card the run is printing, in the order they were selected.</param>
    /// <param name="sheetIndex">The sheet being drawn, from zero.</param>
    /// <param name="duplicates">How many pages each lot gets.</param>
    public static Model_LabelOverlayCard CardForSheet(
        IReadOnlyList<Model_LabelOverlayCard> cards,
        int sheetIndex,
        int duplicates
    )
    {
        ArgumentNullException.ThrowIfNull(cards);

        if (cards.Count == 0)
        {
            throw new ArgumentException("A sheet needs at least one card.", nameof(cards));
        }

        var perLot = Math.Max(1, duplicates);
        var lot = Math.Min(Math.Max(0, sheetIndex) / perLot, cards.Count - 1);

        return cards[lot];
    }

    /// <summary>
    /// The outline drawn around each card, pulled in by half a pen width so no part of the stroke falls
    /// outside the page and gets clipped away.
    /// </summary>
    /// <param name="pageWidth">The drawable width, in inches.</param>
    /// <param name="pageHeight">The drawable height, in inches.</param>
    public static IReadOnlyList<RectangleF> BuildCardOutlines(float pageWidth, float pageHeight)
    {
        var inset = CutLineWidthPoints / 72f / 2f;

        return BuildCardRects(pageWidth, pageHeight)
            .Select(tile => new RectangleF(
                tile.Left + inset,
                tile.Top + inset,
                Math.Max(tile.Width - (2f * inset), 0f),
                Math.Max(tile.Height - (2f * inset), 0f)
            ))
            .ToList();
    }

    /// <summary>
    /// Draws one page of cut-out cards: every tile carrying the same card, then the scissor lines around
    /// each one.
    /// </summary>
    /// <param name="graphics">Origin at the drawable area's top-left corner; units are inches.</param>
    /// <param name="pageWidth">The drawable width, in inches.</param>
    /// <param name="pageHeight">The drawable height, in inches.</param>
    /// <param name="card">The lot this page is for.</param>
    public static void DrawCardSheet(
        Graphics graphics,
        float pageWidth,
        float pageHeight,
        Model_LabelOverlayCard card
    )
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(card);

        var previousSmoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        try
        {
            var tiles = BuildCardRects(pageWidth, pageHeight);

            for (var index = 0; index < tiles.Count; index++)
            {
                DrawCard(graphics, tiles[index], card);
            }

            DrawCutLines(graphics, pageWidth, pageHeight);
        }
        finally
        {
            graphics.SmoothingMode = previousSmoothing;
        }
    }

    /// <summary>
    /// One card: the part across the top, the marking in the middle, the lot along the bottom. No heading
    /// sits above the circle here — the card is being cut out and stuck on something, so the words under
    /// the circle are the only label it needs.
    /// </summary>
    private static void DrawCard(
        Graphics graphics,
        RectangleF tile,
        Model_LabelOverlayCard card
    )
    {
        var centreX = tile.Left + (tile.Width / 2f);
        var centreY = tile.Top + (tile.Height / 2f);

        var partEm = tile.Width * 9f / 100f * 72f;
        var captionEm = tile.Width * 5.5f / 100f * 72f;
        var lotEm = tile.Width * 8f / 100f * 72f;

        // The circle is sized to the card, not to the tag: it takes most of the width but leaves the part
        // and lot their own lines.
        var diameter = Math.Min(
            tile.Width - (2f * CardPaddingInches) - 0.1f,
            tile.Height * 42f / 100f
        );
        diameter = Math.Max(diameter, 0.25f);

        var circle = new RectangleF(
            centreX - (diameter / 2f),
            centreY - (diameter / 2f),
            diameter,
            diameter
        );

        DrawCentredText(
            graphics,
            card.PartId,
            centreX,
            tile.Top + CardPaddingInches,
            partEm,
            tile.Width - (2f * CardPaddingInches)
        );

        DrawCircle(graphics, circle, card.ShapeCode, card.ColorCode);

        DrawCentredText(
            graphics,
            DescribeMarking(card.ColorCode, card.ShapeCode),
            centreX,
            centreY + (diameter / 2f) + 0.08f,
            captionEm,
            tile.Width - (2f * CardPaddingInches)
        );

        DrawCentredText(
            graphics,
            card.LotNumber,
            centreX,
            tile.Bottom - CardPaddingInches - (lotEm / 72f),
            lotEm,
            tile.Width - (2f * CardPaddingInches)
        );
    }

    /// <summary>
    /// The scissor lines: a full box around every card, so each one can be cut out on all four sides.
    /// </summary>
    /// <remarks>
    /// Every card is boxed in its own right rather than the sheet being crossed by grid lines. A grid line
    /// drawn at the very edge of the page is half outside the area the printer can mark, and that half is
    /// what went missing, leaving cards with no border on the outer sides. The outlines are pulled inside
    /// the page by half a pen width instead, so all four sides survive whatever the printer clips.
    /// </remarks>
    private static void DrawCutLines(Graphics graphics, float pageWidth, float pageHeight)
    {
        using var pen = new Pen(Color.FromArgb(90, 90, 90), CutLineWidthPoints / 72f)
        {
            DashStyle = DashStyle.Dash,
        };

        foreach (var outline in BuildCardOutlines(pageWidth, pageHeight))
        {
            graphics.DrawRectangle(pen, outline.X, outline.Y, outline.Width, outline.Height);
        }
    }

    /// <summary>
    /// The filled circle: palette fill, black ring, and the marking glyph in black ink. Shared by the tag
    /// overlay and the cut-out cards so the two can never drift apart.
    /// </summary>
    private static void DrawCircle(
        Graphics graphics,
        RectangleF circle,
        string? shapeCode,
        string? colorCode
    )
    {
        using (var fill = new SolidBrush(ResolveFillColor(colorCode)))
        {
            graphics.FillEllipse(fill, circle);
        }

        using (var border = new Pen(Color.Black, BorderWidthPoints / 72f))
        {
            graphics.DrawEllipse(border, circle);
        }

        DrawGlyph(graphics, circle, shapeCode, colorCode);
    }

    /// <summary>
    /// Prints one page per label. A label that carries no marking is skipped, because a circle with no
    /// shape in it would say less than the blank tag already does.
    /// </summary>
    /// <param name="printerName">The printer to send to.</param>
    /// <param name="markings">The shape and color recorded against each chosen tag.</param>
    /// <param name="geometry">Where the circle lands on the tag, in inches.</param>
    /// <param name="printSettings">The saved print setup; the tag defaults when omitted.</param>
    /// <param name="progress">Told about each page as the printer takes it, so the screen can show progress.</param>
    public static Model_Dao_Result Print(
        string printerName,
        IReadOnlyList<(string ShapeCode, string ColorCode)> markings,
        Model_LabelOverlayGeometry geometry,
        Model_LabelOverlayPrintSettings? printSettings = null,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        var created = CreateTagDocument(printerName, markings, geometry, printSettings, progress);

        if (created.Document is null)
        {
            return created.Failure!;
        }

        using var document = created.Document;

        try
        {
            document.Print();
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"The overlay could not be printed: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Builds the tag-overlay job without running it, so it can be previewed and then printed from the
    /// same document. One page per label; a label with no marking is skipped, because a circle with no
    /// shape in it would say less than the blank tag already does.
    /// </summary>
    /// <param name="printerName">The printer the job will go to.</param>
    /// <param name="markings">The shape and color recorded against each chosen tag.</param>
    /// <param name="geometry">Where the circle lands on the tag, in inches.</param>
    /// <param name="printSettings">The saved print setup; the tag defaults when omitted.</param>
    /// <param name="progress">Told about each page as the printer takes it.</param>
    /// <returns>The job, or the reason it cannot be built.</returns>
    public static (PrintDocument? Document, Model_Dao_Result? Failure) CreateTagDocument(
        string printerName,
        IReadOnlyList<(string ShapeCode, string ColorCode)> markings,
        Model_LabelOverlayGeometry geometry,
        Model_LabelOverlayPrintSettings? printSettings = null,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var settings = printSettings ?? Model_LabelOverlayPrintSettings.CreateDefault();

        var printable = (markings ?? Array.Empty<(string, string)>())
            .Where(marking =>
                !string.IsNullOrWhiteSpace(marking.ShapeCode)
                || !string.IsNullOrWhiteSpace(marking.ColorCode)
            )
            .ToList();

        if (printable.Count == 0)
        {
            return (
                null,
                Model_Dao_Result_Factory.Failure(
                    "None of the selected labels carries a marking, so there is nothing to overlay."
                )
            );
        }

        var page = settings.ResolvePageSize();

        if (!geometry.IsInsideTag(page.Width, page.Height))
        {
            return (
                null,
                Model_Dao_Result_Factory.Failure(
                    $"The marking would print off the page. A {geometry.Diameter:0.##}\" circle at "
                        + $"({geometry.CenterX:0.###}\", {geometry.CenterY:0.###}\") does not fit the "
                        + $"{page.Width:0.##}\" x {page.Height:0.##}\" area the print setup gives it."
                        + (
                            settings.Landscape
                                ? string.Empty
                                : " Set the print setup to landscape to print tags."
                        )
                )
            );
        }

        var printerFailure = ValidatePrinter(printerName);
        if (printerFailure is not null)
        {
            return (null, printerFailure);
        }

        try
        {
            var document = new PrintDocument();
            document.PrinterSettings.PrinterName = printerName.Trim();
            document.DocumentName = "Receiving tag lot marking";
            document.PrintController = new StandardPrintController();

            // The printer's own tray, duplex and quality choices for this output, applied to this job
            // only. Paper and orientation are still set below, so the tag layout cannot be changed by a
            // stale driver block.
            Helper_PrinterDevMode.TryApply(document.PrinterSettings, settings.GetDevMode());

            // Copies are asked for after the driver block, so the count the print dialog was given wins
            // over whatever copy count the saved settings happened to carry.
            document.PrinterSettings.Copies = (short)settings.ResolveCopies();

            // The drawing origin is left at its default. Measured against both receiving printers and the
            // PDF device, GDI+ then maps page inches one-to-one onto the page's own corner, which is what
            // the measured geometry is relative to. OriginAtMargins, and compensating for the printer's
            // hard margin by hand, both move the origin again and put the marking 0.16" out.
            document.OriginAtMargins = false;
            document.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            var media = ApplyPageSetup(document, settings);

            if (!media.IsUsable)
            {
                document.Dispose();
                return (null, media.Failure!);
            }

            var index = 0;

            // A preview renders the pages and then the job renders them again, so the count starts over
            // at the beginning of every pass.
            document.BeginPrint += (_, _) => index = 0;

            document.PrintPage += (_, args) =>
            {
                var graphics = args.Graphics;
                if (graphics is null)
                {
                    args.HasMorePages = false;
                    return;
                }

                graphics.PageUnit = GraphicsUnit.Inch;

                var marking = printable[index];
                DrawOverlay(graphics, geometry, marking.ShapeCode, marking.ColorCode);

                index++;

                // Reported as the page is handed over, which is the printer's own pace rather than a timer.
                progress?.Report(new Model_LabelOverlayPrintProgress(index, printable.Count));

                args.HasMorePages = index < printable.Count;
            };

            return (document, null);
        }
        catch (Exception ex)
        {
            return (
                null,
                Model_Dao_Result_Factory.Failure(
                    $"The overlay could not be prepared: {ex.Message}",
                    ex
                )
            );
        }
    }

    /// <summary>
    /// Prints the cut-out sheet: eight cards to a page, four across and two down, each carrying the part
    /// number, the marking and the lot, with scissor lines between them.
    /// </summary>
    /// <param name="printerName">The printer to send to.</param>
    /// <param name="cards">The cards to print; anything past eight continues on the next page.</param>
    /// <param name="printSettings">The saved print setup; the tag defaults when omitted.</param>
    /// <param name="progress">Told about each sheet as the printer takes it.</param>
    public static Model_Dao_Result PrintCardSheet(
        string printerName,
        IReadOnlyList<Model_LabelOverlayCard> cards,
        Model_LabelOverlayPrintSettings? printSettings = null,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        var created = CreateCardSheetDocument(printerName, cards, printSettings, progress);

        if (created.Document is null)
        {
            return created.Failure!;
        }

        using var document = created.Document;

        try
        {
            document.Print();
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"The card sheet could not be printed: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Builds the cut-out card sheet job without running it, so the same document can be previewed and
    /// then printed: eight cards to a page, four across and two down, each carrying the part number, the
    /// marking and the lot, with a border round every card.
    /// </summary>
    /// <param name="printerName">The printer the job will go to.</param>
    /// <param name="cards">The cards to print; anything past eight continues on the next page.</param>
    /// <param name="printSettings">The saved print setup; the tag defaults when omitted.</param>
    /// <param name="progress">Told about each sheet as the printer takes it.</param>
    /// <returns>The job, or the reason it cannot be built.</returns>
    public static (PrintDocument? Document, Model_Dao_Result? Failure) CreateCardSheetDocument(
        string printerName,
        IReadOnlyList<Model_LabelOverlayCard> cards,
        Model_LabelOverlayPrintSettings? printSettings = null,
        IProgress<Model_LabelOverlayPrintProgress>? progress = null
    )
    {
        ArgumentNullException.ThrowIfNull(cards);

        var settings = printSettings ?? Model_LabelOverlayPrintSettings.CreateDefault();

        var printable = cards
            .Where(card =>
                !string.IsNullOrWhiteSpace(card.ShapeCode)
                || !string.IsNullOrWhiteSpace(card.ColorCode)
            )
            .ToList();

        if (printable.Count == 0)
        {
            return (
                null,
                Model_Dao_Result_Factory.Failure(
                    "None of the selected labels carries a marking, so the card sheet would be blank."
                )
            );
        }

        var printerFailure = ValidatePrinter(printerName);
        if (printerFailure is not null)
        {
            return (null, printerFailure);
        }

        try
        {
            var document = new PrintDocument();
            document.PrinterSettings.PrinterName = printerName.Trim();
            document.DocumentName = "Receiving marking cards";
            document.PrintController = new StandardPrintController();

            // This output's own driver settings, applied to the job only.
            Helper_PrinterDevMode.TryApply(document.PrinterSettings, settings.GetDevMode());

            // The sheet is repeated in code, once per requested page, so the driver must not multiply it
            // again — a copy count left in the saved settings would print the run twice over.
            document.PrinterSettings.Copies = 1;

            // Same origin rule as the tag overlay: default origin, and the cut-out grid placed in page
            // inches inside an area resolved from what the printer can actually reach.
            document.OriginAtMargins = false;
            document.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            var media = ApplyPageSetup(document, settings);

            if (!media.IsUsable)
            {
                document.Dispose();
                return (null, media.Failure!);
            }

            var sheetIndex = 0;
            var duplicates = settings.ResolveCopies();
            var sheets = CardSheetPageCount(printable.Count, duplicates);

            // A preview renders the pages and then the job renders them again, so the count starts over
            // at the beginning of every pass.
            document.BeginPrint += (_, _) => sheetIndex = 0;

            document.PrintPage += (_, args) =>
            {
                var graphics = args.Graphics;
                if (graphics is null)
                {
                    args.HasMorePages = false;
                    return;
                }

                graphics.PageUnit = GraphicsUnit.Inch;

                // The live clip is the page the driver is about to mark, so the grid is sized to it and
                // fills the sheet edge to edge inside the trimming margin.
                var area = ResolveCardSheetArea(
                    args.PageSettings.Bounds,
                    args.PageSettings.PrintableArea,
                    graphics.VisibleClipBounds,
                    settings.CardSheetMarginInches
                );

                graphics.TranslateTransform(area.Left, area.Top);

                // One full page per lot, repeated as many times as duplicates were asked for: a sheet is
                // never shared between lots, because cut cards with no page to go back to cannot be told
                // apart once they are loose.
                DrawCardSheet(
                    graphics,
                    area.Width,
                    area.Height,
                    CardForSheet(printable, sheetIndex, duplicates)
                );

                sheetIndex++;

                progress?.Report(new Model_LabelOverlayPrintProgress(sheetIndex, sheets));

                args.HasMorePages = sheetIndex < sheets;
            };

            return (document, null);
        }
        catch (Exception ex)
        {
            return (
                null,
                Model_Dao_Result_Factory.Failure(
                    $"The card sheet could not be prepared: {ex.Message}",
                    ex
                )
            );
        }
    }

    /// <summary>
    /// Puts the saved tray on the job, by the driver's own number for it.
    /// </summary>
    /// <remarks>
    /// The tray is applied here, not left to the driver settings block, because several drivers ignore a
    /// tray in a saved block: the receiving room's Konica reports the printer's own default no matter what
    /// block it is handed. Setting the page source is the documented way to ask for a tray, and it survives
    /// into the job's own block, so the sheet feeds from the tray the operator chose.
    /// A tray the driver no longer offers is ignored rather than forced, so a printer that has been
    /// swapped out still prints instead of failing.
    /// </remarks>
    private static void ApplyPaperSource(
        PrintDocument document,
        Model_LabelOverlayPrintSettings settings
    )
    {
        if (settings.PaperSourceRawKind == 0)
        {
            return;
        }

        try
        {
            foreach (PaperSource candidate in document.PrinterSettings.PaperSources)
            {
                if (candidate.RawKind != settings.PaperSourceRawKind)
                {
                    continue;
                }

                document.DefaultPageSettings.PaperSource = candidate;
                return;
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            // A printer whose sources cannot be enumerated keeps the one the driver settings block named.
        }
    }

    /// <summary>
    /// Puts the saved setup onto a print job: which way round the media goes, which paper it uses, and —
    /// importantly — whether the printer actually has that paper.
    /// </summary>
    /// <remarks>
    /// The paper is always handed to the driver in its portrait form and the rotation is left to the
    /// landscape flag. Setting a landscape-shaped paper size *and* the flag asks for a quarter turn of a
    /// sheet that has already been turned, which is what prints a landscape tag as a portrait page.
    /// A paper size the driver does not list is ignored by most drivers, and the job is then laid out for
    /// a page that is not there — which comes off the printer as a blank sheet. That case is reported as
    /// a failure rather than sent.
    /// </remarks>
    private static (bool IsUsable, Model_Dao_Result? Failure) ApplyPageSetup(
        PrintDocument document,
        Model_LabelOverlayPrintSettings settings
    )
    {
        var shortEdge = Math.Min(settings.PaperWidthInches, settings.PaperHeightInches);
        var longEdge = Math.Max(settings.PaperWidthInches, settings.PaperHeightInches);

        if (shortEdge <= 0f || longEdge <= 0f)
        {
            shortEdge = Model_LabelOverlayPrintSettings.DefaultPaperWidthInches;
            longEdge = Model_LabelOverlayPrintSettings.DefaultPaperHeightInches;
        }

        var width = (int)Math.Round(shortEdge * 100f, MidpointRounding.AwayFromZero);
        var height = (int)Math.Round(longEdge * 100f, MidpointRounding.AwayFromZero);

        // Read what the printer is set up for before touching anything, so the check below compares
        // against the driver's own answer rather than against what this method just wrote.
        var loadedPage = document.PrinterSettings.DefaultPageSettings;
        var loadedWidth = Math.Max(loadedPage.PaperSize?.Width ?? 0, loadedPage.Bounds.Width);
        var loadedHeight = Math.Max(loadedPage.PaperSize?.Height ?? 0, loadedPage.Bounds.Height);

        PaperSize? driverPaper = null;

        foreach (PaperSize candidate in document.PrinterSettings.PaperSizes)
        {
            if (candidate.Width == width && candidate.Height == height)
            {
                driverPaper = candidate;
                break;
            }
        }

        document.DefaultPageSettings.Landscape = settings.Landscape;

        // A custom paper size the driver does not recognise is ignored, so prefer the driver's own entry
        // when it offers this media.
        document.DefaultPageSettings.PaperSize =
            driverPaper ?? new PaperSize(settings.PaperName, width, height);

        // Setting the paper can put the orientation back, and the tag artwork is drawn for landscape, so
        // the rotation is set again afterwards to be sure of which way round the sheet comes out.
        document.DefaultPageSettings.Landscape = settings.Landscape;

        ApplyPaperSource(document, settings);

        if (driverPaper is not null)
        {
            return (true, null);
        }

        var longestLoaded = Math.Max(loadedWidth, loadedHeight) / 100f;
        var shortestLoaded = Math.Min(loadedWidth, loadedHeight) / 100f;

        if (MediaCanHoldPage(longestLoaded, shortestLoaded, longEdge, shortEdge))
        {
            // A page big enough for the artwork, even if the driver names it something else.
            return (true, null);
        }

        var loadedDescription =
            loadedPage.PaperSize is null
                ? "no paper size it reports"
                : $"{loadedPage.PaperSize.PaperName} "
                    + $"({longestLoaded:0.##}\" x {shortestLoaded:0.##}\")";

        return (
            false,
            Model_Dao_Result_Factory.Failure(
                $"'{document.PrinterSettings.PrinterName}' does not have "
                    + $"{longEdge:0.##}\" x {shortEdge:0.##}\" media — it reports {loadedDescription}. "
                    + "Change the media in the printer setup, or choose the printer the tags come off."
            )
        );
    }

    /// <summary>
    /// Whether the media a printer is loaded with can hold the page the layout asks for.
    /// </summary>
    /// <remarks>
    /// A paper size the driver does not list is ignored, and the job is then laid out for a page that is
    /// not there — which comes off the printer as a blank sheet. This is the check that turns that into
    /// something the operator can act on.
    /// </remarks>
    /// <param name="loadedLongestEdgeInches">The long edge of the media the printer reports, in inches.</param>
    /// <param name="loadedShortestEdgeInches">The short edge of that media, in inches.</param>
    /// <param name="requiredLongestEdgeInches">The long edge the layout needs, in inches.</param>
    /// <param name="requiredShortestEdgeInches">The short edge the layout needs, in inches.</param>
    public static bool MediaCanHoldPage(
        float loadedLongestEdgeInches,
        float loadedShortestEdgeInches,
        float requiredLongestEdgeInches,
        float requiredShortestEdgeInches
    ) =>
        loadedLongestEdgeInches >= requiredLongestEdgeInches - MediaToleranceInches
        && loadedShortestEdgeInches >= requiredShortestEdgeInches - MediaToleranceInches;

    /// <summary>
    /// A plain reading of what the printer will do with a job: its paper, orientation, page bounds,
    /// printable area and the offset of the drawing origin. Written to the log before every print, because
    /// a sheet that comes out blank has to be told apart from one laid out for a page that is not loaded.
    /// </summary>
    /// <param name="printerName">The printer that would be used.</param>
    /// <param name="printSettings">The saved print setup; the defaults when omitted.</param>
    public static string DescribePageSetup(
        string printerName,
        Model_LabelOverlayPrintSettings? printSettings = null
    )
    {
        var settings = printSettings ?? Model_LabelOverlayPrintSettings.CreateDefault();

        try
        {
            using var document = new PrintDocument();
            document.PrinterSettings.PrinterName = printerName?.Trim() ?? string.Empty;

            if (!document.PrinterSettings.IsValid)
            {
                return $"'{printerName}' is not an available printer.";
            }

            var media = ApplyPageSetup(document, settings);
            var page = document.DefaultPageSettings;

            var description =
                $"printer='{document.PrinterSettings.PrinterName}' "
                + $"paper='{page.PaperSize?.PaperName}' ({page.PaperSize?.Width}x{page.PaperSize?.Height}) "
                + $"landscape={page.Landscape} bounds={page.Bounds} printable={page.PrintableArea} "
                + $"hardMargin=({page.HardMarginX / 100f:0.###}\",{page.HardMarginY / 100f:0.###}\") "
                + $"resolved={settings.ResolvePageSize().Width:0.##}\"x{settings.ResolvePageSize().Height:0.##}\" "
                + $"driverSettings={Helper_PrinterDevMode.Describe(settings.GetDevMode())}";

            return media.IsUsable
                ? description
                : $"{description} — NOT USABLE: {media.Failure?.ErrorMessage}";
        }
        catch (Exception ex)
        {
            return $"The page setup for '{printerName}' could not be read: {ex.Message}";
        }
    }

    /// <summary>
    /// The printer check shared by both print paths, so an unusable printer is refused the same way.
    /// </summary>
    private static Model_Dao_Result? ValidatePrinter(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            return Model_Dao_Result_Factory.Failure("Choose a printer before printing.");
        }

        using var probe = new PrintDocument();
        probe.PrinterSettings.PrinterName = printerName.Trim();

        return probe.PrinterSettings.IsValid
            ? null
            : Model_Dao_Result_Factory.Failure(
                $"The printer '{printerName}' is not available. Check that it is installed and online."
            );
    }

    /// <summary>
    /// Draws the shape glyph inside the circle, centred on the circle by the glyph's own ink.
    /// </summary>
    /// <remarks>
    /// <see cref="Graphics.DrawString(string, Font, Brush, PointF, StringFormat)"/> positions the text's
    /// em box, and an em box carries leading above and below the ink and side bearings either side of it.
    /// Centring that box is not centring the shape — it is why the marking sat high and left of the ring.
    /// The ink is measured once per glyph and the drawing origin is then offset by the difference, so the
    /// shape itself lands on the circle's centre and comes out the same visual size for every shape.
    /// </remarks>
    private static void DrawGlyph(
        Graphics graphics,
        RectangleF circle,
        string? shapeCode,
        string? colorCode
    )
    {
        if (string.IsNullOrWhiteSpace(shapeCode))
        {
            return;
        }

        var ink = ProbeGlyphInk(shapeCode);
        if (ink is null)
        {
            return;
        }

        var inkBox = ink.Value;
        var inkLongestEdge = Math.Max(inkBox.Width, inkBox.Height);

        // Everything the probe measured is in its own pixels and scales linearly with the font size, so a
        // single factor converts probe pixels to inches on this page — for the size and the offset alike.
        var inchesPerProbePixel = (circle.Height * GlyphInkFraction) / inkLongestEdge;

        using var font = new Font(
            GlyphFontFamily,
            GlyphProbeEmPoints * GlyphProbeDpi * inchesPerProbePixel,
            FontStyle.Regular,
            GraphicsUnit.Point
        );
        using var brush = new SolidBrush(GlyphInkFor(colorCode));
        using var format = GlyphTextFormat();

        // The probe drew from a known origin, so the ink sits at that origin plus the offset it measured.
        // The drawing origin that centres the ink is therefore the circle's centre moved back by the
        // offset and half the ink.
        var originX =
            circle.Left
            + (circle.Width / 2f)
            - ((inkBox.Left + (inkBox.Width / 2f)) * inchesPerProbePixel);
        var originY =
            circle.Top
            + (circle.Height / 2f)
            - ((inkBox.Top + (inkBox.Height / 2f)) * inchesPerProbePixel);

        graphics.DrawString(shapeCode, font, brush, new PointF(originX, originY), format);
    }

    /// <summary>
    /// The string format used both to probe a glyph's ink and to draw it, so the two agree on where the
    /// text is anchored.
    /// </summary>
    private static StringFormat GlyphTextFormat()
    {
        var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.NoClip;
        format.Alignment = StringAlignment.Near;
        format.LineAlignment = StringAlignment.Near;

        return format;
    }

    /// <summary>
    /// Measures where a glyph's ink sits relative to the point it is drawn from, in probe pixels. The
    /// result is cached because the palette is fixed and the answer only depends on the font.
    /// </summary>
    /// <returns>The ink box, or <c>null</c> when the glyph draws nothing at all.</returns>
    private static RectangleF? ProbeGlyphInk(string glyph)
    {
        if (GlyphInkProbes.TryGetValue(glyph, out var cached))
        {
            return cached;
        }

        RectangleF? ink = null;

        const int probeSize = 800;
        const float probeOrigin = 120f;

        try
        {
            using var bitmap = new Bitmap(probeSize, probeSize);
            bitmap.SetResolution(GlyphProbeDpi, GlyphProbeDpi);

            // The graphics is released before the pixels are read, because GDI+ does not guarantee that
            // its drawing has reached the bitmap while the graphics is still open.
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                using var font = new Font(
                    GlyphFontFamily,
                    GlyphProbeEmPoints,
                    FontStyle.Regular,
                    GraphicsUnit.Point
                );
                using var brush = new SolidBrush(Color.Black);
                using var format = GlyphTextFormat();

                graphics.DrawString(glyph, font, brush, new PointF(probeOrigin, probeOrigin), format);
            }

            ink = FindInkBox(bitmap, probeOrigin);
        }
        catch (Exception ex) when (ex is ArgumentException or ExternalException)
        {
            // A glyph that cannot be measured is left off rather than stopping the print. The words under
            // the circle still say which marking the lot carries.
            ink = null;
        }

        GlyphInkProbes[glyph] = ink;

        return ink;
    }

    /// <summary>
    /// The box the dark pixels of a rendered glyph occupy, relative to the point it was drawn from.
    /// </summary>
    private static RectangleF? FindInkBox(Bitmap bitmap, float origin)
    {
        var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var data = bitmap.LockBits(
            bounds,
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );

        var pixels = new byte[Math.Abs(data.Stride) * bitmap.Height];

        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;

        try
        {
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            for (var y = 0; y < bitmap.Height; y++)
            {
                var row = y * data.Stride;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    // Blue, green and red; alpha is ignored because the probe is drawn on opaque white.
                    var offset = row + (x * 4);

                    if (pixels[offset + 2] > 127 || pixels[offset + 1] > 127 || pixels[offset] > 127)
                    {
                        continue;
                    }

                    if (x < minX)
                    {
                        minX = x;
                    }

                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (y < minY)
                    {
                        minY = y;
                    }

                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return maxX >= minX && maxY >= minY
            ? new RectangleF(
                minX - origin,
                minY - origin,
                maxX - minX + 1,
                maxY - minY + 1
            )
            : null;
    }

    /// <summary>
    /// The glyph's color for a circle of this fill: black — a marking is a black shape — except on a fill so
    /// dark that black ink would not be visible, where the glyph turns white rather than vanishing.
    /// </summary>
    /// <remarks>
    /// The decision is made on the fill's WCAG relative luminance, not on a per-channel average. The
    /// average rated the palette's purple as "dark" and turned its glyph white, which is not what the
    /// marking looks like on the tag: only the near-black swatch genuinely swallows black ink.
    /// </remarks>
    public static Color GlyphInkFor(string? colorCode) =>
        RelativeLuminance(ResolveFillColor(colorCode)) < MinFillLuminanceForBlackInk
            ? Color.White
            : Color.Black;

    /// <summary>
    /// The fill luminance below which black ink stops being legible. Set so the palette's black swatch
    /// takes a white glyph while its purple swatch keeps a black one.
    /// </summary>
    private const float MinFillLuminanceForBlackInk = 0.05f;

    /// <summary>
    /// The WCAG relative luminance of a color, which is what decides whether black ink will show on it.
    /// </summary>
    private static float RelativeLuminance(Color color)
    {
        static float Linearize(byte channel)
        {
            var srgb = channel / 255f;

            return srgb <= 0.03928f
                ? srgb / 12.92f
                : MathF.Pow((srgb + 0.055f) / 1.055f, 2.4f);
        }

        return (0.2126f * Linearize(color.R))
            + (0.7152f * Linearize(color.G))
            + (0.0722f * Linearize(color.B));
    }

    private static void DrawHeading(Graphics graphics, Model_LabelOverlayGeometry geometry)
    {
        if (string.IsNullOrWhiteSpace(geometry.Heading))
        {
            return;
        }

        var radius = geometry.Diameter / 2f;
        var top = geometry.CenterY - radius - (geometry.Diameter * 33f / 100f);
        var emSize = CaptionEmSize(geometry.Diameter);

        DrawCentredText(graphics, geometry.Heading, geometry.CenterX, top, emSize);

        // The tag's own headings are underlined, so this one is too.
        using var font = new Font(TextFontFamily, emSize, FontStyle.Bold, GraphicsUnit.Point);
        var size = graphics.MeasureString(geometry.Heading, font);
        var underlineY = top + size.Height;

        using var underline = new Pen(Color.Black, UnderlineWidthPoints / 72f);

        graphics.DrawLine(
            underline,
            geometry.CenterX - (size.Width / 2f),
            underlineY,
            geometry.CenterX + (size.Width / 2f),
            underlineY
        );
    }

    /// <summary>
    /// The caption and heading size in points, taken from the circle's diameter so both scale with it.
    /// </summary>
    private static float CaptionEmSize(float diameterInches) => diameterInches * 72f * 15f / 100f;

    private static void DrawCentredText(
        Graphics graphics,
        string text,
        float centerX,
        float top,
        float emSize,
        float boxWidth = 4f,
        bool bold = true
    )
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        using var font = new Font(
            TextFontFamily,
            emSize,
            bold ? FontStyle.Bold : FontStyle.Regular,
            GraphicsUnit.Point
        );
        using var brush = new SolidBrush(Color.Black);
        using var format = new StringFormat { Alignment = StringAlignment.Center };

        // A box wide enough for the caller's area, centred on the circle, so a long part number has room
        // rather than being clipped.
        var box = new RectangleF(centerX - (boxWidth / 2f), top, boxWidth, emSize / 72f * 1.6f);

        graphics.DrawString(text, font, brush, box, format);
    }

    private static Color ParseHex(string hexToken)
    {
        var hex = hexToken.TrimStart('#');

        if (
            hex.Length != 6
            || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb)
        )
        {
            return Color.FromArgb(128, 134, 139);
        }

        return Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
    }
}
