namespace MTM_Receiving_Application.Module_Bulk_Inventory.Enums;

/// <summary>
/// Lifecycle status of a single bulk inventory transaction row.
/// </summary>
public enum Enum_BulkInventoryStatus
{
    /// <summary>Row has been added to the batch but automation has not started on it yet.</summary>
    Pending,

    /// <summary>Automation is currently filling fields in Visual for this row.</summary>
    InProgress,

    /// <summary>
    /// Automation filled all fields and sent TAB on To Location; waiting for the user to dismiss
    /// the duplicate-transaction warning popup before the row can be marked Success.
    /// </summary>
    WaitingForConfirmation,

    /// <summary>Visual confirmed the transaction was committed successfully.</summary>
    Success,

    /// <summary>An error occurred during automation; see <c>ErrorMessage</c> for details.</summary>
    Failed,

    /// <summary>Row was skipped via the F6 hotkey or because the batch was cancelled.</summary>
    Skipped,

    /// <summary>
    /// Row was merged into another row during pre-push consolidation
    /// (same PartId + FromLocation + ToLocation — quantities summed).
    /// </summary>
    Consolidated
}
