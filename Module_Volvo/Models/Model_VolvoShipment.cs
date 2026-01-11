using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents a Volvo dunnage shipment header
/// Maps to volvo_label_data database table
/// </summary>
public class Model_VolvoShipment
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Date of shipment arrival
    /// </summary>
    public DateTime ShipmentDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Auto-generated sequential number for the day (resets daily)
    /// Example: Shipment #1, Shipment #2 for the same date
    /// </summary>
    public int ShipmentNumber { get; set; }

    /// <summary>
    /// Purchase order number (filled after purchasing department provides it)
    /// NULL until shipment is completed
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Receiver number from Infor Visual (filled after PO receiving)
    /// NULL until shipment is completed
    /// </summary>
    public string? ReceiverNumber { get; set; }

    /// <summary>
    /// Employee number from authentication context
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes about the shipment
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Status: 'pending_po' (awaiting PO from purchasing) or 'completed' (PO/Receiver entered)
    /// </summary>
    public string Status { get; set; } = "pending_po";

    /// <summary>
    /// Timestamp when shipment was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Timestamp when shipment was last modified
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Flag indicating if shipment has been archived to history
    /// Set to true when status changes to 'completed'
    /// </summary>
    public bool IsArchived { get; set; } = false;
}
