using System;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents a quality hold record for restricted part numbers (MMFSR, MMCSR).
/// Maps to receiving_quality_holds database table.
/// </summary>
public class Model_QualityHold
{
    /// <summary>
    /// Unique quality hold identifier.
    /// </summary>
    public int QualityHoldID { get; set; }

    /// <summary>
    /// Reference to the receiving load that contains the restricted part.
    /// </summary>
    public int LoadID { get; set; }

    /// <summary>
    /// Restricted part number (e.g., MMFSR05645, MMCSR12345).
    /// </summary>
    public string PartID { get; set; } = string.Empty;

    /// <summary>
    /// Type of restriction: MMFSR or MMCSR.
    /// </summary>
    public string RestrictionType { get; set; } = string.Empty;

    /// <summary>
    /// Name/ID of quality person who acknowledged the hold.
    /// NULL if not yet acknowledged.
    /// </summary>
    public string? QualityAcknowledgedBy { get; set; }

    /// <summary>
    /// Timestamp when quality acknowledged the load.
    /// NULL if not yet acknowledged.
    /// </summary>
    public DateTime? QualityAcknowledgedAt { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets whether this quality hold has been acknowledged by quality.
    /// </summary>
    public bool IsAcknowledged => QualityAcknowledgedAt.HasValue && !string.IsNullOrEmpty(QualityAcknowledgedBy);
}
