using System;

namespace MTM_Receiving_Application.Models.Dunnage;

/// <summary>
/// Represents a dunnage/packing material label entry
/// Maps to label_table_dunnage database table
/// </summary>
public class Model_DunnageLine
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// First line of dunnage label text
    /// </summary>
    public string Line1 { get; set; } = string.Empty;

    /// <summary>
    /// Second line of dunnage label text
    /// </summary>
    public string Line2 { get; set; } = string.Empty;

    /// <summary>
    /// Purchase order number
    /// </summary>
    public int PONumber { get; set; }

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Employee number who created the label
    /// </summary>
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Supplier/vendor name
    /// </summary>
    public string VendorName { get; set; } = "Unknown";

    /// <summary>
    /// Storage or destination location
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Label number for multiple labels
    /// </summary>
    public int LabelNumber { get; set; } = 1;

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
