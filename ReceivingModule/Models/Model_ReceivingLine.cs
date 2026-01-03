using System;

namespace MTM_Receiving_Application.ReceivingModule.Models;

/// <summary>
/// Represents a receiving label entry from Google Sheets "Receiving Data"
/// Maps to label_table_receiving database table
/// </summary>
public class Model_ReceivingLine
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Number of pieces received (Column A in Google Sheets)
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Part identifier (Column B in Google Sheets)
    /// </summary>
    public string PartID { get; set; } = string.Empty;

    /// <summary>
    /// Purchase order number (Column C in Google Sheets)
    /// </summary>
    public int PONumber { get; set; }

    /// <summary>
    /// Receiving employee number (Column D in Google Sheets)
    /// </summary>
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Heat/lot number (Column E in Google Sheets)
    /// </summary>
    public string Heat { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date (Column F in Google Sheets)
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Storage location (Column G in Google Sheets)
    /// </summary>
    public string InitialLocation { get; set; } = string.Empty;

    /// <summary>
    /// Optional coil count (Column H in Google Sheets)
    /// </summary>
    public int? CoilsOnSkid { get; set; }

    /// <summary>
    /// Current label number (calculated)
    /// </summary>
    public int LabelNumber { get; set; } = 1;

    /// <summary>
    /// Total labels for this PO (calculated)
    /// </summary>
    public int TotalLabels { get; set; } = 1;

    /// <summary>
    /// Supplier name (looked up from part/PO)
    /// </summary>
    public string VendorName { get; set; } = "Unknown";

    /// <summary>
    /// Part description (looked up from part ID)
    /// </summary>
    public string PartDescription { get; set; } = string.Empty;

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Formatted label text (e.g., "1 / 5")
    /// </summary>
    public string LabelText => $"{LabelNumber} / {TotalLabels}";
}
