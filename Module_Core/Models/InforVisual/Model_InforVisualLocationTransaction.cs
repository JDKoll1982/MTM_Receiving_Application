using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Represents one Infor Visual transaction-history row used during reconciliation smart-fix logic.
/// </summary>
public sealed class Model_InforVisualLocationTransaction
{
    public string SourceLoadId { get; set; } = string.Empty;

    public string WarehouseId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public DateTime TransactionDate { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int? TransactionId { get; set; }

    public string ReceiptWarehouseId { get; set; } = string.Empty;

    public string ReceiptLocationId { get; set; } = string.Empty;
}