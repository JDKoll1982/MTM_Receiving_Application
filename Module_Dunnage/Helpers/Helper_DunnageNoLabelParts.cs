using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

/// <summary>
/// Splits a receipt between the parts that need a printed label and the parts that do not.
/// </summary>
/// <remarks>
/// A part on the no-label list is saved straight to history, so nothing is ever queued for LabelView for
/// it. The split is done here rather than in the service so the rule can be checked without a database.
/// Part numbers are matched without regard to case or surrounding spaces, because the list and the
/// receiving screens are keyed from different places.
/// </remarks>
public static class Helper_DunnageNoLabelParts
{
    /// <summary>
    /// Divides the loads into the ones that need a label and the ones that should go straight to history.
    /// </summary>
    /// <param name="loads">The loads being saved.</param>
    /// <param name="noLabelPartIds">The part numbers that need no printed label.</param>
    public static (
        List<Model_DunnageLoad> NeedsLabel,
        List<Model_DunnageLoad> NoLabel
    ) Split(IEnumerable<Model_DunnageLoad>? loads, IEnumerable<string>? noLabelPartIds)
    {
        var needsLabel = new List<Model_DunnageLoad>();
        var noLabel = new List<Model_DunnageLoad>();

        if (loads is null)
        {
            return (needsLabel, noLabel);
        }

        var flagged = (noLabelPartIds ?? [])
            .Where(partId => !string.IsNullOrWhiteSpace(partId))
            .Select(partId => partId.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var load in loads)
        {
            if (load is null)
            {
                continue;
            }

            if (flagged.Contains(load.PartId?.Trim() ?? string.Empty))
            {
                noLabel.Add(load);
            }
            else
            {
                needsLabel.Add(load);
            }
        }

        return (needsLabel, noLabel);
    }
}
