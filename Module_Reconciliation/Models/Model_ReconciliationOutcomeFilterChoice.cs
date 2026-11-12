using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Reconciliation.Converters;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One line the outcome filter offers. A null <see cref="Outcome"/> is the
/// "all outcomes" line that clears the filter, so the page can always get back to
/// the whole day (FR-043). The display text comes from
/// <see cref="Converter_ReconciliationOutcomeText"/>, the only place the pinned
/// outcome strings exist (FR-032, FR-050).
/// </summary>
public sealed record Model_ReconciliationOutcomeFilterChoice(
    Enum_ReconciliationOutcome? Outcome,
    string DisplayText
)
{
    /// <summary>Every outcome in declared order, behind the line that clears the filter.</summary>
    public static IReadOnlyList<Model_ReconciliationOutcomeFilterChoice> All { get; } =
    [
        new(null, "All outcomes"),
        .. Enum.GetValues<Enum_ReconciliationOutcome>()
            .Select(outcome => new Model_ReconciliationOutcomeFilterChoice(
                outcome,
                Converter_ReconciliationOutcomeText.GetDisplayText(outcome)
            )),
    ];
}
