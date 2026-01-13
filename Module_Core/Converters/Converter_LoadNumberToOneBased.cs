using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a zero-based load number to 1-based for display
/// </summary>
public class Converter_LoadNumberToOneBased : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int loadNumber)
        {
            return loadNumber + 1;
        }
        return value!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        if (value is string strValue && int.TryParse(strValue, out int displayNumber))
        {
            return displayNumber - 1;
        }
        return value!;
    }
}

