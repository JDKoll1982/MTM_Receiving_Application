using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

public class Converter_NullableIntToString : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        // Handle null explicitly
        if (value == null)
        {
            return string.Empty;
        }
        
        // Handle nullable int
        if (value is int intValue)
        {
            return intValue.ToString();
        }
        
        // Fallback
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
        {
            if (int.TryParse(stringValue, out int result))
            {
                return result;
            }
        }
        return null!;
    }
}
