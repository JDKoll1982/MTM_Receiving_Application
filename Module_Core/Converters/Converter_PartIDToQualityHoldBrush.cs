using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a Part ID string to a SolidColorBrush for quality hold highlighting.
/// Returns red brush for restricted parts (MMFSR, MMCSR), transparent/default otherwise.
/// Used in WinUI 3 DataGrid RowStyle for conditional row highlighting.
/// </summary>
public class Converter_PartIDToQualityHoldBrush : IValueConverter
{
    private static readonly SolidColorBrush RedBrush = new(Microsoft.UI.Colors.Red);
    private static readonly SolidColorBrush LightRedBrush = new(Windows.UI.Color.FromArgb(255, 255, 230, 230)); // #FFE6E6
    private static readonly SolidColorBrush TransparentBrush = new(Microsoft.UI.Colors.Transparent);

    /// <summary>
    /// Converts a part ID string to a brush color for highlighting.
    /// </summary>
    /// <param name="value">Part ID string (e.g., "MMFSR05645")</param>
    /// <param name="targetType">Target type (SolidColorBrush)</param>
    /// <param name="parameter">Optional parameter (not used)</param>
    /// <param name="language">Language string (not used)</param>
    /// <returns>SolidColorBrush - Red background for restricted parts, transparent otherwise</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string partID)
        {
            return TransparentBrush;
        }

        if (string.IsNullOrWhiteSpace(partID))
        {
            return TransparentBrush;
        }

        // Check for restricted part patterns: MMFSR or MMCSR
        if (partID.Contains("MMFSR", StringComparison.OrdinalIgnoreCase) ||
            partID.Contains("MMCSR", StringComparison.OrdinalIgnoreCase))
        {
            return LightRedBrush; // Light red background for visibility
        }

        return TransparentBrush;
    }

    /// <summary>
    /// Not implemented - this is a one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("Converter_PartIDToQualityHoldBrush is one-way only");
    }
}
