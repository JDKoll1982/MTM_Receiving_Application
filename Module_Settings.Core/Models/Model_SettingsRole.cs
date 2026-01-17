using System;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Represents a settings role.
/// </summary>
public class Model_SettingsRole
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
