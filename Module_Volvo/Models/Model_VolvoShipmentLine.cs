using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents an individual part line item within a Volvo shipment
/// Maps to volvo_shipment_lines database table
/// </summary>
public class Model_VolvoShipmentLine
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to parent shipment
    /// </summary>
    public int ShipmentId { get; set; }

    /// <summary>
    /// Volvo part number (from volvo_parts_master)
    /// Example: V-EMB-500, V-EMB-750
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Number of skids actually received (user-entered count)
    /// </summary>
    public int ReceivedSkidCount { get; set; }

    /// <summary>
    /// Calculated piece count from component explosion
    /// IMPORTANT: Stored at creation time (historical integrity - doesn't change if master data updates)
    /// </summary>
    public int CalculatedPieceCount { get; set; }

    /// <summary>
    /// Flag indicating if there is a discrepancy between expected and received quantities
    /// If true, ExpectedSkidCount and DiscrepancyNote are populated
    /// </summary>
    public bool HasDiscrepancy { get; set; } = false;

    /// <summary>
    /// Expected skid count from Volvo packlist (NULL if no discrepancy)
    /// Used to calculate difference: ReceivedSkidCount - ExpectedSkidCount
    /// </summary>
    public double? ExpectedSkidCount { get; set; }

    /// <summary>
    /// User's note about the discrepancy (NULL if no discrepancy)
    /// </summary>
    public string? DiscrepancyNote { get; set; }
}
