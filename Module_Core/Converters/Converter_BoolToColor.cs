using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
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
            "theme" => ResolveThemeForegroundBrush(),
            _ => new SolidColorBrush(Colors.Black),
        };
    }

    /// <summary>
    /// The normal text brush, so a value can be coloured only when a flag is set and otherwise
    /// keep the theme's own foreground instead of a hard-coded colour.
    /// </summary>
    private static SolidColorBrush ResolveThemeForegroundBrush()
    {
        var brush = Application.Current?.Resources is { } resources
            && resources.TryGetValue("TextFillColorPrimaryBrush", out var resolved)
                ? resolved as SolidColorBrush
                : null;

        return brush ?? new SolidColorBrush(Colors.White);
    }
}
