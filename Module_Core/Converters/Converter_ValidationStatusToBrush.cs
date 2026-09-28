using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a nullable validation status into a status brush: true = green (ready),
/// false = red (needs attention), null = gray (not yet validated).
/// </summary>
public class Converter_ValidationStatusToBrush : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var color = value switch
        {
            true => Colors.Green,
            false => Colors.Red,
            _ => Colors.Gray,
        };

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException("Converter_ValidationStatusToBrush is one-way only");
    }
}
