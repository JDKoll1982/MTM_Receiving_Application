using System;
using System.Globalization;

namespace MTM_Receiving_Application.Module_Shared.Helpers;

/// <summary>
/// Evaluates the simple arithmetic an operator can type into a quantity box ("12*4",
/// "(8+4)/2", "120-15"). Only whole-number literals, <c>+ - * /</c> and parentheses are
/// supported, which keeps the answers predictable and easy to explain on the shop floor.
/// </summary>
/// <remarks>
/// Pure logic with no UI or database dependency so the keypad behaviour can be unit-tested
/// without a WinUI thread.
/// </remarks>
public static class Helper_SharedMathExpression
{
    /// <summary>
    /// True for every character a quantity expression may contain. Everything else is filtered
    /// out while the operator types, so letters, decimal points, percent signs, and thousands
    /// separators can never reach the evaluator.
    /// </summary>
    /// <param name="character">The character typed into the box.</param>
    public static bool IsAllowedInputCharacter(char character)
    {
        return char.IsDigit(character)
            || char.IsWhiteSpace(character)
            || character is '+' or '-' or '*' or '/' or '(' or ')';
    }

    /// <summary>
    /// Evaluates a whole-number expression using the standard precedence rules
    /// (parentheses, then multiply/divide, then add/subtract).
    /// </summary>
    /// <param name="expression">Expression exactly as typed, for example <c>(8+4)/2</c>.</param>
    /// <param name="result">The evaluated value, or zero when evaluation fails.</param>
    /// <returns><see langword="true"/> when the whole expression evaluated successfully.</returns>
    public static bool TryEvaluate(string? expression, out decimal result)
    {
        result = 0m;

        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var parser = new ExpressionParser(expression);
        try
        {
            var value = parser.ParseExpression();
            if (!parser.IsAtEnd)
            {
                return false;
            }

            result = value;
            return true;
        }
        catch (ArithmeticException)
        {
            // Divide by zero and overflow are treated as "not a valid quantity".
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Evaluates the expression and rounds it to the nearest whole number, reporting whether
    /// any rounding had to happen so the box can warn about it.
    /// </summary>
    /// <param name="expression">Expression exactly as typed.</param>
    /// <param name="wholeNumber">The nearest whole number, or zero when evaluation fails.</param>
    /// <param name="wasRounded">True when the exact answer had a fraction that was rounded.</param>
    public static bool TryEvaluateWholeNumber(
        string? expression,
        out decimal wholeNumber,
        out bool wasRounded
    )
    {
        wholeNumber = 0m;
        wasRounded = false;

        if (!TryEvaluate(expression, out var exact))
        {
            return false;
        }

        wholeNumber = Math.Round(exact, 0, MidpointRounding.AwayFromZero);
        wasRounded = wholeNumber != exact;
        return true;
    }

    /// <summary>
    /// Parses a committed quantity text as a plain number: digits with at most one decimal point,
    /// no sign, no thousands separators, no exponent. Use this instead of
    /// <see cref="decimal.TryParse(string?, out decimal)"/>, which reads a leftover expression
    /// such as <c>5+</c> as <c>5</c> and would silently move the wrong quantity.
    /// </summary>
    /// <param name="rawInput">Text as displayed in the quantity box.</param>
    /// <param name="value">Parsed value, or zero when the text is not a plain number.</param>
    public static bool TryParseQuantityText(string? rawInput, out decimal value)
    {
        value = 0m;

        return decimal.TryParse(
            rawInput?.Trim(),
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    /// <summary>
    /// Renders a value the way the box displays it: no decimal point, no thousands separators.
    /// </summary>
    /// <param name="value">Value to render.</param>
    public static string ToInputText(decimal value)
    {
        return value.ToString("0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Recursive-descent evaluator for the restricted grammar
    /// <c>expression := term (('+'|'-') term)*</c>, <c>term := factor (('*'|'/') factor)*</c>,
    /// <c>factor := ('+'|'-') factor | number | '(' expression ')'</c>.
    /// </summary>
    private sealed class ExpressionParser
    {
        private readonly string _text;
        private int _position;

        public ExpressionParser(string text)
        {
            _text = text;
        }

        public bool IsAtEnd
        {
            get
            {
                SkipWhitespace();
                return _position >= _text.Length;
            }
        }

        public decimal ParseExpression()
        {
            var value = ParseTerm();

            while (true)
            {
                SkipWhitespace();
                if (_position >= _text.Length)
                {
                    return value;
                }

                var symbol = _text[_position];
                if (symbol is not ('+' or '-'))
                {
                    return value;
                }

                _position++;
                var right = ParseTerm();
                value = symbol == '+' ? value + right : value - right;
            }
        }

        private decimal ParseTerm()
        {
            var value = ParseFactor();

            while (true)
            {
                SkipWhitespace();
                if (_position >= _text.Length)
                {
                    return value;
                }

                var symbol = _text[_position];
                if (symbol is not ('*' or '/'))
                {
                    return value;
                }

                _position++;
                var right = ParseFactor();
                value =
                    symbol == '*'
                        ? value * right
                        : right == 0m
                            ? throw new DivideByZeroException()
                            : value / right;
            }
        }

        private decimal ParseFactor()
        {
            SkipWhitespace();
            if (_position >= _text.Length)
            {
                throw new FormatException("Unexpected end of expression.");
            }

            var symbol = _text[_position];
            if (symbol is '+' or '-')
            {
                _position++;
                var operand = ParseFactor();
                return symbol == '-' ? -operand : operand;
            }

            if (symbol == '(')
            {
                _position++;
                var inner = ParseExpression();
                SkipWhitespace();
                if (_position >= _text.Length || _text[_position] != ')')
                {
                    throw new FormatException("Missing closing parenthesis.");
                }

                _position++;
                return inner;
            }

            return ParseNumber();
        }

        private decimal ParseNumber()
        {
            var start = _position;
            while (_position < _text.Length && char.IsDigit(_text[_position]))
            {
                _position++;
            }

            if (_position == start)
            {
                throw new FormatException("Expected a whole number.");
            }

            return decimal.Parse(
                _text.AsSpan(start, _position - start),
                NumberStyles.None,
                CultureInfo.InvariantCulture
            );
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }
    }
}
