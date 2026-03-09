namespace MTM_Receiving_Application.Module_Bulk_Inventory.Enums;

/// <summary>
/// The type of Infor Visual inventory transaction being performed.
/// Determines which window and fill sequence <c>Service_VisualInventoryAutomation</c> uses.
/// </summary>
public enum Enum_BulkInventoryTransactionType
{
    /// <summary>Inventory Transfer — moves stock between locations using the "Inventory Transfers" window.</summary>
    Transfer,

    /// <summary>New Transaction — posts a work-order issue using the "Inventory Transaction Entry" window.</summary>
    NewTransaction
}
