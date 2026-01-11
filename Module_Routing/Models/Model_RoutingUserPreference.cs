using System;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Represents user preferences for routing module (default mode, validation toggle)
/// Maps to settings_routing_personal database table
/// </summary>
public class Model_RoutingUserPreference
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Employee number (unique)
    /// </summary>
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Default mode: "WIZARD", "MANUAL", or "EDIT"
    /// </summary>
    public string DefaultMode { get; set; } = "WIZARD";

    /// <summary>
    /// Infor Visual validation toggle (true=on, false=off)
    /// </summary>
    public bool EnableValidation { get; set; } = true;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
