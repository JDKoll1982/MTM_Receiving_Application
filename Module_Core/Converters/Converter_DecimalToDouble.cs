using System;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts between <see cref="decimal"/> and <see cref="double"/> for binding
/// <c>decimal</c> model properties to WinUI <c>NumberBox.Value</c> (which is <c>double</c>).
/// </summary>
public class Converter_DecimalToDouble : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is decimal d ? (double)d : 0.0;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        // NumberBox reports NaN when the user clears the text; casting that to decimal overflows.
        if (value is not double d || double.IsNaN(d) || double.IsInfinity(d))
        {
            return 0m;
        }

        if (d <= (double)decimal.MinValue || d >= (double)decimal.MaxValue)
        {
            return 0m;
        }

        return (decimal)d;
    }
}
