using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using Windows.UI;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Converters;

/// <summary>
/// Draws the swatch beside a lot marking's color code. The brush comes from the palette the marking
/// was chosen from, so a code the palette no longer offers still renders as a visible swatch instead
/// of an empty cell.
/// </summary>
public class Converter_LotColorCodeToBrush : IValueConverter
{
    private static readonly SolidColorBrush UnknownColorBrush = new(
        Color.FromArgb(255, 128, 134, 139)
    );

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var color = Model_LotMarkingPalette.FindColor(value as string);

        return color is null ? UnknownColorBrush : ParseBrush(color.HexToken);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language) =>
        throw new NotImplementedException("Converter_LotColorCodeToBrush is one-way only");

    private static SolidColorBrush ParseBrush(string hexToken)
    {
        var hex = hexToken.TrimStart('#');

        if (
            hex.Length != 6
            || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb)
        )
        {
            return UnknownColorBrush;
        }

        return new SolidColorBrush(
            Color.FromArgb(
                255,
                (byte)((rgb >> 16) & 0xFF),
                (byte)((rgb >> 8) & 0xFF),
                (byte)(rgb & 0xFF)
            )
        );
    }
}
