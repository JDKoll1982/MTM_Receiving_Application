using System;

namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a quality hold record with two-step acknowledgment tracking
/// Maps to: tbl_Receiving_QualityHold table in SQL Server
/// User Requirements:
///   - Configurable part patterns (not hardcoded)
///   - Two-step acknowledgment tracking
///   - Full audit trail
///   - Hard blocking on save
/// </summary>
public class Model_Receiving_TableEntitys_QualityHold
{
    /// <summary>
    /// Unique quality hold identifier (GUID)
    /// </summary>
    public string QualityHoldId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to tbl_Receiving_Line
    /// </summary>
    public string LineId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to tbl_Receiving_Transaction
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Part number that triggered the quality hold
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// The configurable pattern that triggered the hold
    /// Example: "MMFSR*", "MMCSR*", "CUSTOM*"
    /// </summary>
    public string PartPattern { get; set; } = string.Empty;

    /// <summary>
    /// Type of restriction (configurable)
    /// Example: "Weight Sensitive", "Quality Control", "Special Handling"
    /// </summary>
    public string RestrictionType { get; set; } = string.Empty;

    /// <summary>
    /// Date/time of first acknowledgment (on part selection)
    /// </summary>
    public DateTime? UserAcknowledgedDate { get; set; }

    /// <summary>
    /// Username who completed first acknowledgment
    /// </summary>
    public string? UserAcknowledgedBy { get; set; }

    /// <summary>
    /// Message displayed during first acknowledgment
    /// </summary>
    public string? UserAcknowledgmentMessage { get; set; }

    /// <summary>
    /// Date/time of final acknowledgment (before save)
    /// </summary>
    public DateTime? FinalAcknowledgedDate { get; set; }

    /// <summary>
    /// Username who completed final acknowledgment
    /// </summary>
    public string? FinalAcknowledgedBy { get; set; }

    /// <summary>
    /// Message displayed during final acknowledgment
    /// </summary>
    public string? FinalAcknowledgmentMessage { get; set; }

    /// <summary>
    /// Whether both acknowledgment steps are complete
    /// Required to be true before save (hard block)
    /// </summary>
    public bool IsFullyAcknowledged { get; set; }

    /// <summary>
    /// Quality inspector name (future feature - not required for MVP)
    /// </summary>
    public string? QualityInspectorName { get; set; }

    /// <summary>
    /// Quality inspector sign-off date (future feature)
    /// </summary>
    public DateTime? QualityInspectorDate { get; set; }

    /// <summary>
    /// Quality inspector notes (future feature)
    /// </summary>
    public string? QualityInspectorNotes { get; set; }

    /// <summary>
    /// Whether the quality hold has been released (future feature)
    /// </summary>
    public bool IsReleased { get; set; }

    /// <summary>
    /// Date when quality hold was released (future feature)
    /// </summary>
    public DateTime? ReleasedDate { get; set; }

    /// <summary>
    /// Load number this quality hold applies to
    /// </summary>
    public int LoadNumber { get; set; }

    /// <summary>
    /// Total weight at time of quality hold
    /// </summary>
    public decimal? TotalWeight { get; set; }

    /// <summary>
    /// Package type at time of quality hold
    /// </summary>
    public string? PackageType { get; set; }

    /// <summary>
    /// General notes about this quality hold
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this record is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this record is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Username who created this record
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date/time when this record was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Username who last modified this record
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Date/time when this record was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Computed property: Whether user acknowledgment (step 1) is complete
    /// </summary>
    public bool IsUserAcknowledged => UserAcknowledgedDate.HasValue && !string.IsNullOrWhiteSpace(UserAcknowledgedBy);

    /// <summary>
    /// Computed property: Whether final acknowledgment (step 2) is complete
    /// </summary>
    public bool IsFinalAcknowledged => FinalAcknowledgedDate.HasValue && !string.IsNullOrWhiteSpace(FinalAcknowledgedBy);

    /// <summary>
    /// Computed property: Acknowledgment status message
    /// </summary>
    public string AcknowledgmentStatus
    {
        get
        {
            if (IsFullyAcknowledged)
                return "Fully Acknowledged (2 of 2)";
            if (IsUserAcknowledged)
                return "Partially Acknowledged (1 of 2)";
            return "Not Acknowledged";
        }
    }

    /// <summary>
    /// Computed property: Whether this quality hold is blocking save
    /// </summary>
    public bool IsBlockingSave => !IsFullyAcknowledged;
}
