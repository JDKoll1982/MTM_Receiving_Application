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

    public List<Model_Tool_MaterialAvailabilityIncomingDate> UpcomingDates { get; set; } = [];

    public List<Model_Tool_MaterialAvailabilityAssociatedPartRun> AssociatedPartRuns { get; set; } =
    [];

    public string NextDateSummary { get; set; } = "No associated part runs were found.";

    public DateTime? EarliestRelevantDate { get; set; }

    public bool IsExpanded { get; set; }

    public bool HasSearchLocation => string.IsNullOrWhiteSpace(SearchLocationId) is false;

    public bool HasCurrentLocations => CurrentLocations.Count > 0;

    public bool HasIncomingSupply => IncomingRollup.POLineCount > 0;

    public bool HasIncomingDates => UpcomingDates.Count > 0;

    public bool HasAssociatedPartRuns => AssociatedPartRuns.Count > 0;

    public string QuantitySummaryLabel =>
        HasSearchLocation ? $"Qty in {SearchLocationId}" : "Total on hand";

    public string QuantitySummaryDisplay =>
        FormatWhole(HasSearchLocation ? QuantityInSearchLocation : TotalPositiveQuantity);

    public string TotalPositiveQuantityDisplay => FormatWhole(TotalPositiveQuantity);

    public string LocationCountSummary =>
        HasCurrentLocations
            ? $"{CurrentLocations.Count} location(s) with qty > 0"
            : "No locations with qty > 0";

    internal int SortBucket { get; set; } = 2;

    internal DateTime SortDate { get; set; } = DateTime.MaxValue;

    private static string FormatWhole(decimal value)
    {
        return decimal.Round(value, 0, MidpointRounding.AwayFromZero).ToString("0");
    }
}
