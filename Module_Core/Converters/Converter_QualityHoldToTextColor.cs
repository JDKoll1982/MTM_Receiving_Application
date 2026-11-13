using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Helpers.UI;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a quality-hold flag to a text foreground brush: red while the part is on
/// quality hold, otherwise the theme's own text colour so the part id stays readable in
/// the dark theme.
/// </summary>
public class Converter_QualityHoldToTextColor : IValueConverter
{
    private static readonly SolidColorBrush RedTextBrush = new(Microsoft.UI.Colors.Red);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool requiresQualityHold && requiresQualityHold
            ? RedTextBrush
            : Helper_UI_ThemeTextBrush.PrimaryText;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("Converter_QualityHoldToTextColor is one-way only");
    }
}
