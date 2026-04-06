using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Represents one JSON-backed mock receiving transaction used to simulate InforVisual inventory movement.
/// </summary>
public sealed class Model_InforVisualMockReceivingTransaction
{
    public string SourceLoadId { get; set; } = string.Empty;

    public string PONumber { get; set; } = string.Empty;

    public string PartID { get; set; } = string.Empty;

    public string POLineNumber { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string UnitOfMeasure { get; set; } = "EA";

    public DateTime ReceivedDate { get; set; }

    public DateTime TransactionDate { get; set; }

    public string ReceiptWarehouseId { get; set; } = "002";

    public string ReceiptLocationId { get; set; } = string.Empty;

    public string CurrentWarehouseId { get; set; } = "002";

    public string CurrentLocationId { get; set; } = string.Empty;

    public int EmployeeNumber { get; set; }

    public string UserId { get; set; } = string.Empty;
}