using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a Part ID string to a SolidColorBrush for text color (foreground) highlighting.
/// Returns red for restricted parts (MMFSR, MMCSR), black otherwise.
/// Used in WinUI 3 DataGrid for conditional text color in restricted rows.
/// </summary>
public class Converter_PartIDToQualityHoldTextColor : IValueConverter
{
    private static readonly SolidColorBrush RedTextBrush = new(Microsoft.UI.Colors.Red);
    private static readonly SolidColorBrush BlackTextBrush = new(Microsoft.UI.Colors.Black);

    /// <summary>
    /// Converts a part ID string to a text color brush.
    /// </summary>
    /// <param name="value">Part ID string (e.g., "MMFSR05645")</param>
    /// <param name="targetType">Target type (SolidColorBrush)</param>
    /// <param name="parameter">Optional parameter (not used)</param>
    /// <param name="language">Language string (not used)</param>
    /// <returns>SolidColorBrush - Red text for restricted parts, black otherwise</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string partID)
        {
            return BlackTextBrush;
        }

        if (string.IsNullOrWhiteSpace(partID))
        {
            return BlackTextBrush;
        }

        // Check for restricted part patterns: MMFSR or MMCSR
        if (partID.Contains("MMFSR", StringComparison.OrdinalIgnoreCase) ||
            partID.Contains("MMCSR", StringComparison.OrdinalIgnoreCase))
        {
            return RedTextBrush;
        }

        return BlackTextBrush;
    }

    /// <summary>
    /// Not implemented - this is a one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("Converter_PartIDToQualityHoldTextColor is one-way only");
    }
}
