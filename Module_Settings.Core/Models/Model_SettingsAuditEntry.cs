using System;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Represents a settings audit log entry.
/// </summary>
public class Model_SettingsAuditEntry
{
    public int Id { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? WorkstationName { get; set; }
}
