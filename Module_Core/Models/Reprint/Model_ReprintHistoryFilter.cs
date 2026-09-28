using System;

namespace MTM_Receiving_Application.Module_Core.Models.Reprint;

/// <summary>
/// Filters used by the Reprint Labels sub-pages when loading history. The date range is not
/// persisted; the Search By column choice is persisted per user via <c>settings_personal</c>.
/// </summary>
public class Model_ReprintHistoryFilter
{
    /// <summary>
    /// Start of the date range. Null means "no lower bound" (used by the All button to search
    /// the entire history).
    /// </summary>
    public DateTime? StartDate { get; set; } = DateTime.Today;

    /// <summary>
    /// End of the date range. Null means "no upper bound" (used by the All button to search
    /// the entire history).
    /// </summary>
    public DateTime? EndDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Normalized Search By column key (e.g. "part", "po", "description", "vendor", "heat",
    /// "type", "quantity"). The per-module stored procedure maps the key to its columns.
    /// </summary>
    public string SearchBy { get; set; } = "part";

    public string SearchText { get; set; } = string.Empty;
}
