using System;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Module_Core.Converters;

public class Converter_ObjectToBoolean : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        return true;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        string language
    )
    {
        throw new NotImplementedException();
    }
}
