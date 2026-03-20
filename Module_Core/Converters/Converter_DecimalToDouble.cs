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
        return value is double d ? (decimal)d : 0m;
    }
}
