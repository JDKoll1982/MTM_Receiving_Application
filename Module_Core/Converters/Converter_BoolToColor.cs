using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a boolean value into one of two named colors provided as "TrueColor|FalseColor".
/// </summary>
public class Converter_BoolToColor : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            var parts = paramString.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                return boolValue ? ResolveBrush(parts[0]) : ResolveBrush(parts[1]);
            }
        }

        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException("Converter_BoolToColor is one-way only");
    }

    private static SolidColorBrush ResolveBrush(string colorName)
    {
        return colorName.Trim().ToLowerInvariant() switch
        {
            "green" => new SolidColorBrush(Colors.Green),
            "gray" => new SolidColorBrush(Colors.Gray),
            "grey" => new SolidColorBrush(Colors.Gray),
            "red" => new SolidColorBrush(Colors.Red),
            "orange" => new SolidColorBrush(Colors.Orange),
            "blue" => new SolidColorBrush(Colors.Blue),
            "black" => new SolidColorBrush(Colors.Black),
            _ => new SolidColorBrush(Colors.Black),
        };
    }
}
