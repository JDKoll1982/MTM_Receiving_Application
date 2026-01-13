using System;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents PO and line information from Infor Visual ERP
/// Used for PO validation and description retrieval
/// This is NOT a database table - it's returned from Infor Visual queries
/// </summary>
public class Model_InforVisualPOLine
{
    /// <summary>
    /// Purchase order number
    /// </summary>
    public string PONumber { get; set; } = string.Empty;

    /// <summary>
    /// Line item number
    /// </summary>
    public string LineNumber { get; set; } = string.Empty;

    /// <summary>
    /// Part/material ID from Visual
    /// </summary>
    public string PartID { get; set; } = string.Empty;

    /// <summary>
    /// Part description from Visual
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ordered quantity
    /// </summary>
    public decimal QuantityOrdered { get; set; }

    /// <summary>
    /// Received quantity (to date)
    /// </summary>
    public decimal QuantityReceived { get; set; }

    /// <summary>
    /// Remaining quantity (ordered - received)
    /// </summary>
    public decimal QuantityRemaining => QuantityOrdered - QuantityReceived;

    /// <summary>
    /// Vendor name
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Full specification text from PURC_LINE_BINARY
    /// </summary>
    public string Specifications { get; set; } = string.Empty;

    /// <summary>
    /// Truncated specification text for grid display (first 50 chars)
    /// </summary>
    public string SpecificationsPreview => Specifications.Length > 50 
        ? Specifications.Substring(0, 50) + "..." 
        : Specifications;

    /// <summary>
    /// Expected delivery date (nullable)
    /// </summary>
    public DateTime? DueDate { get; set; }
}
