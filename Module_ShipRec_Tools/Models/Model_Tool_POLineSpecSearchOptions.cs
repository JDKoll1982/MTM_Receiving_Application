using System;
using System.Collections.Generic;

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
        "All",
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

    public string PoStatusFilter { get; set; } = string.Empty;

    public string SearchMode { get; set; } = DefaultSearchMode;

    public int VisibleLines { get; set; } = DefaultVisibleLines;

    public HashSet<string> VisibleColumnKeys { get; set; } =
        new(DefaultVisibleColumnKeys, StringComparer.OrdinalIgnoreCase);

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
