using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a quality-hold flag to a text foreground brush.
/// </summary>
public class Converter_QualityHoldToTextColor : IValueConverter
{
    private static readonly SolidColorBrush RedTextBrush = new(Microsoft.UI.Colors.Red);
    private static readonly SolidColorBrush DefaultTextBrush = new(Microsoft.UI.Colors.Black);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool requiresQualityHold && requiresQualityHold
            ? RedTextBrush
            : DefaultTextBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("Converter_QualityHoldToTextColor is one-way only");
    }
}
