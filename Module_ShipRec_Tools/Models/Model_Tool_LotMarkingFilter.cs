using System;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// What the lot list is narrowed and ordered by. Held as one value so a filter can be passed around,
/// stored, and handed to a report without the parts drifting out of step with each other.
/// </summary>
/// <param name="FromDate">The first day to include, or null for no start.</param>
/// <param name="ToDate">The last day to include, or null for no end.</param>
/// <param name="SortField">The column the list is ordered by.</param>
/// <param name="IsDescending">Whether the order runs from high to low rather than low to high.</param>
public sealed record Model_Tool_LotMarkingFilter(
    DateTime? FromDate,
    DateTime? ToDate,
    Enum_LotMarkingSortField SortField,
    bool IsDescending
)
{
    /// <summary>The filter a list opens with: no date limit, most recent receipt first.</summary>
    public static Model_Tool_LotMarkingFilter Default { get; } =
        new(null, null, Enum_LotMarkingSortField.LastReceiptDate, IsDescending: true);

    /// <summary>True when either end of the range is set, so the list is limited by date.</summary>
    public bool HasDateRange => FromDate is not null || ToDate is not null;

    /// <summary>
    /// True when the range is set but runs backwards, which can never match a receipt. Kept as a
    /// property rather than a validation error so the dialog can say plainly what is wrong.
    /// </summary>
    public bool IsBackwardsRange => FromDate is not null && ToDate is not null && FromDate > ToDate;
}
