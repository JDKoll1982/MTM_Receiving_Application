using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts between double? and double for NumberBox bindings
/// </summary>
public class NullableDoubleToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return doubleValue;
        }
        return 0.0;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return (double?)doubleValue;
        }
        return null;
    }
}
