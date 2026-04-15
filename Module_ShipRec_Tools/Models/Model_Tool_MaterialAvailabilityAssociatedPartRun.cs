using System;
using System.Globalization;
using System.Linq;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI model for one associated parent part and its best available next run date.
/// </summary>
public class Model_Tool_MaterialAvailabilityAssociatedPartRun
{
    public string AssociatedPartNumber { get; set; } = string.Empty;

    public string AssociatedPartDescription { get; set; } = string.Empty;

    public string ComponentPartNumber { get; set; } = string.Empty;

    public DateTime? NextDueToRunDate { get; set; }

    public bool IsFutureOrTodayRun { get; set; }

    public bool IsCurrentlyRunning { get; set; }

    public bool IsPastJob { get; set; }

    public bool IsOutsideSelectedLookAheadWindow { get; set; }

    public string NextDueDateSource { get; set; } = string.Empty;

    public string WorkOrderDisplay { get; set; } = string.Empty;

    public string WorkOrderStatus { get; set; } = string.Empty;

    public DateTime? WorkOrderStatusEffectiveDate { get; set; }

    public string SiteId { get; set; } = string.Empty;

    public DateTime? ScheduledStartDate { get; set; }

    public DateTime? ScheduledFinishDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public int? OperationSequence { get; set; }

    public decimal? QtyPer { get; set; }

    public decimal? FixedQty { get; set; }

    public decimal? CalculatedQty { get; set; }

    public decimal? IssuedQty { get; set; }

    public decimal? AllocatedQty { get; set; }

    public decimal? FulfilledQty { get; set; }

    public string UsageUnitOfMeasure { get; set; } = string.Empty;

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

    public decimal RequiredPartsQuantity { get; set; }

    public decimal EstimatedCoilUse { get; set; }

    public string NormalizedUsageUnitOfMeasure { get; set; } = string.Empty;

    public string NextRunDateDisplay =>
        FormatDate(ScheduledStartDate ?? NextDueToRunDate, "No scheduled date");

    public string RunTimingLabel =>
        NextDueToRunDate.HasValue is false ? "No scheduled job"
        : IsCurrentlyRunning ? "Currently Running"
        : IsPastJob ? "Past Job"
        : "Future Run";

    public string ScheduledFinishDateDisplay => FormatDate(ScheduledFinishDate);

    public string RequiredDateDisplay => FormatDate(RequiredDate);

    public string WorkOrderStatusEffectiveDateDisplay => FormatDate(WorkOrderStatusEffectiveDate);

    public string QtyPerDisplay => FormatDecimal(QtyPer);

    public string FixedQtyDisplay => FormatDecimal(FixedQty);

    public string CalculatedQtyDisplay => FormatDecimal(CalculatedQty);

    public string IssuedQtyDisplay => FormatDecimal(IssuedQty);

    public string AllocatedQtyDisplay => FormatDecimal(AllocatedQty);

    public string FulfilledQtyDisplay => FormatDecimal(FulfilledQty);

    public string RequiredPartsQuantityDisplay => FormatDecimal(RequiredPartsQuantity);

    public string EstimatedCoilUseDisplay => FormatDecimal(EstimatedCoilUse);

    public string ScrapPercentDisplay =>
        ScrapPercent.HasValue ? $"{ScrapPercent.Value:0.##}%" : string.Empty;

    public string RunQtyPerCycleDisplay => FormatDecimal(RunQtyPerCycle);

    public string SetupRunHoursDisplay =>
        string.IsNullOrWhiteSpace(FormatDecimal(SetupHours))
        && string.IsNullOrWhiteSpace(FormatDecimal(RunHours))
            ? string.Empty
            : $"Setup {FormatDecimal(SetupHours)} / Run {FormatDecimal(RunHours)}";

    public string DimensionsDisplay =>
        string.IsNullOrWhiteSpace(Dimensions) is false
            ? string.IsNullOrWhiteSpace(DimensionExpression)
                ? Dimensions
                : $"{Dimensions} ({DimensionExpression})"
            : DimensionExpression;

    public string LengthWidthHeightDisplay
    {
        get
        {
            var length = Length.HasValue ? $"L {FormatDecimal(Length)}" : string.Empty;
            var width = Width.HasValue ? $"W {FormatDecimal(Width)}" : string.Empty;
            var height = Height.HasValue ? $"H {FormatDecimal(Height)}" : string.Empty;
            return string.Join(
                " | ",
                new[] { length, width, height }.Where(value =>
                    string.IsNullOrWhiteSpace(value) is false
                )
            );
        }
    }

    public string DrawingRevisionDisplay =>
        string.IsNullOrWhiteSpace(DrawingId) ? DrawingRevision
        : string.IsNullOrWhiteSpace(DrawingRevision) ? DrawingId
        : $"{DrawingId} / Rev {DrawingRevision}";

    private static string FormatDecimal(decimal? value)
    {
        return value.HasValue
            ? value.Value.ToString("0.##", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatDate(DateTime? value)
    {
        return FormatDate(value, string.Empty);
    }

    private static string FormatDate(DateTime? value, string fallback)
    {
        return value.HasValue
            ? value.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
            : fallback;
    }
}
