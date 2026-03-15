using System;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Represents a saved non-PO reference entry used during Dunnage Details Entry
/// when no purchase order number is applicable.
/// Database table: dunnage_non_po_entries
/// </summary>
public class Model_DunnageNonPOEntry
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UseCount { get; set; }
}
