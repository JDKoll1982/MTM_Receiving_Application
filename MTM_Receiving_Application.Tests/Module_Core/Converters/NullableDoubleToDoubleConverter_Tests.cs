using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Module_Core.Converters;

/// <summary>
/// Unit tests for <see cref="NullableDoubleToDoubleConverter"/>.
/// Tests bidirectional conversion between nullable double (double?) and double for NumberBox bindings.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Type", "Converter")]
public class NullableDoubleToDoubleConverter_Tests
{
    private readonly NullableDoubleToDoubleConverter _sut;

    public NullableDoubleToDoubleConverter_Tests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(42.5, 42.5)]
    [InlineData(100.0, 100.0)]
    [InlineData(1234.56789, 1234.56789)]
    [InlineData(-50.25, -50.25)]
    [InlineData(double.MaxValue, double.MaxValue)]
    [InlineData(double.MinValue, double.MinValue)]
    public void Convert_WithDoubleValue_ReturnsDoubleValue(double input, double expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(double), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text")]
    [InlineData(42)]
    [InlineData(true)]
    public void Convert_WithNonDoubleValue_ReturnsZero(object? input)
    {
        // Act
        var result = _sut.Convert(input, typeof(double), null, "en-US");

        // Assert
        result.Should().Be(0.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(42.5)]
    [InlineData(100.0)]
    [InlineData(1234.56789)]
    [InlineData(-50.25)]
    public void ConvertBack_WithDoubleValue_ReturnsNullableDouble(double input)
    {
        // Act
        var result = _sut.ConvertBack(input, typeof(double?), null, "en-US");

        // Assert
        result.Should().Be((double?)input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text")]
    [InlineData(42)]
    [InlineData(true)]
    public void ConvertBack_WithNonDoubleValue_ReturnsNull(object? input)
    {
        // Act
        var result = _sut.ConvertBack(input, typeof(double?), null, "en-US");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(42.5)]
    [InlineData(1234.56789)]
    [InlineData(-50.25)]
    public void Convert_RoundTrip_PreservesValue(double originalValue)
    {
        // Act - Convert to double
        var doubleResult = _sut.Convert(originalValue, typeof(double), null, "en-US");

        // Act - Convert back to nullable double
        var nullableResult = _sut.ConvertBack(doubleResult, typeof(double?), null, "en-US");

        // Assert
        nullableResult.Should().Be(originalValue);
    }

    [Fact]
    public void Convert_WithNaN_ReturnsNaN()
    {
        // Act
        var result = _sut.Convert(double.NaN, typeof(double), null, "en-US");

        // Assert
        result.Should().Be(double.NaN);
    }

    [Fact]
    public void Convert_WithPositiveInfinity_ReturnsPositiveInfinity()
    {
        // Act
        var result = _sut.Convert(double.PositiveInfinity, typeof(double), null, "en-US");

        // Assert
        result.Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void Convert_WithNegativeInfinity_ReturnsNegativeInfinity()
    {
        // Act
        var result = _sut.Convert(double.NegativeInfinity, typeof(double), null, "en-US");

        // Assert
        result.Should().Be(double.NegativeInfinity);
    }

    [Fact]
    public void ConvertBack_WithNaN_ReturnsNullableNaN()
    {
        // Act
        var result = _sut.ConvertBack(double.NaN, typeof(double?), null, "en-US");

        // Assert
        result.Should().Be((double?)double.NaN);
    }
}
