using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Converters;

public sealed class Converter_DecimalToBlankTextTests
{
    private readonly Converter_DecimalToBlankText _converter = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Convert_ShouldReturnEmptyText_WhenQuantityIsBlankSentinel(decimal quantity)
    {
        _converter
            .Convert(quantity, typeof(string), null, string.Empty)
            .Should()
            .Be(string.Empty);
    }

    [Theory]
    [InlineData(7, "7")]
    [InlineData(120.5, "120.5")]
    public void Convert_ShouldReturnDigits_WhenQuantityIsPositive(decimal quantity, string expected)
    {
        _converter.Convert(quantity, typeof(string), null, string.Empty).Should().Be(expected);
    }

    [Fact]
    public void Convert_ShouldReturnEmptyText_WhenValueIsNotADecimal()
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
    [InlineData("120.5", 120.5)]
    public void ConvertBack_ShouldReturnExpectedQuantity_WhenTextBoxTextChanges(
        string text,
        decimal expected
    )
    {
        _converter.ConvertBack(text, typeof(decimal), null, string.Empty).Should().Be(expected);
    }

    [Fact]
    public void ConvertBack_ShouldReturnBlankSentinel_WhenValueIsNotText()
    {
        _converter.ConvertBack(3m, typeof(decimal), null, string.Empty).Should().Be(0m);
    }
}
