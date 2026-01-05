using System;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents usage tracking for personalized Quick Add and smart sorting
/// Maps to routing_usage_tracking database table
/// </summary>
public class Model_RoutingUsageTracking
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Employee number being tracked
    /// </summary>
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Foreign key to routing_recipients table
    /// </summary>
    public int RecipientId { get; set; }

    /// <summary>
    /// Number of times employee selected this recipient
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Timestamp of most recent usage
    /// </summary>
    public DateTime LastUsedDate { get; set; } = DateTime.Now;
}
