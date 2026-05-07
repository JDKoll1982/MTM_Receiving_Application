using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Represents one resolved Infor Visual inventory transfer between a source and destination location.
/// </summary>
public sealed class Model_InforVisualLocationTransferMovement
{
    public string PartId { get; set; } = string.Empty;

    public string PONumber { get; set; } = string.Empty;

    public string POLineNumber { get; set; } = string.Empty;

    public string SourceWarehouseId { get; set; } = string.Empty;

    public string SourceLocationId { get; set; } = string.Empty;

    public string DestinationWarehouseId { get; set; } = string.Empty;

    public string DestinationLocationId { get; set; } = string.Empty;

    public decimal TransferQuantity { get; set; }

    public DateTime TransactionDate { get; set; }

    public string TransactionUserId { get; set; } = string.Empty;

    public int? SourceTransactionId { get; set; }

    public int? DestinationTransactionId { get; set; }
}