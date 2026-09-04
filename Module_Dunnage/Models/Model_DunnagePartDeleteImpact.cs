namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Counts of saved rows that a dunnage-part delete will cascade-remove.
/// Returned by sp_Dunnage_Parts_GetDeleteImpact and used to build the
/// delete-impact warning shown before a part is deleted.
/// </summary>
public sealed class Model_DunnagePartDeleteImpact
{
    /// <summary>Active label-data queue rows referencing the part.</summary>
    public int LabelDataCount { get; set; }

    /// <summary>Archived history rows referencing the part.</summary>
    public int HistoryCount { get; set; }

    /// <summary>Inventory-tracking rows referencing the part.</summary>
    public int InventoryCount { get; set; }

    public bool HasImpact => LabelDataCount > 0 || HistoryCount > 0 || InventoryCount > 0;
}
