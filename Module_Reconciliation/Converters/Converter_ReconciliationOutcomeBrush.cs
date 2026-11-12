using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Converters;

/// <summary>
/// Turns a finding outcome into the grid row brush. Row colour is the only way an
/// outcome is conveyed in the grid; there is no status column (FR-050).
/// The tints are translucent so they read on both the light and the dark theme.
/// </summary>
public sealed class Converter_ReconciliationOutcomeBrush : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Enum_ReconciliationOutcome outcome)
        {
            return Transparent();
        }

        return new SolidColorBrush(ToRowColor(outcome));
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException("An outcome brush cannot be read back.");

    private static Color ToRowColor(Enum_ReconciliationOutcome outcome) =>
        outcome switch
        {
            Enum_ReconciliationOutcome.Unchanged => Color.FromArgb(0x00, 0x00, 0x00, 0x00),
            Enum_ReconciliationOutcome.Updated => Color.FromArgb(0x33, 0x2E, 0x7D, 0x32),
            Enum_ReconciliationOutcome.ExtraSurplus => Color.FromArgb(0x33, 0xF9, 0xA8, 0x25),
            Enum_ReconciliationOutcome.NotEnough => Color.FromArgb(0x33, 0xC6, 0x28, 0x28),
            Enum_ReconciliationOutcome.NotFound => Color.FromArgb(0x26, 0x9E, 0x9E, 0x9E),
            Enum_ReconciliationOutcome.Skipped => Color.FromArgb(0x1A, 0x9E, 0x9E, 0x9E),
            Enum_ReconciliationOutcome.Error => Color.FromArgb(0x40, 0xB7, 0x1C, 0x1C),
            Enum_ReconciliationOutcome.ManuallySet => Color.FromArgb(0x33, 0x00, 0x83, 0x8F),
            _ => Color.FromArgb(0x00, 0x00, 0x00, 0x00),
        };

    private static SolidColorBrush Transparent() =>
        new(Color.FromArgb(0x00, 0x00, 0x00, 0x00));
}
