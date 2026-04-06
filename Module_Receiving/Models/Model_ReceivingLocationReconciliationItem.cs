using System;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Describes the reconciliation outcome for one saved receiving row.
/// </summary>
public sealed class Model_ReceivingLocationReconciliationItem
{
    public Guid LoadId { get; set; }

    public int? LabelDataRecordId { get; set; }

    public int? HistoryRecordId { get; set; }

    public string DataSource { get; set; } = string.Empty;

    public string PartID { get; set; } = string.Empty;

    public string PONumber { get; set; } = string.Empty;

    public string POLineNumber { get; set; } = string.Empty;

    public DateTime ReceivedDate { get; set; }

    public string ExistingLocation { get; set; } = string.Empty;

    public string ProposedLocation { get; set; } = string.Empty;

    public decimal SavedRowQuantity { get; set; }

    public decimal QuantityMoved { get; set; }

    public decimal MatchedLocationQuantity { get; set; }

    public string QuantityUnitOfMeasure { get; set; } = string.Empty;

    public string AllocationMethod { get; set; } = string.Empty;

    public decimal QuantityDifference { get; set; }

    public string MovedByUserId { get; set; } = string.Empty;

    public DateTime? MovedAt { get; set; }

    public string Resolution { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public Model_ReceivingLoad? SourceLoad { get; set; }
}
