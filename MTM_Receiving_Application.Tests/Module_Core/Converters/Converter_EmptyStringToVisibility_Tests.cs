using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Module_Core.Converters;

/// <summary>
/// Unit tests for <see cref="Converter_EmptyStringToVisibility"/>.
/// Tests conversion of string values to Visibility based on empty/whitespace checks with optional inverse.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Type", "Converter")]
public class Converter_EmptyStringToVisibility_Tests
{
    private readonly Converter_EmptyStringToVisibility _sut;

    public Converter_EmptyStringToVisibility_Tests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData("Hello", null, Visibility.Visible)]
    [InlineData("Some text", null, Visibility.Visible)]
    [InlineData("  text  ", null, Visibility.Visible)]
    [InlineData("", null, Visibility.Collapsed)]
    [InlineData("   ", null, Visibility.Collapsed)]
    [InlineData(null, null, Visibility.Collapsed)]
    public void Convert_WithStringValue_ReturnsExpectedVisibility(string? input, object? parameter, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Hello", "Inverse", Visibility.Collapsed)]
    [InlineData("Some text", "Inverse", Visibility.Collapsed)]
    [InlineData("", "Inverse", Visibility.Visible)]
    [InlineData("   ", "Inverse", Visibility.Visible)]
    [InlineData(null, "Inverse", Visibility.Visible)]
    [InlineData("Text", "inverse", Visibility.Collapsed)]
    public void Convert_WithInverseParameter_ReturnsInvertedVisibility(string? input, string parameter, Visibility expected)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), parameter, "en-US");

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(true)]
    [InlineData(3.14)]
    public void Convert_WithNonStringValue_ReturnsCollapsed(object input)
    {
        // Act
        var result = _sut.Convert(input, typeof(Visibility), null, "en-US");

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_WithUnknownParameter_IgnoresParameter()
    {
        // Act
        var result = _sut.Convert("Hello", typeof(Visibility), "SomeOtherParam", "en-US");

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(string), null, "en-US");

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
