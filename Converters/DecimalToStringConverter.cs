using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Converters
{
    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal d)
            {
                return d == 0 ? string.Empty : d.ToString("G29");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return 0m;
                }
                if (decimal.TryParse(s, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }
}
