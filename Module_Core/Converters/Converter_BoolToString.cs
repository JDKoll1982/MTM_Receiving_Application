using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    public class Converter_BoolToString : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

