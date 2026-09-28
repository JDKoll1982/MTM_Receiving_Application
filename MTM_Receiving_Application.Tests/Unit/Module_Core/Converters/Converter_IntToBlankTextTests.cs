using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Converters;

public sealed class Converter_IntToBlankTextTests
{
    private readonly Converter_IntToBlankText _converter = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Convert_ShouldReturnEmptyText_WhenCountIsBlankSentinel(int count)
    {
        _converter.Convert(count, typeof(string), null, string.Empty).Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_ShouldReturnDigits_WhenCountIsPositive()
    {
        _converter.Convert(7, typeof(string), null, string.Empty).Should().Be("7");
    }

    [Fact]
    public void Convert_ShouldReturnEmptyText_WhenValueIsNotAnInt()
    {
        _converter.Convert(null, typeof(string), null, string.Empty).Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("abc", 0)]
    [InlineData("-4", 0)]
    [InlineData("0", 0)]
    [InlineData("2", 2)]
    [InlineData("12", 12)]
    public void ConvertBack_ShouldReturnExpectedCount_WhenTextBoxTextChanges(
        string text,
        int expected
    )
    {
        _converter.ConvertBack(text, typeof(int), null, string.Empty).Should().Be(expected);
    }

    [Fact]
    public void ConvertBack_ShouldReturnBlankSentinel_WhenValueIsNotText()
    {
        _converter.ConvertBack(3, typeof(int), null, string.Empty).Should().Be(0);
    }
}
