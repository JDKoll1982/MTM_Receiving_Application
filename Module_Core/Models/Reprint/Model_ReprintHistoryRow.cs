using System;

namespace MTM_Receiving_Application.Module_Core.Models.Reprint;

/// <summary>
/// A single history row surfaced on a Reprint Labels sub-page. Kept intentionally compact
/// (date, part, quantity, reference) and module-agnostic so the Reprint page renders the
/// same grid shape for Receiving, Dunnage, and Volvo.
/// </summary>
public class Model_ReprintHistoryRow
{
    /// <summary>
    /// Canonical history identifier used to reprint this row. For Receiving and Volvo this is
    /// the numeric history row id; for Dunnage it is the history <c>load_uuid</c>.
    /// </summary>
    public string HistoryId { get; set; } = string.Empty;

    public DateTime RecordDate { get; set; }

    public string Part { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    /// <summary>
    /// Module-specific reference shown in the grid (PO number, shipment number, label number, ...).
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// True when an <c>is_reprint = 1</c> row already exists in the active label queue for this
    /// history record. Such rows render red and cannot be selected for reprint.
    /// </summary>
    public bool AlreadyQueued { get; set; }
}
