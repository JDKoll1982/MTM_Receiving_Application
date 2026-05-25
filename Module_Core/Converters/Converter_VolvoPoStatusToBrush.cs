using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Volvo.Models;
using Windows.UI;

namespace MTM_Receiving_Application.Module_Core.Converters;

public class Converter_VolvoPoStatusToBrush : IValueConverter
{
    private static readonly SolidColorBrush PendingBrush = new(Color.FromArgb(255, 254, 249, 195));
    private static readonly SolidColorBrush ReceivedBrush = new(Color.FromArgb(255, 220, 252, 231));
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromArgb(255, 248, 250, 252));

    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        var normalizedStatus = VolvoLinePoStatus.NormalizeStorageValue(value?.ToString());

        return normalizedStatus switch
        {
            VolvoLinePoStatus.Pending => PendingBrush,
            VolvoLinePoStatus.Received => ReceivedBrush,
            _ => DefaultBrush,
        };
    }

    public object ConvertBack(
        object value,
        System.Type targetType,
        object parameter,
        string language
    )
    {
        return VolvoLinePoStatus.Pending;
    }
}
