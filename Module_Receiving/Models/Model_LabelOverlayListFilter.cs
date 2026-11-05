using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// What one Label Overlay tab's list is narrowed and ordered by, and which of its columns are on show.
/// </summary>
/// <remarks>
/// This is the same filter the Heat / Lot Management tool uses — a date range, a sort field with a
/// direction, and a set of visible columns — so an operator who has used that screen already knows this
/// one. Each tab keeps its own copy, and each copy is saved per user, so narrowing the cut-out lots never
/// narrows the overlay tags.
/// </remarks>
/// <param name="Filter">The date range and ordering.</param>
/// <param name="VisibleColumnKeys">The columns chosen to be shown.</param>
public sealed record Model_LabelOverlayListFilter(
    Model_Tool_LotMarkingFilter Filter,
    IReadOnlyList<string> VisibleColumnKeys
)
{
    /// <summary>The keys this list can show, in menu order.</summary>
    public static IReadOnlyList<Model_Tool_LotMarkingColumn> Columns =>
        Model_Tool_LotMarkingColumnCatalog.All;

    /// <summary>Every column showing, no date limit, most recently received first.</summary>
    public static Model_LabelOverlayListFilter Default { get; } =
        new(Model_Tool_LotMarkingFilter.Default, Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys);

    /// <summary>True when either end of the range is set.</summary>
    public bool HasDateRange => Filter.HasDateRange;

    /// <summary>True when the range is set but runs backwards, so it can never match anything.</summary>
    public bool IsBackwardsRange => Filter.IsBackwardsRange;

    /// <summary>Whether a column should be shown, falling back to showing it for an unknown key.</summary>
    public bool IsColumnVisible(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        // An empty set means the user cleared every column, which reads as "show them all" rather than
        // leaving a list with nothing in it.
        return VisibleColumnKeys.Count == 0
            || VisibleColumnKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether a date falls inside the range. A null end is open, and a backwards range keeps the list
    /// empty rather than quietly swapping the ends.
    /// </summary>
    public bool IncludesDate(DateTime? value)
    {
        if (IsBackwardsRange)
        {
            return false;
        }

        if (Filter.FromDate is null && Filter.ToDate is null)
        {
            return true;
        }

        if (value is null)
        {
            return false;
        }

        var day = value.Value.Date;

        return (Filter.FromDate is null || day >= Filter.FromDate.Value.Date)
            && (Filter.ToDate is null || day <= Filter.ToDate.Value.Date);
    }

    /// <summary>The stored field name, so a saved filter survives a change to the enum's numbering.</summary>
    public string SortFieldName => Filter.SortField.ToString();
}
