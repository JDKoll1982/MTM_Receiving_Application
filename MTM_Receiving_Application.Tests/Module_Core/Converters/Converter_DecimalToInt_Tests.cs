using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Module_Core.Converters;

/// <summary>
/// Unit tests for <see cref="Converter_DecimalToInt"/>.
/// Tests conversion of decimal/double/float values to formatted integer strings (N0 format).
/// ConvertBack parses strings back to decimal.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Type", "Converter")]
public class Converter_DecimalToInt_Tests
{
    private readonly Converter_DecimalToInt _sut;

    public Converter_DecimalToInt_Tests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData(42.99, "42")]
    [InlineData(100.01, "100")]
    [InlineData(0.0, "0")]
    [InlineData(1234.56, "1,234")]
    [InlineData(-50.75, "-50")]
    public void Convert_WithDecimalValue_ReturnsFormattedIntegerString(decimal input, string expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(42.99, "42")]
    [InlineData(1234.56, "1,234")]
    [InlineData(0.0, "0")]
    public void Convert_WithDoubleValue_ReturnsFormattedIntegerString(double input, string expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(42.99f, "42")]
    [InlineData(100.5f, "100")]
    public void Convert_WithFloatValue_ReturnsFormattedIntegerString(float input, string expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsEmptyString()
    {
        // Act
        var result = _sut.Convert(null, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("true")]
    [InlineData(new object[] { "text" })]
    public void Convert_WithNonNumericValue_ReturnsValueToString(object input)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(input.ToString());
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("100", 100)]
    [InlineData("0", 0)]
    [InlineData("1234", 1234)]
    [InlineData("-50", -50)]
    public void ConvertBack_WithValidString_ReturnsDecimal(string input, decimal expected)
    {
        // Act
        var result = _sut.ConvertBack(input, typeof(decimal), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData(null)]
    public void ConvertBack_WithInvalidString_ReturnsZero(string? input)
    {
        // Act
        var result = _sut.ConvertBack(input, typeof(decimal), null, "en-US");

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertBack_WithNonStringValue_ReturnsZero()
    {
        // Act
        var result = _sut.ConvertBack(42, typeof(decimal), null, "en-US");

        // Assert
        result.Should().Be(0m);
    }
}
