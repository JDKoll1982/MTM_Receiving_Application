using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts between int and string for two-way binding in TextBox controls
/// </summary>
public class Converter_IntToString : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue.ToString();
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (value is string stringValue && int.TryParse(stringValue, out int result))
        {
            return result;
        }
        return 0;
    }
}
