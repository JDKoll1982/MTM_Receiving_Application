using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

public class Converter_NullableDoubleToString : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        // Handle null explicitly
        if (value == null)
        {
            return string.Empty;
        }
        
        // Handle nullable double
        if (value is double doubleValue)
        {
            return doubleValue.ToString("F2"); // Format to 2 decimal places
        }
        
        // Fallback
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
        {
            if (double.TryParse(stringValue, out double result))
            {
                return result;
            }
        }
        return null!;
    }
}
