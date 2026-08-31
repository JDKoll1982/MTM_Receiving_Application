using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MTM_Receiving_Application.Module_Core.Converters;

/// <summary>
/// Converts a delivery/PO status key (Closed | Late | Partial | OnTime) to the
/// status-dot brush used on the Delivery Schedule grid. Mirrors the legend:
/// Closed = red, Late = dark red, Partial = yellow, On Time/Open = green.
/// </summary>
public class Converter_DeliveryStatusToBrush : IValueConverter
{
    private static readonly SolidColorBrush ClosedBrush = new(
        Color.FromArgb(255, 13, 110, 253)
    ); // #0D6EFD (blue)
    private static readonly SolidColorBrush LateBrush = new(
        Color.FromArgb(255, 217, 4, 41)
    ); // #D90429
    private static readonly SolidColorBrush PartialBrush = new(
        Color.FromArgb(255, 255, 193, 7)
    ); // #FFC107
    private static readonly SolidColorBrush OnTimeBrush = new(
        Color.FromArgb(255, 40, 167, 69)
    ); // #28A745
    private static readonly SolidColorBrush DefaultBrush = OnTimeBrush;

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return (value as string)?.ToUpperInvariant() switch
        {
            "CLOSED" => ClosedBrush,
            "LATE" => LateBrush,
            "PARTIAL" => PartialBrush,
            "ONTIME" => OnTimeBrush,
            _ => DefaultBrush,
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException("Converter_DeliveryStatusToBrush is one-way only");
    }
}
