using System;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents a recipient (person or department) who can receive packages
/// Maps to routing_recipients database table
/// </summary>
public class Model_RoutingRecipient
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Recipient name (unique, person or department)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical location (e.g., "Building A - Floor 2")
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Department name (optional)
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Active status (0=inactive, 1=active)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Usage count for sorting (from JOIN with routing_usage_tracking, not stored in table)
    /// </summary>
    public int UsageCount { get; set; } = 0;
}
