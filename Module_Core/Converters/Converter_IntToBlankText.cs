using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts between an <see cref="int"/> count and the text of an editable box, rendering the
/// blank sentinel of <c>0</c> as an empty string.
/// </summary>
/// <remarks>
/// Intended for two-way <c>TextBox</c> bindings that must update the source on every keystroke.
/// A <c>NumberBox</c> cannot do this: it only commits <c>Value</c> when the user presses Enter,
/// clicks a spin button, or leaves the control.
/// </remarks>
public class Converter_IntToBlankText : IValueConverter
{
    /// <summary>
    /// Maps a stored count to box text, using an empty string for the blank sentinel and for any
    /// value that is not a positive count.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is int count && count > 0
            ? count.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
    }

    /// <summary>
    /// Maps box text back to a stored count, treating empty, unparseable, or non-positive text as
    /// the blank sentinel.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (
            value is string text
            && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)
            && count > 0
        )
        {
            return count;
        }

        return 0;
    }
}
