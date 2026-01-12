using Xunit;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests;

public class UnitTest1
{
    [Fact]
    public void Test_BasicMath_ShouldPass()
    {
        // Arrange
        var expected = 4;

        // Act
        var result = 2 + 2;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Test_StringOperation_ShouldPass()
    {
        // Arrange
        var testString = "Hello, World!";

        // Act
        var result = testString.ToUpper();

        // Assert
        result.Should().Be("HELLO, WORLD!");
    }
}
