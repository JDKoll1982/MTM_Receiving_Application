using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Module_Core.Converters;

/// <summary>
/// Unit tests for <see cref="Converter_StringFormat"/>.
/// Tests string formatting using parameter as format string (e.g., "Total: {0}").
/// </summary>
[Trait("Category", "Unit")]
[Trait("Type", "Converter")]
public class Converter_StringFormat_Tests
{
    private readonly Converter_StringFormat _sut;

    public Converter_StringFormat_Tests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData("Hello", "Greeting: {0}", "Greeting: Hello")]
    [InlineData(42, "Count: {0}", "Count: 42")]
    [InlineData(3.14, "Value: {0:F2}", "Value: 3.14")]
    [InlineData("Item", "{0} - Selected", "Item - Selected")]
    public void Convert_WithValidFormatParameter_ReturnsFormattedString(object input, string format, string expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), format, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData(42)]
    [InlineData(3.14)]
    public void Convert_WithNullParameter_ReturnsValueAsIs(object input)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), null, "en-US");

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(true)]
    [InlineData(3.14)]
    public void Convert_WithNonStringParameter_ReturnsValueAsIs(object input)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), 123, "en-US");

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void Convert_WithNullValue_FormatsNullAsString()
    {
        // Act
        var result = _sut.Convert(null, typeof(string), "Value: {0}", "en-US");

        // Assert
        result.Should().Be("Value: ");
    }

    [Theory]
    [InlineData(1234, "{0:N0}", "1,234")]
    [InlineData(1234.5678, "{0:F2}", "1234.57")]
    [InlineData(0.75, "{0:P0}", "75%")]
    public void Convert_WithNumericFormatting_ReturnsFormattedNumber(object input, string format, string expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), format, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_WithEmptyFormatString_ReturnsEmptyString()
    {
        // Act
        var result = _sut.Convert("Hello", typeof(string), "", "en-US");

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        Action act = () => _sut.ConvertBack("Formatted String", typeof(object), "{0}", "en-US");

        // Assert
        act.Should().Throw<NotImplementedException>();
    }

    [Theory]
    [InlineData("Test", "Prefix: {0} Suffix", "Prefix: Test Suffix")]
    [InlineData(100, "Item #{0:D3}", "Item #100")]
    public void Convert_WithComplexFormatString_HandlesCorrectly(object input, string format, string expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(string), format, "en-US");

        // Assert
        result.Should().Be(expected);
    }
}
