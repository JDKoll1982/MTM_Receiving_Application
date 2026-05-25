using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Helpers;

public sealed class Helper_DunnagePoNumberTests
{
    [Theory]
    [InlineData("62450", "PO-062450")]
    [InlineData("PO-62450", "PO-062450")]
    [InlineData("po62450", "PO-062450")]
    [InlineData("64489B", "PO-064489B")]
    [InlineData("PO-064489B", "PO-064489B")]
    public void FormatForEntry_ShouldReturnCanonicalReceivingStylePo(string input, string expected)
    {
        Helper_DunnagePoNumber.FormatForEntry(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("PO-12A")]
    [InlineData("Not a PO")]
    [InlineData("PO-1234567")]
    public void FormatForEntry_ShouldLeaveUnsupportedValuesTrimmedButOtherwiseUnchanged(
        string input
    )
    {
        Helper_DunnagePoNumber.FormatForEntry(input).Should().Be(input.Trim());
    }
}
