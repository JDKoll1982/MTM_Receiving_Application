using System;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents audit trail entry for label edits
/// Maps to routing_label_history database table
/// </summary>
public class Model_RoutingLabelHistory
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to routing_labels table
    /// </summary>
    public int LabelId { get; set; }

    /// <summary>
    /// Name of field that was changed (e.g., "Recipient", "Quantity")
    /// </summary>
    public string FieldChanged { get; set; } = string.Empty;

    /// <summary>
    /// Old value before edit (nullable)
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after edit (nullable)
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Employee number who made the edit
    /// </summary>
    public int EditedBy { get; set; }

    /// <summary>
    /// Timestamp of edit
    /// </summary>
    public DateTime EditDate { get; set; } = DateTime.Now;
}
