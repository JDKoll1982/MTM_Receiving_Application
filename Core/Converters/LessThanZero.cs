using System.Globalization;
using System.Reflection;
using System.Windows.Data;

// Encapsulation Complete. (2024/07/30)ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);

namespace MTM_Waitlist_Application_2._0.Core.Converters
{
    public class LessThanZeroConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is string stringValue && double.TryParse(stringValue, out double doubleValueFromString))
                {
                    return doubleValueFromString < 0;
                }
                else if (value is int intValue)
                {
                    return intValue < 0;
                }
                else if (value is double doubleValue)
                {
                    return doubleValue < 0;
                }
                else if (value is float floatValue)
                {
                    return floatValue < 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return false;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return null;
            }
        }
    }
}