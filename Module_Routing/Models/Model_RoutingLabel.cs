using System;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents an internal routing label for packages/materials
/// Maps to routing_label_data database table
/// </summary>
public class Model_RoutingLabel
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Purchase order number or "OTHER" for non-PO packages
    /// </summary>
    public string PONumber { get; set; } = string.Empty;

    /// <summary>
    /// Line item number or "0" for OTHER packages
    /// </summary>
    public string LineNumber { get; set; } = string.Empty;

    /// <summary>
    /// Part/material description (from Infor Visual or manual entry)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to routing_recipients table
    /// </summary>
    public int RecipientId { get; set; }

    /// <summary>
    /// Recipient name (from JOIN, not stored in table)
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Recipient location (from JOIN, not stored in table)
    /// </summary>
    public string RecipientLocation { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of items on label
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Employee number who created the label
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Label creation timestamp
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Foreign key to routing_po_alternatives table (nullable, required if PONumber='OTHER')
    /// </summary>
    public int? OtherReasonId { get; set; }

    /// <summary>
    /// Other reason description (from JOIN, not stored in table)
    /// </summary>
    public string? OtherReasonDescription { get; set; }

    /// <summary>
    /// Soft delete flag (0=deleted, 1=active)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Work Order ID (if linked) - Not stored in DB currently, used for CSV
    /// </summary>
    public string WorkOrder { get; set; } = string.Empty;

    /// <summary>
    /// Customer Order ID (if linked) - Not stored in DB currently, used for CSV
    /// </summary>
    public string CustomerOrder { get; set; } = string.Empty;

    /// <summary>
    /// Whether label has been exported to CSV file
    /// </summary>
    public bool CsvExported { get; set; } = false;

    /// <summary>
    /// Timestamp of CSV export (nullable)
    /// </summary>
    public DateTime? CsvExportDate { get; set; }
}
