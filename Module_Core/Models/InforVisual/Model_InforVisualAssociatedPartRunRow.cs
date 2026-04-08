using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Read-only work-order demand row showing which parent part consumes a component part and when it is next due to run.
/// </summary>
public class Model_InforVisualAssociatedPartRunRow
{
    public string InputPartNumber { get; set; } = string.Empty;

    public string InputPartDescription { get; set; } = string.Empty;

    public string AssociatedPartNumber { get; set; } = string.Empty;

    public string AssociatedPartDescription { get; set; } = string.Empty;

    public DateTime? NextDueToRunDate { get; set; }

    public bool IsFutureOrTodayRun { get; set; }

    public string NextDueDateSource { get; set; } = string.Empty;

    public string WorkOrderType { get; set; } = string.Empty;

    public string WorkOrderBaseId { get; set; } = string.Empty;

    public string WorkOrderLotId { get; set; } = string.Empty;

    public string WorkOrderSplitId { get; set; } = string.Empty;

    public string WorkOrderSubId { get; set; } = string.Empty;

    public int? OperationSeqNo { get; set; }

    public int? RequirementPieceNo { get; set; }

    public string WorkOrderStatus { get; set; } = string.Empty;

    public string RequirementStatus { get; set; } = string.Empty;
}
