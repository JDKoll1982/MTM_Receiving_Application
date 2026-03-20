using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    public class Converter_DoubleToDecimal : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value is decimal d)
            {
                if (d == 0m)
                {
                    return double.NaN;
                }

                return System.Convert.ToDouble(d);
            }

            return double.NaN;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        {
            if (value is double d)
            {
                if (double.IsNaN(d))
                {
                    return 0m;
                }

                return System.Convert.ToDecimal(d);
            }
            return 0m;
        }
    }
}

