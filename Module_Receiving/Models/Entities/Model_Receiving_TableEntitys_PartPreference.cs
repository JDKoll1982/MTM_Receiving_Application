using System;

namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a part preference configuration
/// Maps to: tbl_Receiving_PartPreference table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_PartPreference
{
    public int PartPreferenceId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public int? PartTypeId { get; set; }
    public string? PartTypeName { get; set; }
    public string? PartTypeCode { get; set; }
    public string? DefaultReceivingLocation { get; set; }
    public string? DefaultPackageType { get; set; }
    public int? DefaultPackagesPerLoad { get; set; }
    public bool RequiresQualityHold { get; set; }
    public string? QualityHoldProcedure { get; set; }
    public string Scope { get; set; } = "System";
    public string? ScopeUserId { get; set; }
}
