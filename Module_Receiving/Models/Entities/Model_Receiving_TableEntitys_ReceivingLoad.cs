using System;

namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents an individual load within a receiving transaction
/// Maps to: receiving_loads table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_ReceivingLoad
{
    /// <summary>
    /// Unique load identifier (auto-generated)
    /// </summary>
    public int LoadId { get; set; }

    /// <summary>
    /// Parent transaction ID (foreign key)
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Sequential load number within transaction (1, 2, 3, ...)
    /// </summary>
    public int LoadNumber { get; set; }

    /// <summary>
    /// Part number/ID
    /// Format: MMC0001000, MMF0002000, etc.
    /// </summary>
    public string PartId { get; set; } = string.Empty;

    /// <summary>
    /// Part type (from enum or auto-detected from prefix)
    /// </summary>
    public string? PartType { get; set; }

    /// <summary>
    /// Total weight or quantity for this load
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of measure (LBS, KG, EA, etc.)
    /// Default: LBS
    /// </summary>
    public string UnitOfMeasure { get; set; } = "LBS";

    /// <summary>
    /// Heat lot number or batch identifier
    /// </summary>
    public string? HeatLotNumber { get; set; }

    /// <summary>
    /// Package type (Skid, Pallet, Box, etc.)
    /// </summary>
    public string? PackageType { get; set; }

    /// <summary>
    /// Number of packages in this load
    /// Default: 1
    /// </summary>
    public int PackagesPerLoad { get; set; } = 1;

    /// <summary>
    /// Weight per individual package (calculated or entered)
    /// </summary>
    public decimal? WeightPerPackage { get; set; }

    /// <summary>
    /// Receiving location (warehouse location code)
    /// </summary>
    public string? ReceivingLocation { get; set; }

    /// <summary>
    /// Whether quality hold was acknowledged for this load
    /// </summary>
    public bool QualityHoldAcknowledged { get; set; }

    /// <summary>
    /// Date/time when quality hold was acknowledged
    /// </summary>
    public DateTime? QualityHoldAcknowledgedAt { get; set; }

    /// <summary>
    /// Whether this load requires quality hold based on part pattern
    /// </summary>
    public bool IsQualityHoldRequired { get; set; }

    /// <summary>
    /// Type of quality hold restriction (e.g., "Weight Sensitive", "Quality Control")
    /// </summary>
    public string? QualityHoldRestrictionType { get; set; }

    /// <summary>
    /// Whether user acknowledged quality hold (step 1 of 2)
    /// </summary>
    public bool UserAcknowledgedQualityHold { get; set; }

    /// <summary>
    /// Whether final acknowledgment was completed (step 2 of 2)
    /// Hard block: Cannot save without this being true
    /// </summary>
    public bool FinalAcknowledgedQualityHold { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Record last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to parent transaction
    /// </summary>
    public Model_Receiving_TableEntitys_ReceivingTransaction? Transaction { get; set; }
}
