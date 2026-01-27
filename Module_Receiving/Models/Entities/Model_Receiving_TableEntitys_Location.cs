namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a warehouse receiving location
/// Maps to: tbl_Receiving_Location table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_Location
{
    public int LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool AllowReceiving { get; set; }
    public bool IsQualityHoldArea { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystemDefault { get; set; }
}
