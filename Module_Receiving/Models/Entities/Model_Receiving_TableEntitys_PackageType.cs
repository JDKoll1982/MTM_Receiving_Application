namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a package type for receiving
/// Maps to: tbl_Receiving_PackageType table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_PackageType
{
    public int PackageTypeId { get; set; }
    public string PackageTypeName { get; set; } = string.Empty;
    public string PackageTypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultPackagesPerLoad { get; set; }
    public decimal? TypicalCapacityLBS { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystemDefault { get; set; }
}
