using System.Drawing;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers the cut-out card sheet and the print setup that decides which way round it goes. Both decide
/// what comes out of the printer, and neither needs a printer to check.
/// </summary>
public sealed class Helper_LabelOverlayPrinterCardSheetTests
{
    [Fact]
    public void CardsPerSheet_ShouldBeEight()
    {
        Helper_LabelOverlayPrinter.CardsPerSheet.Should().Be(8);
    }

    [Fact]
    public void BuildCardRects_ShouldTileTheWholePage_WhenThePageHasSize()
    {
        var tiles = Helper_LabelOverlayPrinter.BuildCardRects(
            Helper_LabelOverlayPrinter.PageWidthInches,
            Helper_LabelOverlayPrinter.PageHeightInches
        );

        tiles.Should().HaveCount(8);

        var covered = tiles.Sum(tile => tile.Width * tile.Height);
        var page =
            Helper_LabelOverlayPrinter.PageWidthInches
            * Helper_LabelOverlayPrinter.PageHeightInches;

        covered.Should().BeApproximately(page, 0.0001f, "the cards share edges with no gaps");
    }

    [Fact]
    public void BuildCardRects_ShouldStartAtTheTopLeftCorner()
    {
        var tiles = Helper_LabelOverlayPrinter.BuildCardRects(
            Helper_LabelOverlayPrinter.PageWidthInches,
            Helper_LabelOverlayPrinter.PageHeightInches
        );

        tiles[0].Left.Should().Be(0f);
        tiles[0].Top.Should().Be(0f);
        tiles[0].Width.Should()
            .BeApproximately(Helper_LabelOverlayPrinter.PageWidthInches / 4f, 0.0001f);
        tiles[0].Height.Should()
            .BeApproximately(Helper_LabelOverlayPrinter.PageHeightInches / 2f, 0.0001f);
    }

    [Fact]
    public void BuildCardRects_ShouldPutTheLastCardInTheBottomRightCorner()
    {
        var tiles = Helper_LabelOverlayPrinter.BuildCardRects(
            Helper_LabelOverlayPrinter.PageWidthInches,
            Helper_LabelOverlayPrinter.PageHeightInches
        );

        var last = tiles[^1];

        last.Right.Should().BeApproximately(Helper_LabelOverlayPrinter.PageWidthInches, 0.0001f);
        last.Bottom.Should().BeApproximately(Helper_LabelOverlayPrinter.PageHeightInches, 0.0001f);
    }

    [Fact]
    public void BuildCardRects_ShouldReturnNothing_WhenThePageHasNoSize()
    {
        Helper_LabelOverlayPrinter.BuildCardRects(0f, 8.5f).Should().BeEmpty();
    }

    [Fact]
    public void BuildSheetCards_ShouldFillEveryTileWithTheOneCard()
    {
        var card = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-1", "⭐", "BLUE");

        var sheet = Helper_LabelOverlayPrinter.BuildSheetCards(card);

        sheet.Should().HaveCount(Helper_LabelOverlayPrinter.CardsPerSheet);
        sheet.Should().AllSatisfy(tile => tile.Should().Be(card));
    }

    [Fact]
    public void BuildSheetCards_ShouldNotMixTwoLotsOnOneSheet()
    {
        var first = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-1", "⭐", "BLUE");
        var second = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-2", "▲", "TEAL");

        var sheet = Helper_LabelOverlayPrinter.BuildSheetCards(first);

        sheet.Should().NotContain(second, "a cut sheet is one lot's page, or loose cards cannot be told apart");
    }

    [Fact]
    public void BuildSheetCards_ShouldHonourASmallerSheet()
    {
        var card = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-1", "⭐", "BLUE");

        Helper_LabelOverlayPrinter.BuildSheetCards(card, 2).Should().HaveCount(2);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(3, 1, 3)]
    [InlineData(3, 4, 12)]
    [InlineData(9, 2, 18)]
    public void CardSheetPageCount_ShouldGiveOnePagePerLotPerDuplicate(
        int cardCount,
        int duplicates,
        int expected
    )
    {
        Helper_LabelOverlayPrinter.CardSheetPageCount(cardCount, duplicates).Should().Be(expected);
    }

    [Fact]
    public void CardSheetPageCount_ShouldNeverCountZeroPages()
    {
        Helper_LabelOverlayPrinter
            .CardSheetPageCount(2, 0)
            .Should()
            .Be(2, "a duplicate count of zero still prints each lot once");
    }

    [Fact]
    public void CardForSheet_ShouldPrintEachLotOnItsOwnPages_AndNeverMixThem()
    {
        var first = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-1", "⭐", "BLUE");
        var second = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-2", "▲", "TEAL");
        var cards = new List<Model_LabelOverlayCard> { first, second };

        // Two lots duplicated twice: four sheets, the first lot's two then the second lot's two.
        var perSheet = Enumerable
            .Range(0, Helper_LabelOverlayPrinter.CardSheetPageCount(cards.Count, 2))
            .Select(index => Helper_LabelOverlayPrinter.CardForSheet(cards, index, 2))
            .ToList();

        perSheet.Should().HaveCount(4);
        perSheet.Take(2).Should().OnlyContain(card => card == first);
        perSheet.Skip(2).Should().OnlyContain(card => card == second);
    }

    [Fact]
    public void CardForSheet_ShouldRepeatTheSameLot_UntilItsDuplicatesRunOut()
    {
        var card = new Model_LabelOverlayCard("TEST-PART-1", "TEST-LOT-1", "⭐", "BLUE");
        var cards = new List<Model_LabelOverlayCard> { card };

        for (var sheet = 0; sheet < 5; sheet++)
        {
            Helper_LabelOverlayPrinter.CardForSheet(cards, sheet, 5).Should().Be(card);
        }
    }

    [Fact]
    public void BuildCardOutlines_ShouldBoxEveryCardOnAllFourSides()
    {
        var outlines = Helper_LabelOverlayPrinter.BuildCardOutlines(
            Helper_LabelOverlayPrinter.PageWidthInches,
            Helper_LabelOverlayPrinter.PageHeightInches
        );

        outlines.Should().HaveCount(Helper_LabelOverlayPrinter.CardsPerSheet);

        var pageWidth = Helper_LabelOverlayPrinter.PageWidthInches;
        var pageHeight = Helper_LabelOverlayPrinter.PageHeightInches;

        outlines
            .Should()
            .AllSatisfy(outline =>
            {
                // A stroke centred on the page edge loses half its width to the printer's clip, which is
                // what left cards without a border; the outline has to sit inside the page to survive.
                outline.Left.Should().BeGreaterThan(0f);
                outline.Top.Should().BeGreaterThan(0f);
                outline.Right.Should().BeLessThan(pageWidth);
                outline.Bottom.Should().BeLessThan(pageHeight);
                outline.Width.Should().BeGreaterThan(0f);
                outline.Height.Should().BeGreaterThan(0f);
            });
    }

    [Fact]
    public void BuildCardOutlines_ShouldSitOnTheTileItBelongsTo()
    {
        var tiles = Helper_LabelOverlayPrinter.BuildCardRects(
            Helper_LabelOverlayPrinter.PageWidthInches,
            Helper_LabelOverlayPrinter.PageHeightInches
        );
        var outlines = Helper_LabelOverlayPrinter.BuildCardOutlines(
            Helper_LabelOverlayPrinter.PageWidthInches,
            Helper_LabelOverlayPrinter.PageHeightInches
        );

        for (var index = 0; index < tiles.Count; index++)
        {
            outlines[index].Left.Should().BeGreaterThanOrEqualTo(tiles[index].Left);
            outlines[index].Top.Should().BeGreaterThanOrEqualTo(tiles[index].Top);
            outlines[index].Right.Should().BeLessThanOrEqualTo(tiles[index].Right);
            outlines[index].Bottom.Should().BeLessThanOrEqualTo(tiles[index].Bottom);
        }
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldFitTheGridToThePageBeingDrawnOn()
    {
        // The driver claims the whole sheet is reachable, but this page can only be marked over a smaller
        // area — the grid has to follow the page, or its outer borders are clipped away.
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0.17f, 0.17f, 9.5f, 7.5f),
            0.25f
        );

        area.Left.Should().BeApproximately(0.25f, 0.0001f);
        area.Top.Should().BeApproximately(0.25f, 0.0001f);
        area.Right.Should().BeApproximately(9.65f, 0.0001f);
        area.Bottom.Should().BeApproximately(7.65f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldKeepTheTrimmingMargin_WhenThePageIsTheSmallerOne()
    {
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0.17f, 0.17f, 10.66f, 8.16f),
            0.25f
        );

        area.Left.Should().BeApproximately(0.25f, 0.0001f);
        area.Right.Should().BeApproximately(10.75f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldKeepThePageArea_WhenTheGraphicsReportsNoClip()
    {
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0f, 0f, 1100f, 850f),
            RectangleF.Empty,
            0.25f
        );

        area.Width.Should().BeApproximately(10.5f, 0.0001f);
        area.Height.Should().BeApproximately(8f, 0.0001f);
    }

    [Fact]
    public void PrintCardSheet_ShouldRefuse_WhenNoCardCarriesAMarking()
    {
        var cards = new List<Model_LabelOverlayCard>
        {
            new("TEST-PART-1", "TEST-LOT-1", string.Empty, "  "),
        };

        var result = Helper_LabelOverlayPrinter.PrintCardSheet("any printer", cards);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("blank");
    }

    [Fact]
    public void PrintCardSheet_ShouldRefuse_WhenNoPrinterIsChosen()
    {
        var cards = new List<Model_LabelOverlayCard>
        {
            new("TEST-PART-1", "TEST-LOT-1", "⭐", "BLUE"),
        };

        var result = Helper_LabelOverlayPrinter.PrintCardSheet("   ", cards);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Choose a printer");
    }

    [Fact]
    public void Print_ShouldRefuse_WhenNoPrinterIsChosen()
    {
        var result = Helper_LabelOverlayPrinter.Print(
            string.Empty,
            [("⭐", "BLUE")],
            Model_LabelOverlayGeometry.Default
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Choose a printer");
    }

    [Fact]
    public void Print_ShouldRefuse_WhenThePageIsTooSmallForTheCircle()
    {
        var small = new Model_LabelOverlayPrintSettings
        {
            PaperName = "Photo 4x6",
            PaperWidthInches = 4f,
            PaperHeightInches = 6f,
        };

        var result = Helper_LabelOverlayPrinter.Print(
            "any printer",
            [("⭐", "BLUE")],
            Model_LabelOverlayGeometry.Default,
            small
        );

        result.IsSuccess.Should().BeFalse("a 6 x 4 page cannot hold the tag's two-inch circle");
        result.ErrorMessage.Should().Contain("does not fit");
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldStayInsideThePrintableArea_WhenTheDriverHasHardMargins()
    {
        // A letter sheet the driver can only reach from a sixteenth of an inch in on each side.
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(16f, 15f, 1068f, 820f),
            0.25f
        );

        area.Left.Should().BeApproximately(0.25f, 0.0001f);
        area.Top.Should().BeApproximately(0.25f, 0.0001f);
        area.Right.Should().BeApproximately(10.75f, 0.0001f);
        area.Bottom.Should().BeApproximately(8.25f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldBackOffTheHardMargin_WhenTheAskedMarginIsSmaller()
    {
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(16f, 15f, 1068f, 820f),
            0.05f
        );

        // The requested five hundredths is inside the printer's own unreachable edge, so the printable
        // area wins — otherwise the left and top scissor lines would be clipped away.
        area.Left.Should().BeApproximately(0.18f, 0.0001f);
        area.Top.Should().BeApproximately(0.17f, 0.0001f);
        area.Right.Should().BeApproximately(10.82f, 0.0001f);
        area.Bottom.Should().BeApproximately(8.33f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldUseTheWholePage_WhenTheDriverReportsNoPrintableArea()
    {
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            RectangleF.Empty,
            0.25f
        );

        area.Left.Should().BeApproximately(0.25f, 0.0001f);
        area.Top.Should().BeApproximately(0.25f, 0.0001f);
        area.Width.Should().BeApproximately(10.5f, 0.0001f);
        area.Height.Should().BeApproximately(8f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldFallBackToThePage_WhenTheReachableAreaIsTooSmall()
    {
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(500f, 400f, 40f, 20f),
            0.25f
        );

        area.Width.Should().BeApproximately(10.5f, 0.0001f);
        area.Height.Should().BeApproximately(8f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldIgnoreThePrintableArea_WhenItIsTheWrongWayRound()
    {
        // The Microsoft PDF printer answers with the paper's own orientation even on a landscape job. A
        // landscape sheet taking that answer at face value would come back 8.5 inches wide and squeeze the
        // cards into half the page.
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0f, 0f, 850f, 1100f),
            0.25f
        );

        area.Left.Should().BeApproximately(0.25f, 0.0001f);
        area.Top.Should().BeApproximately(0.25f, 0.0001f);
        area.Width.Should().BeApproximately(10.5f, 0.0001f);
        area.Height.Should().BeApproximately(8f, 0.0001f);
    }

    [Fact]
    public void ResolveCardSheetArea_ShouldIgnoreThePrintableArea_WhenItIsLargerThanThePage()
    {
        var area = Helper_LabelOverlayPrinter.ResolveCardSheetArea(
            new RectangleF(0f, 0f, 1100f, 850f),
            new RectangleF(0f, 0f, 1300f, 1000f),
            0.25f
        );

        area.Width.Should().BeApproximately(10.5f, 0.0001f);
        area.Height.Should().BeApproximately(8f, 0.0001f);
    }

    [Fact]
    public void MediaCanHoldPage_ShouldAcceptLetterForTheTag()
    {
        Helper_LabelOverlayPrinter
            .MediaCanHoldPage(11f, 8.5f, 11f, 8.5f)
            .Should()
            .BeTrue("letter media is what the tag artwork is drawn for");
    }

    [Fact]
    public void MediaCanHoldPage_ShouldAcceptBiggerMedia()
    {
        Helper_LabelOverlayPrinter
            .MediaCanHoldPage(11.69f, 8.27f, 11f, 8.5f)
            .Should()
            .BeFalse("A4 is narrower than letter across the short edge");

        Helper_LabelOverlayPrinter.MediaCanHoldPage(14f, 8.5f, 11f, 8.5f).Should().BeTrue();
    }

    [Theory]
    [InlineData(4f, 6f)]
    [InlineData(3f, 2f)]
    [InlineData(0f, 0f)]
    public void MediaCanHoldPage_ShouldRejectMediaTooSmallForTheTag(float longEdge, float shortEdge)
    {
        Helper_LabelOverlayPrinter
            .MediaCanHoldPage(longEdge, shortEdge, 11f, 8.5f)
            .Should()
            .BeFalse(
                "a printer loaded with label stock cannot print an 11 x 8.5 tag, and sending the job "
                    + "anyway is what comes out blank"
            );
    }
}
