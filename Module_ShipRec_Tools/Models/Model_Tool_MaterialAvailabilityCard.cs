using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI card model for the location and part-based material availability board.
/// </summary>
public class Model_Tool_MaterialAvailabilityCard
{
    public string PartId { get; set; } = string.Empty;

    public string PartDescription { get; set; } = string.Empty;

    public string SearchLocationId { get; set; } = string.Empty;

    public decimal QuantityInSearchLocation { get; set; }

    public decimal TotalPositiveQuantity { get; set; }

    public List<Model_Tool_MaterialAvailabilityLocation> CurrentLocations { get; set; } = [];

    public Model_Tool_MaterialAvailabilityRollup IncomingRollup { get; set; } = new();

    public List<Model_Tool_MaterialAvailabilityIncomingLine> IncomingLines { get; set; } = [];

    public List<Model_Tool_MaterialAvailabilityIncomingDate> UpcomingDates { get; set; } = [];

    public List<Model_Tool_MaterialAvailabilityAssociatedPartRun> AssociatedPartRuns { get; set; } =
    [];

    public string NextDateSummary { get; set; } = "No associated part runs were found.";

    public string DescriptionDisplay =>
        string.IsNullOrWhiteSpace(PartDescription)
            ? "Description: Not available"
            : $"Description: {PartDescription}";

    public DateTime? EarliestRelevantDate { get; set; }

    public bool IsExpanded { get; set; }

    public bool HasSearchLocation => string.IsNullOrWhiteSpace(SearchLocationId) is false;

    public bool HasCurrentLocations => CurrentLocations.Count > 0;

    public bool HasIncomingSupply => IncomingRollup.POLineCount > 0;

    public bool HasIncomingDetails => IncomingLines.Count > 0;

    public bool HasIncomingDates => UpcomingDates.Count > 0;

    public bool HasAssociatedPartRuns => AssociatedPartRuns.Count > 0;

    public Model_Tool_MaterialAvailabilityAssociatedPartRun? PrimaryAssociatedPartRun =>
        AssociatedPartRuns.Count > 0 ? AssociatedPartRuns[0] : null;

    public string QuantitySummaryLabel =>
        HasSearchLocation ? $"Qty in {SearchLocationId}" : "Total on hand";

    public string QuantitySummaryDisplay =>
        FormatWhole(HasSearchLocation ? QuantityInSearchLocation : TotalPositiveQuantity);

    public string TotalPositiveQuantityDisplay => FormatWhole(TotalPositiveQuantity);

    public string LocationCountSummary =>
        HasCurrentLocations
            ? CurrentLocations.Count == 1
                ? "1 location found - Click to view it"
                : $"{CurrentLocations.Count} locations found - Click to view them"
            : "No locations found";

    public bool HasEstimatedCoilUseRisk =>
        PrimaryAssociatedPartRun is not null
        && PrimaryAssociatedPartRun.EstimatedCoilUse > TotalPositiveQuantity;

    public string EstimatedCoilUseRiskText =>
        HasEstimatedCoilUseRisk && PrimaryAssociatedPartRun is not null
            ? $"Estimated coil use ({PrimaryAssociatedPartRun.EstimatedCoilUseDisplay}) exceeds on hand ({TotalPositiveQuantityDisplay})."
            : string.Empty;

    public string NextRunSummaryDisplay
    {
        get
        {
            if (PrimaryAssociatedPartRun is null)
            {
                return NextDateSummary;
            }

            var usageUnit = string.IsNullOrWhiteSpace(
                PrimaryAssociatedPartRun.NormalizedUsageUnitOfMeasure
            )
                ? string.Empty
                : $" {PrimaryAssociatedPartRun.NormalizedUsageUnitOfMeasure}";
            var summaryDate = PrimaryAssociatedPartRun.NextDueToRunDate.HasValue
                ? $" on {PrimaryAssociatedPartRun.NextRunDateDisplay}"
                : string.Empty;
            return $"{PrimaryAssociatedPartRun.RunTimingLabel}: {PrimaryAssociatedPartRun.WorkOrderDisplay} / {PrimaryAssociatedPartRun.AssociatedPartNumber}{summaryDate} | Required Parts: {PrimaryAssociatedPartRun.RequiredPartsQuantityDisplay} | Estimated Coil Use: {PrimaryAssociatedPartRun.EstimatedCoilUseDisplay}{usageUnit}";
        }
    }

    internal int SortBucket { get; set; } = 2;

    internal DateTime SortDate { get; set; } = DateTime.MaxValue;

    private static string FormatWhole(decimal value)
    {
        return decimal.Round(value, 0, MidpointRounding.AwayFromZero).ToString("0");
    }
}
