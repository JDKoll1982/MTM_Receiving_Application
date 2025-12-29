using System;
using System.Net;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Converters;

/// <summary>
/// Converts an icon code (like &#xE7B8; or E7B8) to a glyph character.
/// </summary>
public class Converter_IconCodeToGlyph : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string iconCode && !string.IsNullOrWhiteSpace(iconCode))
        {
            // Handle HTML entity format: &#xE7B8;
            if (iconCode.StartsWith("&#x") && iconCode.EndsWith(";"))
            {
                string hex = iconCode.Substring(3, iconCode.Length - 4);
                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                {
                    return ((char)code).ToString();
                }
            }

            // Handle raw hex format: E7B8
            if (iconCode.Length == 4 && int.TryParse(iconCode, System.Globalization.NumberStyles.HexNumber, null, out int rawCode))
            {
                return ((char)rawCode).ToString();
            }

            // If it's already a single character or something else, return as is
            return iconCode;
        }

        return "\uE7B8"; // Default box icon
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
