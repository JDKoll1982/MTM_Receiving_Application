using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    /// <summary>
    /// Converts decimal values to integer (whole number) for display, removing decimal places.
    /// </summary>
    public class Converter_DecimalToInt : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return ((int)decimalValue).ToString("N0");
            }
            if (value is double doubleValue)
            {
                return ((int)doubleValue).ToString("N0");
            }
            if (value is float floatValue)
            {
                return ((int)floatValue).ToString("N0");
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        {
            if (value is string strValue && decimal.TryParse(strValue, out decimal result))
            {
                return result;
            }
            return 0m;
        }
    }
}

