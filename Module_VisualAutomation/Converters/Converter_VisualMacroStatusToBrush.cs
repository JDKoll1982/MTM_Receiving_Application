using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_VisualAutomation.Enums;
using Windows.UI;

namespace MTM_Receiving_Application.Module_VisualAutomation.Converters;

/// <summary>
/// Maps a probe status to the accent colour used by the status pill on each card.
/// </summary>
public sealed class Converter_VisualMacroStatusToBrush : IValueConverter
{
    private static readonly SolidColorBrush NotRunBrush = new(
        Color.FromArgb(0xFF, 0x8A, 0x8A, 0x8A)
    );
    private static readonly SolidColorBrush RunningBrush = new(
        Color.FromArgb(0xFF, 0x0F, 0x6C, 0xBD)
    );
    private static readonly SolidColorBrush PassedBrush = new(
        Color.FromArgb(0xFF, 0x0F, 0x7B, 0x0F)
    );
    private static readonly SolidColorBrush FailedBrush = new(
        Color.FromArgb(0xFF, 0xC4, 0x2B, 0x1C)
    );

    public object Convert(object? value, Type targetType, object? parameter, string language) =>
        value switch
        {
            Enum_VisualProbeStatus.Passed => PassedBrush,
            Enum_VisualProbeStatus.Failed => FailedBrush,
            Enum_VisualProbeStatus.Running => RunningBrush,
            _ => NotRunBrush,
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, string language) =>
        throw new NotSupportedException();
}
