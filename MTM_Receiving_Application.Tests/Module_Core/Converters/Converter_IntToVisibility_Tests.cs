using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Module_Core.Converters;

/// <summary>
/// Unit tests for <see cref="Converter_IntToVisibility"/>.
/// Tests conversion of integer values to Visibility with multiple parameter modes:
/// - No parameter: Visible if value > 0
/// - "Inverse": Visible if value == 0
/// - Numeric string (e.g., "1", "2"): Visible if value == parameter
/// - Direct int parameter: Visible if value == parameter
/// </summary>
[Trait("Category", "Unit")]
[Trait("Type", "Converter")]
public class Converter_IntToVisibility_Tests
{
    private readonly Converter_IntToVisibility _sut;

    public Converter_IntToVisibility_Tests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData(0, Visibility.Collapsed)]
    [InlineData(1, Visibility.Visible)]
    [InlineData(5, Visibility.Visible)]
    [InlineData(-1, Visibility.Collapsed)]
    public void Convert_WithNoParameter_ShowsWhenValueGreaterThanZero(int input, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), null, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, Visibility.Visible)]
    [InlineData(1, Visibility.Collapsed)]
    [InlineData(5, Visibility.Collapsed)]
    [InlineData(-1, Visibility.Collapsed)]
    public void Convert_WithInverseParameter_ShowsWhenValueIsZero(int input, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), "Inverse", "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, Visibility.Visible)]
    [InlineData(1, Visibility.Collapsed)]
    [InlineData(5, Visibility.Collapsed)]
    public void Convert_WithInverseParameterCaseInsensitive_WorksCorrectly(int input, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), "inverse", "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "1", Visibility.Visible)]
    [InlineData(1, "2", Visibility.Collapsed)]
    [InlineData(2, "2", Visibility.Visible)]
    [InlineData(3, "3", Visibility.Visible)]
    [InlineData(0, "0", Visibility.Visible)]
    public void Convert_WithNumericStringParameter_ShowsWhenValueMatchesParameter(int input, string parameter, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 1, Visibility.Visible)]
    [InlineData(1, 2, Visibility.Collapsed)]
    [InlineData(2, 2, Visibility.Visible)]
    [InlineData(0, 0, Visibility.Visible)]
    public void Convert_WithDirectIntParameter_ShowsWhenValueMatchesParameter(int input, int parameter, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsCollapsed()
    {
        // Act
        var result = _sut.Convert(null, typeof(Visibility), null, "en-US");

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Theory]
    [InlineData("NotANumber")]
    [InlineData("abc")]
    public void Convert_WithNonNumericStringParameter_FallsBackToGreaterThanZero(string parameter)
    {
        // Act
        var result = _sut.Convert(5, typeof(Visibility), parameter, "en-US");

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Theory]
    [InlineData("5")]
    [InlineData(5.5)]
    public void Convert_WithNonIntegerValue_TreatsAsZero(object input)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), null, "en-US");

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(int), null, "en-US");

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
