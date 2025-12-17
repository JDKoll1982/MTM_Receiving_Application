using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Converters
{
    public class DoubleToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal d)
            {
                return System.Convert.ToDouble(d);
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                return System.Convert.ToDecimal(d);
            }
            return 0m;
        }
    }
}
