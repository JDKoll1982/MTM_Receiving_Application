namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a part type classification
/// Maps to: tbl_Receiving_PartType table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_PartType
{
    public int PartTypeId { get; set; }
    public string PartTypeName { get; set; } = string.Empty;
    public string PartTypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PartPrefixes { get; set; }
    public bool RequiresDiameter { get; set; }
    public bool RequiresWidth { get; set; }
    public bool RequiresLength { get; set; }
    public bool RequiresThickness { get; set; }
    public bool RequiresWeight { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystemDefault { get; set; }
}
