using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Helpers;

/// <summary>
/// Validates Helper_ScannerLocationFormat against all 16 layout variants documented in
/// ScannerUpdate.md (Step 6b-a1-c-b): 8 standard 2-digit-padded forms and 8 unpadded forms.
/// </summary>
public sealed class Helper_ScannerLocationFormatTests
{
    // ── Standard 2-digit padded group ────────────────────────────────────────────

    [Theory]
    [InlineData("V-A0-01", "V-A0-01")]
    [InlineData("VA-A0-01", "VA-A0-01")]
    [InlineData("VA-A001", "VA-A0-01")]
    [InlineData("V-A001", "V-A0-01")]
    [InlineData("VAA0-01", "VA-A0-01")]
    [InlineData("VA0-01", "V-A0-01")]
    [InlineData("VAA001", "VA-A0-01")]
    [InlineData("VA001", "V-A0-01")]
    public void SanitizeLocationFormat_ShouldNormalizeStandardPaddedLayouts(
        string rawInput,
        string expected
    )
    {
        var result = Helper_ScannerLocationFormat.SanitizeLocationFormat(rawInput);

        result.Should().Be(expected);
    }

    // ── Unpadded group (auto-corrected & padded) ────────────────────────────────

    [Theory]
    [InlineData("V-A0-1", "V-A0-01")]
    [InlineData("VA-A0-1", "VA-A0-01")]
    [InlineData("VA-A01", "VA-A0-01")]
    [InlineData("V-A01", "V-A0-01")]
    [InlineData("VAA0-1", "VA-A0-01")]
    [InlineData("VA0-1", "V-A0-01")]
    [InlineData("VAA01", "VA-A0-01")]
    [InlineData("VA01", "V-A0-01")]
    public void SanitizeLocationFormat_ShouldPadUnpaddedSuffixLayouts(
        string rawInput,
        string expected
    )
    {
        var result = Helper_ScannerLocationFormat.SanitizeLocationFormat(rawInput);

        result.Should().Be(expected);
    }

    [Fact]
    public void SanitizeLocationFormat_ShouldUppercaseLowercaseInput()
    {
        Helper_ScannerLocationFormat.SanitizeLocationFormat("va-a0-01").Should().Be("VA-A0-01");
    }

    [Fact]
    public void SanitizeLocationFormat_ShouldReturnNull_WhenInputDoesNotMatchLayout()
    {
        Helper_ScannerLocationFormat.SanitizeLocationFormat("A").Should().BeNull();
        Helper_ScannerLocationFormat.SanitizeLocationFormat("A-0-01").Should().BeNull();
        Helper_ScannerLocationFormat.SanitizeLocationFormat("A-B-C-D").Should().BeNull();
        Helper_ScannerLocationFormat.SanitizeLocationFormat("ABC-01").Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeLocationFormat_ShouldReturnNull_WhenBlankInput(string? rawInput)
    {
        Helper_ScannerLocationFormat.SanitizeLocationFormat(rawInput).Should().BeNull();
    }
}
