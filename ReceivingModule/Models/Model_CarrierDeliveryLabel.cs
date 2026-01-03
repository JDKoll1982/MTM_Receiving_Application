using System;

namespace MTM_Receiving_Application.ReceivingModule.Models;

/// <summary>
/// Represents a carrier delivery label with shipping information (UPS/FedEx/USPS)
/// Used by receiving clerk to determine package routing and delivery destination
/// Maps to label_table_parcel database table
/// </summary>
public class Model_CarrierDeliveryLabel
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Destination person or department name
    /// </summary>
    public string DeliverTo { get; set; } = string.Empty;

    /// <summary>
    /// Department identifier
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Description of package contents
    /// </summary>
    public string PackageDescription { get; set; } = string.Empty;

    /// <summary>
    /// Purchase order number (optional)
    /// </summary>
    public int? PONumber { get; set; }

    /// <summary>
    /// Work order number (optional)
    /// </summary>
    public string WorkOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Employee number who created the label
    /// </summary>
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Label number for multiple labels
    /// </summary>
    public int LabelNumber { get; set; } = 1;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
