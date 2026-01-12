using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void Test_BasicMath_ShouldPass()
    {
        // Arrange
        var expected = 4;

        // Act
        var result = 2 + 2;

        // Assert
        result.Should().Be(expected);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Test_StringOperation_ShouldPass()
    {
        // Arrange
        var testString = "Hello, World!";

        // Act
        var result = testString.ToUpper();

        // Assert
        result.Should().Be("HELLO, WORLD!");
        Assert.AreEqual("HELLO, WORLD!", result);
    }
}
