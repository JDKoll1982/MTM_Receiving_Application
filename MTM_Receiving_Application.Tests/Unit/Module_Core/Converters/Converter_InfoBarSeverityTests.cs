using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Converters;
using Xunit;
using CoreInfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;
using WinInfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Converters;

public class Converter_InfoBarSeverityTests
{
    private readonly Converter_InfoBarSeverity _sut = new();

    [Theory]
    [InlineData(CoreInfoBarSeverity.Informational, WinInfoBarSeverity.Informational)]
    [InlineData(CoreInfoBarSeverity.Success, WinInfoBarSeverity.Success)]
    [InlineData(CoreInfoBarSeverity.Warning, WinInfoBarSeverity.Warning)]
    [InlineData(CoreInfoBarSeverity.Error, WinInfoBarSeverity.Error)]
    public void Convert_ReturnsMappedSeverity_WhenValueIsValid(CoreInfoBarSeverity input, WinInfoBarSeverity expected)
    {
        var result = _sut.Convert(input, typeof(WinInfoBarSeverity), null, "en-US");

        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_NullValue_ReturnsInformational()
    {
        var result = _sut.Convert(null, typeof(WinInfoBarSeverity), null, "en-US");

        result.Should().Be(WinInfoBarSeverity.Informational);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        var act = () => _sut.ConvertBack(WinInfoBarSeverity.Error, typeof(CoreInfoBarSeverity), null, "en-US");

        act.Should().Throw<NotImplementedException>();
    }
}
