using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Converters;

public sealed class Converter_DecimalToDoubleTests
{
    private readonly Converter_DecimalToDouble _converter = new();

    [Fact]
    public void ConvertBack_ShouldReturnZero_WhenValueIsNaN()
    {
        _converter
            .ConvertBack(double.NaN, typeof(decimal), null, string.Empty)
            .Should()
            .Be(0m);
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void ConvertBack_ShouldReturnZero_WhenValueIsOutsideDecimalRange(double value)
    {
        _converter.ConvertBack(value, typeof(decimal), null, string.Empty).Should().Be(0m);
    }

    [Fact]
    public void ConvertBack_ShouldReturnZero_WhenValueIsNotDouble()
    {
        _converter.ConvertBack("7155", typeof(decimal), null, string.Empty).Should().Be(0m);
    }

    [Fact]
    public void ConvertBack_ShouldReturnDecimal_WhenValueIsInRange()
    {
        _converter.ConvertBack(7155.5, typeof(decimal), null, string.Empty).Should().Be(7155.5m);
    }

    [Fact]
    public void Convert_ShouldReturnDouble_WhenValueIsDecimal()
    {
        _converter.Convert(7155.5m, typeof(double), null, string.Empty).Should().Be(7155.5);
    }

    [Fact]
    public void Convert_ShouldReturnZero_WhenValueIsNotDecimal()
    {
        _converter.Convert(null, typeof(double), null, string.Empty).Should().Be(0.0);
    }
}
