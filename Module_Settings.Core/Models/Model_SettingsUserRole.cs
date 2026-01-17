using System;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Represents a user-role mapping for settings permissions.
/// </summary>
public class Model_SettingsUserRole
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
}
