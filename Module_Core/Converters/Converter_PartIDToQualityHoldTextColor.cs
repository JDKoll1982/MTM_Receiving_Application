using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Helpers.UI;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a Part ID string to a SolidColorBrush for text color (foreground) highlighting.
/// Returns red for restricted parts (MMFSR, MMCSR), the theme's own text colour otherwise,
/// so an unrestricted part stays readable in the dark theme.
/// Used in WinUI 3 DataGrid for conditional text color in restricted rows.
/// </summary>
public class Converter_PartIDToQualityHoldTextColor : IValueConverter
{
    private static readonly SolidColorBrush RedTextBrush = new(Microsoft.UI.Colors.Red);

    /// <summary>
    /// Converts a part ID string to a text color brush.
    /// </summary>
    /// <param name="value">Part ID string (e.g., "MMFSR05645")</param>
    /// <param name="targetType">Target type (SolidColorBrush)</param>
    /// <param name="parameter">Optional parameter (not used)</param>
    /// <param name="language">Language string (not used)</param>
    /// <returns>SolidColorBrush - Red text for restricted parts, the theme text colour otherwise</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string partID)
        {
            return Helper_UI_ThemeTextBrush.PrimaryText;
        }

        if (string.IsNullOrWhiteSpace(partID))
        {
            return Helper_UI_ThemeTextBrush.PrimaryText;
        }

        // Check for restricted part patterns: MMFSR or MMCSR
        if (
            partID.Contains("MMFSR", StringComparison.OrdinalIgnoreCase)
            || partID.Contains("MMCSR", StringComparison.OrdinalIgnoreCase)
        )
        {
            return RedTextBrush;
        }

        return Helper_UI_ThemeTextBrush.PrimaryText;
    }

    /// <summary>
    /// Not implemented - this is a one-way converter.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="language"></param>
    /// <exception cref="NotImplementedException"></exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("Converter_PartIDToQualityHoldTextColor is one-way only");
    }
}
