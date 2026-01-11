using System;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents a Volvo dunnage part in the master catalog
/// Maps to volvo_masterdata database table
/// </summary>
public class Model_VolvoPart
{
    /// <summary>
    /// Part number (primary key)
    /// Example: V-EMB-1, V-EMB-500, V-EMB-750
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Number of pieces per skid for this part
    /// Example: V-EMB-2 = 20 pieces/skid, V-EMB-500 = 88 pieces/skid
    /// </summary>
    public int QuantityPerSkid { get; set; }

    /// <summary>
    /// Flag indicating if part is active
    /// If false, part is hidden from dropdown lists but still referenced in historical data
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when part was created in the system
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Timestamp when part was last modified
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
}
