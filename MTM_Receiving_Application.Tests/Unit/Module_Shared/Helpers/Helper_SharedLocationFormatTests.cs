using FluentAssertions;
using MTM_Receiving_Application.Module_Shared.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Shared.Helpers;

/// <summary>
/// Validates the canonical warehouse-location formatter used by every module: the documented
/// three-segment variants, the two-segment forms used by locations with no middle block
/// (R04 -> R-04, S00 -> S-00, W04 -> W-04), and the pass-through behavior for anything that
/// matches neither layout.
/// </summary>
public sealed class Helper_SharedLocationFormatTests
{
    // ── Three-segment (letter+digit middle block) ────────────────────────────────

    [Theory]
    [InlineData("V-A0-01", "V-A0-01")]
    [InlineData("VA-A0-01", "VA-A0-01")]
    [InlineData("VA-A001", "VA-A0-01")]
    [InlineData("V-A001", "V-A0-01")]
    [InlineData("VAA0-01", "VA-A0-01")]
    [InlineData("VA0-01", "V-A0-01")]
    [InlineData("VAA001", "VA-A0-01")]
    [InlineData("VA001", "V-A0-01")]
    [InlineData("V-A0-1", "V-A0-01")]
    [InlineData("VA-A0-1", "VA-A0-01")]
    [InlineData("VA-A01", "VA-A0-01")]
    [InlineData("V-A01", "V-A0-01")]
    [InlineData("VAA0-1", "VA-A0-01")]
    [InlineData("VA0-1", "V-A0-01")]
    [InlineData("VAA01", "VA-A0-01")]
    [InlineData("VA01", "V-A0-01")]
    public void Sanitize_ShouldNormalizeThreeSegmentLayouts(string rawInput, string expected)
    {
        Helper_SharedLocationFormat.Sanitize(rawInput).Should().Be(expected);
    }

    [Theory]
    [InlineData("va001", "V-A0-01")]
    [InlineData("VA001", "V-A0-01")]
    [InlineData("Va001", "V-A0-01")]
    [InlineData("V-A001", "V-A0-01")]
    [InlineData("VA0-01", "V-A0-01")]
    public void Sanitize_ShouldNormalizeEveryDocumentedVaVariant(
        string rawInput,
        string expected
    )
    {
        Helper_SharedLocationFormat.Sanitize(rawInput).Should().Be(expected);
    }

    // ── Two-segment (no middle block) ────────────────────────────────────────────

    [Theory]
    [InlineData("r04", "R-04")]
    [InlineData("R04", "R-04")]
    [InlineData("R-04", "R-04")]
    [InlineData("R4", "R-04")]
    [InlineData("R-4", "R-04")]
    [InlineData("R5", "R-05")]
    [InlineData("s00", "S-00")]
    [InlineData("S00", "S-00")]
    [InlineData("S0", "S-00")]
    [InlineData("w04", "W-04")]
    [InlineData("W04", "W-04")]
    [InlineData("W-4", "W-04")]
    [InlineData("T-00", "T-00")]
    public void Sanitize_ShouldNormalizeTwoSegmentLayouts(string rawInput, string expected)
    {
        Helper_SharedLocationFormat.Sanitize(rawInput).Should().Be(expected);
    }

    // ── Pass-through / no rule ───────────────────────────────────────────────────

    [Theory]
    [InlineData("RECV")]
    [InlineData("T00A")]
    [InlineData("ABC-01")]
    [InlineData("A-0-01")]
    [InlineData("A-B-C-D")]
    [InlineData("A")]
    [InlineData("MMC0000658")]
    public void Sanitize_ShouldReturnNull_WhenNoRuleMatches(string rawInput)
    {
        Helper_SharedLocationFormat.Sanitize(rawInput).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_ShouldReturnNull_WhenInputIsBlank(string? rawInput)
    {
        Helper_SharedLocationFormat.Sanitize(rawInput).Should().BeNull();
    }

    [Theory]
    [InlineData("r04", "R-04")]
    [InlineData("va001", "V-A0-01")]
    [InlineData("RECV", "RECV")]
    [InlineData("recv", "RECV")]
    [InlineData("  recv  ", "RECV")]
    public void FormatOrPassThrough_ShouldReturnCanonicalValueOrUppercaseInput(
        string rawInput,
        string expected
    )
    {
        Helper_SharedLocationFormat.FormatOrPassThrough(rawInput).Should().Be(expected);
    }

    [Fact]
    public void FormatOrPassThrough_ShouldReturnEmpty_WhenInputIsNull()
    {
        Helper_SharedLocationFormat.FormatOrPassThrough(null).Should().BeEmpty();
    }
}
