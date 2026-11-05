using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models;

/// <summary>
/// Covers the print setup's page maths. Getting landscape wrong is what printed the tag sideways, so the
/// rotation is checked directly rather than through a printer.
/// </summary>
public sealed class Model_LabelOverlayPrintSettingsTests
{
    [Fact]
    public void CreateDefault_ShouldBeLandscapeLetterMatchingTheTagArtwork()
    {
        var settings = Model_LabelOverlayPrintSettings.CreateDefault();

        settings.Landscape.Should().BeTrue();
        settings.Format.Should().Be(Enum_LabelOverlayPrintFormat.TagOverlay);
        settings.CardSheetMarginInches.Should()
            .Be(Model_LabelOverlayPrintSettings.DefaultCardSheetMarginInches);
        settings.Copies.Should().Be(Model_LabelOverlayPrintSettings.DefaultCopies);

        var page = settings.ResolvePageSize();

        page.Width.Should().BeApproximately(11f, 0.001f);
        page.Height.Should().BeApproximately(8.5f, 0.001f);
    }

    [Fact]
    public void ResolvePageSize_ShouldSwapTheEdges_WhenPortrait()
    {
        var settings = new Model_LabelOverlayPrintSettings { Landscape = false };
        var page = settings.ResolvePageSize();

        page.Width.Should().BeApproximately(8.5f, 0.001f);
        page.Height.Should().BeApproximately(11f, 0.001f);
    }

    [Fact]
    public void ResolvePageSize_ShouldStillTurnTheLongEdgeAcross_WhenThePaperIsStoredLandscapeWay()
    {
        var settings = new Model_LabelOverlayPrintSettings
        {
            PaperWidthInches = 11f,
            PaperHeightInches = 8.5f,
            Landscape = true,
        };

        var page = settings.ResolvePageSize();

        page.Width.Should().BeApproximately(11f, 0.001f);
        page.Height.Should().BeApproximately(8.5f, 0.001f);
    }

    [Fact]
    public void ResolvePageSize_ShouldFallBackToLetter_WhenThePaperHasNoSize()
    {
        var settings = new Model_LabelOverlayPrintSettings
        {
            PaperWidthInches = 0f,
            PaperHeightInches = 0f,
            Landscape = true,
        };

        var page = settings.ResolvePageSize();

        page.Width.Should().BeApproximately(11f, 0.001f);
        page.Height.Should().BeApproximately(8.5f, 0.001f);
    }

    [Fact]
    public void Describe_ShouldNameThePaperOrientationAndPage()
    {
        var settings = new Model_LabelOverlayPrintSettings();

        settings
            .Describe()
            .Should()
            .Be("Letter · Landscape · 11\" x 8.5\"", "the advanced panel shows this while collapsed");
    }

    [Fact]
    public void Describe_ShouldNameTheTray_WhenOneIsSaved()
    {
        var settings = new Model_LabelOverlayPrintSettings
        {
            PaperSourceName = "Auto",
            PaperSourceRawKind = 7,
        };

        settings
            .Describe()
            .Should()
            .Be("Letter · Landscape · 11\" x 8.5\" · Auto", "the tray is what operators change most");
    }

    [Fact]
    public void ResolveCopies_ShouldDefaultToOneCopy()
    {
        Model_LabelOverlayPrintSettings.CreateDefault().ResolveCopies().Should().Be(1);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(4, 4)]
    [InlineData(999, 50)]
    public void ResolveCopies_ShouldHoldTheCountToTheAllowedRange(int stored, int expected)
    {
        var settings = new Model_LabelOverlayPrintSettings { Copies = stored };

        settings.ResolveCopies().Should().Be(expected);
    }
}
