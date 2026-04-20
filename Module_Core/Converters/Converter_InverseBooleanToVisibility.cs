using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Module_Core.Converters;

public class Converter_InverseBooleanToVisibility : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        if (value is bool boolValue && !boolValue)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(
        object value,
        System.Type targetType,
        object parameter,
        string language
    )
    {
        return value is Visibility visibility && visibility != Visibility.Visible;
    }
}
