using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts between a <see cref="decimal"/> quantity and the text of an editable box, rendering the
/// blank sentinel of <c>0</c> as an empty string.
/// </summary>
/// <remarks>
/// Intended for two-way <c>TextBox</c> bindings that must update the source on every keystroke so
/// running totals recalculate while the user types. A <c>NumberBox</c> cannot do this: it only
/// commits <c>Value</c> when the user presses Enter, clicks a spin button, or leaves the control.
/// </remarks>
public class Converter_DecimalToBlankText : IValueConverter
{
    /// <summary>
    /// Maps a stored quantity to box text, using an empty string for the blank sentinel and for any
    /// value that is not a positive quantity.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is decimal quantity && quantity > 0
            ? quantity.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
    }

    /// <summary>
    /// Maps box text back to a stored quantity, treating empty, unparseable, signed, or non-positive
    /// text as the blank sentinel.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (
            value is string text
            && decimal.TryParse(
                text,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var quantity
            )
            && quantity > 0
        )
        {
            return quantity;
        }

        return 0m;
    }
}
