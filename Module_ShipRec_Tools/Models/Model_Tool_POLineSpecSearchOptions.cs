using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// User-configurable options for the PO line spec search tool.
/// </summary>
public sealed class Model_Tool_POLineSpecSearchOptions
{
    public const int DefaultVisibleLines = 200;

    public const string DefaultSearchMode = "Weighted Ranking";

    public static readonly IReadOnlyList<int> VisibleLinesOptions = [20, 50, 100, 200, 500, 1000];

    public static readonly IReadOnlyList<string> PoStatusFilterOptions =
    [
        "Firmed",
        "Released",
        "Closed",
        "Cancelled/Void",
    ];

    public static readonly IReadOnlyList<string> SearchModeOptions =
    [
        "Exact Phrase",
        "Tokenized Partial",
        "Weighted Ranking",
    ];

    public static readonly IReadOnlyList<string> DefaultVisibleColumnKeys =
    [
        "PONumber",
        "POLineNumber",
        "PartId",
        "VendorName",
        "SpecExcerpt",
        "MatchScore",
    ];

    public static readonly IReadOnlyList<string> AvailableColumnKeys =
    [
        "PONumber",
        "POLineNumber",
        "PartId",
        "VendorName",
        "VendorId",
        "VendorPartId",
        "QtyOrdered",
        "TotalQtyReceived",
        "PoStatus",
        "SpecExcerpt",
        "MatchScore",
    ];

    /// <summary>
    /// The PO statuses the search is limited to, as a comma-separated list of
    /// <see cref="PoStatusFilterOptions"/> names (for example "Firmed,Released"). Empty means every
    /// status, which is what no checkbox ticked stores.
    /// </summary>
    public string PoStatusFilter { get; set; } = string.Empty;

    public string SearchMode { get; set; } = DefaultSearchMode;

    public int VisibleLines { get; set; } = DefaultVisibleLines;

    public HashSet<string> VisibleColumnKeys { get; set; } =
        new(DefaultVisibleColumnKeys, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The stored status filter as the individual status names it holds.
    /// </summary>
    /// <param name="value">The comma-separated stored value.</param>
    /// <returns>One canonical status name per selected status; empty when nothing is selected.</returns>
    public static IReadOnlyList<string> ParsePoStatusFilters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var selected = new List<string>();

        foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var canonical = PoStatusFilterOptions.FirstOrDefault(option =>
                string.Equals(option, part, StringComparison.OrdinalIgnoreCase)
            );

            // An unknown name can only come from an older save or hand-edited settings; keeping it
            // would silently hide every row, so it is dropped.
            if (canonical is not null && !selected.Contains(canonical, StringComparer.OrdinalIgnoreCase))
            {
                selected.Add(canonical);
            }
        }

        return selected;
    }

    /// <summary>
    /// Builds the stored value from the statuses that are ticked.
    /// </summary>
    /// <param name="statuses">The ticked statuses, in any order.</param>
    /// <returns>The comma-separated value in the order the options are listed.</returns>
    public static string BuildPoStatusFilter(IEnumerable<string>? statuses)
    {
        if (statuses is null)
        {
            return string.Empty;
        }

        var ticked = statuses.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return string.Join(
            ',',
            PoStatusFilterOptions.Where(option => ticked.Contains(option))
        );
    }

    /// <summary>
    /// True when every status is selected or none is, in which case the search does not need a
    /// status filter at all.
    /// </summary>
    public bool IsEveryPoStatusSelected
    {
        get
        {
            var selected = ParsePoStatusFilters(PoStatusFilter);
            return selected.Count == 0 || selected.Count == PoStatusFilterOptions.Count;
        }
    }

    public Model_Tool_POLineSpecSearchOptions Clone()
    {
        return new Model_Tool_POLineSpecSearchOptions
        {
            PoStatusFilter = PoStatusFilter,
            SearchMode = SearchMode,
            VisibleLines = VisibleLines,
            VisibleColumnKeys = new HashSet<string>(VisibleColumnKeys, StringComparer.OrdinalIgnoreCase),
        };
    }
}
