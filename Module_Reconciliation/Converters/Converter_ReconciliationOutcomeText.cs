using System;
using Microsoft.UI.Xaml.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Converters;

/// <summary>
/// Turns a finding outcome into the exact display string the spec pins. This pair
/// of converters is the only place those strings exist (FR-032, FR-050).
/// </summary>
public sealed class Converter_ReconciliationOutcomeText : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is Enum_ReconciliationOutcome outcome ? GetDisplayText(outcome) : string.Empty;

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException("Outcome display text cannot be read back.");

    /// <summary>The pinned display text for one outcome.</summary>
    /// <param name="outcome">The outcome to name.</param>
    public static string GetDisplayText(Enum_ReconciliationOutcome outcome) =>
        outcome switch
        {
            Enum_ReconciliationOutcome.Unchanged => "Unchanged",
            Enum_ReconciliationOutcome.Updated => "Updated",
            Enum_ReconciliationOutcome.ExtraSurplus => "Extra Surplus",
            Enum_ReconciliationOutcome.NotEnough => "Not Enough",
            Enum_ReconciliationOutcome.NotFound => "NotFound",
            Enum_ReconciliationOutcome.Skipped => "Skipped",
            Enum_ReconciliationOutcome.Error => "Error",
            Enum_ReconciliationOutcome.ManuallySet => "Manually Set",
            _ => string.Empty,
        };
}
