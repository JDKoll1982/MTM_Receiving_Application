using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    public class Converter_EmptyStringToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? stringValue = value as string;
            bool isVisible = !string.IsNullOrWhiteSpace(stringValue);

            if (parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

