using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One column the lot list and its printed report can show.
/// </summary>
/// <param name="Key">The stable key stored in settings and written into a report.</param>
/// <param name="DisplayName">The heading a user reads.</param>
public sealed record Model_Tool_LotMarkingColumn(string Key, string DisplayName);

/// <summary>
/// The columns the lot list can show, in the order they appear, and the set shown by default.
/// </summary>
/// <remarks>
/// The key is what gets stored, so a display name can be reworded without changing what a saved
/// column choice means. Both the grid and the printed report read this one list, which is why a column
/// turned off in the Filter window is gone from both rather than from only one of them.
/// </remarks>
public static class Model_Tool_LotMarkingColumnCatalog
{
    public const string Shape = "Shape";
    public const string Color = "Color";
    public const string Heat = "Heat";
    public const string Supplier = "Supplier";
    public const string Parts = "Parts";
    public const string Skids = "Skids";
    public const string LastLine = "LastLine";

    /// <summary>Every column, in the order the list shows them.</summary>
    public static IReadOnlyList<Model_Tool_LotMarkingColumn> All { get; } =
    [
        new(Shape, "Shape"),
        new(Color, "Color"),
        new(Heat, "Heat / lot number"),
        new(Supplier, "Supplier"),
        new(Parts, "Part number"),
        new(Skids, "Skids"),
        new(LastLine, "Last received"),
    ];

    /// <summary>The columns a list shows when nobody has chosen otherwise.</summary>
    public static IReadOnlyList<string> DefaultVisibleKeys { get; } = All.Select(column => column.Key).ToList();

    /// <summary>True when the key names a column this list knows about.</summary>
    public static bool IsKnownKey(string? key) =>
        All.Any(column =>
            string.Equals(column.Key, key, StringComparison.OrdinalIgnoreCase)
        );

    /// <summary>
    /// Narrows a stored set to the columns that still exist, keeping the display order. A key that is
    /// no longer offered is dropped rather than resurrected, so a stale saved choice cannot break the
    /// grid.
    /// </summary>
    public static HashSet<string> Normalize(IEnumerable<string>? keys)
    {
        var wanted = new HashSet<string>(
            keys ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase
        );

        return new HashSet<string>(
            All.Where(column => wanted.Contains(column.Key)).Select(column => column.Key),
            StringComparer.OrdinalIgnoreCase
        );
    }
}
