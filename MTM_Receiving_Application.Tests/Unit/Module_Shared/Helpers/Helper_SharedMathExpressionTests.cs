using FluentAssertions;
using MTM_Receiving_Application.Module_Shared.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Shared.Helpers;

/// <summary>
/// Covers the arithmetic engine behind the shared math quantity textbox: whole-number literals,
/// operator precedence, parentheses, divide-by-zero, and the rounding signal the box uses to warn
/// the operator that an answer was adjusted.
/// </summary>
public sealed class Helper_SharedMathExpressionTests
{
    [Theory]
    [InlineData("12", 12)]
    [InlineData("  7 ", 7)]
    [InlineData("12+4", 16)]
    [InlineData("20-5", 15)]
    [InlineData("6*7", 42)]
    [InlineData("10/2", 5)]
    [InlineData("2+3*4", 14)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("100 - (25+5) / 3", 90)]
    [InlineData("-(4+6)", -10)]
    public void TryEvaluate_ShouldApplyStandardPrecedence(string expression, decimal expected)
    {
        Helper_SharedMathExpression.TryEvaluate(expression, out var result).Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("5/0")]
    [InlineData("(5+3")]
    [InlineData("5)")]
    [InlineData("5+")]
    [InlineData("5 5")]
    [InlineData("2.5")]
    [InlineData("abc")]
    [InlineData("*5")]
    public void TryEvaluate_ShouldReturnFalse_WhenExpressionIsInvalid(string expression)
    {
        Helper_SharedMathExpression.TryEvaluate(expression, out var result).Should().BeFalse();
        result.Should().Be(0m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryEvaluate_ShouldReturnFalse_WhenExpressionIsBlank(string? expression)
    {
        Helper_SharedMathExpression.TryEvaluate(expression, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("48", 48, false)]
    [InlineData("12*4", 48, false)]
    [InlineData("10/4", 3, true)]
    [InlineData("2/3", 1, true)]
    [InlineData("11/2", 6, true)]
    public void TryEvaluateWholeNumber_ShouldRoundToNearestWholeNumber(
        string expression,
        decimal expected,
        bool expectedRounded
    )
    {
        var succeeded = Helper_SharedMathExpression.TryEvaluateWholeNumber(
            expression,
            out var wholeNumber,
            out var wasRounded
        );

        succeeded.Should().BeTrue();
        wholeNumber.Should().Be(expected);
        wasRounded.Should().Be(expectedRounded);
    }

    [Fact]
    public void TryEvaluateWholeNumber_ShouldReportRounding_WhenFractionIsRoundedAwayFromZero()
    {
        Helper_SharedMathExpression.TryEvaluateWholeNumber("1/2", out var wholeNumber, out var wasRounded)
            .Should()
            .BeTrue();

        wholeNumber.Should().Be(1);
        wasRounded.Should().BeTrue();
    }

    [Theory]
    [InlineData('5', true)]
    [InlineData('+', true)]
    [InlineData('*', true)]
    [InlineData('(', true)]
    [InlineData(' ', true)]
    [InlineData('.', false)]
    [InlineData(',', false)]
    [InlineData('%', false)]
    [InlineData('a', false)]
    public void IsAllowedInputCharacter_ShouldOnlyAcceptArithmeticCharacters(
        char character,
        bool expected
    )
    {
        Helper_SharedMathExpression.IsAllowedInputCharacter(character).Should().Be(expected);
    }

    [Fact]
    public void ToInputText_ShouldRenderWholeNumberWithoutSeparators()
    {
        Helper_SharedMathExpression.ToInputText(12500m).Should().Be("12500");
    }

    [Theory]
    [InlineData("48", 48)]
    [InlineData(" 48 ", 48)]
    [InlineData("2.5", 2.5)]
    public void TryParseQuantityText_ShouldAcceptPlainNumbers(string rawInput, decimal expected)
    {
        Helper_SharedMathExpression.TryParseQuantityText(rawInput, out var value).Should().BeTrue();
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("5+")]
    [InlineData("-5")]
    [InlineData("12*4")]
    [InlineData("1,5")]
    [InlineData("1e3")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParseQuantityText_ShouldRejectLeftoverExpressionsAndSigns(string? rawInput)
    {
        // decimal.TryParse alone reads "5+" as 5, which would silently move the wrong quantity.
        Helper_SharedMathExpression.TryParseQuantityText(rawInput, out var value).Should().BeFalse();
        value.Should().Be(0m);
    }
}
