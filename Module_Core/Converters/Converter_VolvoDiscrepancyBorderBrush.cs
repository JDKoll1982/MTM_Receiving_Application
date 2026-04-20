using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MTM_Receiving_Application.Module_Core.Converters;

public class Converter_VolvoDiscrepancyBorderBrush : IValueConverter
{
    private static readonly SolidColorBrush DefaultBorderBrush = new(
        Color.FromArgb(255, 148, 163, 184)
    );
    private static readonly SolidColorBrush WarningBorderBrush = new(
        Color.FromArgb(255, 217, 119, 6)
    );

    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        return value is bool hasDiscrepancy && hasDiscrepancy
            ? WarningBorderBrush
            : DefaultBorderBrush;
    }

    public object ConvertBack(
        object value,
        System.Type targetType,
        object parameter,
        string language
    )
    {
        return false;
    }
}
