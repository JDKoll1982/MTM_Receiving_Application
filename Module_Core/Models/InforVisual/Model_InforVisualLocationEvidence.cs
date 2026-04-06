using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Represents the available Infor Visual evidence used to infer a receipt's current location.
/// </summary>
public sealed class Model_InforVisualLocationEvidence
{
    public string CurrentWarehouseId { get; set; } = string.Empty;

    public string CurrentLocationId { get; set; } = string.Empty;

    public decimal CurrentQuantity { get; set; }

    public decimal MatchedTransactionQuantity { get; set; }

    public int MatchedTransactionCount { get; set; }

    public DateTime? MatchedTransactionDate { get; set; }

    public string MatchedTransactionUserId { get; set; } = string.Empty;

    public int? MatchedTransactionId { get; set; }

    public int ReceiptCount { get; set; }

    public DateTime? FirstReceivedDate { get; set; }

    public DateTime? LastReceivedDate { get; set; }

    public string LatestReceiptWarehouseId { get; set; } = string.Empty;

    public string LatestReceiptLocationId { get; set; } = string.Empty;

    public DateTime? LatestReceiptEvidenceDate { get; set; }

    public string LatestTransactionWarehouseId { get; set; } = string.Empty;

    public string LatestTransactionLocationId { get; set; } = string.Empty;

    public DateTime? LatestTransactionDate { get; set; }

    public int? LatestTransactionId { get; set; }

    public decimal LatestTransactionQuantity { get; set; }

    public string LatestTransactionUserId { get; set; } = string.Empty;
}
