using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Read-only work-order demand row showing which parent part consumes a component part and when it is next due to run.
/// </summary>
public class Model_InforVisualAssociatedPartRunRow
{
    public string InputPartNumber { get; set; } = string.Empty;

    public string InputPartDescription { get; set; } = string.Empty;

    public string ComponentPartNumber { get; set; } = string.Empty;

    public string AssociatedPartNumber { get; set; } = string.Empty;

    public string AssociatedPartDescription { get; set; } = string.Empty;

    public DateTime? NextDueToRunDate { get; set; }

    public bool IsFutureOrTodayRun { get; set; }

    public bool IsCurrentlyRunning { get; set; }

    public bool IsPastJob { get; set; }

    public string NextDueDateSource { get; set; } = string.Empty;

    public string WorkOrderType { get; set; } = string.Empty;

    public string WorkOrderBaseId { get; set; } = string.Empty;

    public string WorkOrderLotId { get; set; } = string.Empty;

    public string WorkOrderSplitId { get; set; } = string.Empty;

    public string WorkOrderSubId { get; set; } = string.Empty;

    public DateTime? WorkOrderStatusEffectiveDate { get; set; }

    public string SiteId { get; set; } = string.Empty;

    public int? OperationSeqNo { get; set; }

    public int? RequirementPieceNo { get; set; }

    public string WorkOrderStatus { get; set; } = string.Empty;

    public string RequirementStatus { get; set; } = string.Empty;

    public DateTime? ScheduledStartDate { get; set; }

    public DateTime? ScheduledFinishDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public decimal? QtyPer { get; set; }

    public decimal? FixedQty { get; set; }

    public decimal? CalcQty { get; set; }

    public decimal? IssuedQty { get; set; }

    public decimal? AllocatedQty { get; set; }

    public decimal? FulfilledQty { get; set; }

    public string UsageUm { get; set; } = string.Empty;

    public decimal? ScrapPercent { get; set; }

    public string OperationType { get; set; } = string.Empty;

    public string ResourceId { get; set; } = string.Empty;

    public string ServiceId { get; set; } = string.Empty;

    public string OperationWarehouseId { get; set; } = string.Empty;

    public decimal? RunQtyPerCycle { get; set; }

    public decimal? SetupHours { get; set; }

    public decimal? RunHours { get; set; }

    public string Dimensions { get; set; } = string.Empty;

    public string DimensionExpression { get; set; } = string.Empty;

    public decimal? Length { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public string DrawingId { get; set; } = string.Empty;

    public string DrawingRevision { get; set; } = string.Empty;
}
