namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a system or user setting
/// Maps to: tbl_Receiving_Settings table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_Setting
{
    public int SettingId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string SettingType { get; set; } = "String";
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string Scope { get; set; } = "System";
    public string? ScopeUserId { get; set; }
    public string? ValidValues { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool RequiresRestart { get; set; }
}
