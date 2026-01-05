namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Constants for Volvo shipment status values
/// Prevents magic strings throughout the codebase
/// </summary>
public static class VolvoShipmentStatus
{
    /// <summary>
    /// Shipment is pending PO creation (user has not yet completed with PO/Receiver numbers)
    /// </summary>
    public const string PendingPo = "pending_po";

    /// <summary>
    /// Shipment has been completed with PO and Receiver numbers
    /// </summary>
    public const string Completed = "completed";

    /// <summary>
    /// Shipment has been archived (soft delete)
    /// </summary>
    public const string Archived = "archived";
}
