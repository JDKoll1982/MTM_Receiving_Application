using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Converters;

namespace MTM_Receiving_Application.Tests.Module_Core.Converters;

/// <summary>
/// Unit tests for <see cref="Converter_BooleanToVisibility"/>.
/// Tests conversion of boolean values to WinUI Visibility with optional inverse parameter.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Type", "Converter")]
public class Converter_BooleanToVisibility_Tests
{
    private readonly Converter_BooleanToVisibility _sut;

    public Converter_BooleanToVisibility_Tests()
    {
        _sut = new();
    }

    [Theory]
    [InlineData(true, null, Visibility.Visible)]
    [InlineData(false, null, Visibility.Collapsed)]
    [InlineData(true, "Inverse", Visibility.Collapsed)]
    [InlineData(false, "Inverse", Visibility.Visible)]
    [InlineData(true, "inverse", Visibility.Collapsed)]
    [InlineData(false, "inverse", Visibility.Visible)]
    public void Convert_WithBooleanValue_ReturnsExpectedVisibility(bool input, string? parameter, Visibility expected)
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
    [InlineData("true")]
    [InlineData("false")]
    [InlineData(1)]
    [InlineData(0)]
    public void Convert_WithNonBooleanValue_ReturnsCollapsed(object input)
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
        var result = _sut.Convert(true, typeof(Visibility), "SomeOtherParam", "en-US");

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act
        Action act = () => _sut.ConvertBack(Visibility.Visible, typeof(bool), null, "en-US");

        // Assert
        act.Should().Throw<NotImplementedException>();
    }
}
