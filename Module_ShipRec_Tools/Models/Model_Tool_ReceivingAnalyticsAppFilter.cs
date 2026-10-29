using System;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Client-only filter state for the Receiving Analytics app-data tab. The date
/// bounds default to the last 30 calendar days: the start is today minus 29 days
/// (inclusive) and the end is today plus one day (exclusive).
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppFilter
{
    /// <summary>First date in the range, inclusive.</summary>
    public DateTime StartDate { get; set; } = DefaultStartDate();

    /// <summary>First date after the range, exclusive, so the range covers the last 30 calendar days.</summary>
    public DateTime EndDate { get; set; } = DefaultEndDate();

    /// <summary>Bucketing period for the activity and on-time panels.</summary>
    public Enum_ReceivingAnalyticsPeriod Period { get; set; } = Enum_ReceivingAnalyticsPeriod.Day;

    /// <summary>
    /// Dominant unit of measure. Null means no single unit, so the UI shows one
    /// line per unit. Blank reads as "EA" at render time, not here.
    /// </summary>
    public string? UnitOfMeasure { get; set; }

    /// <summary>Supplier-ranking measure for the supplier panel.</summary>
    public Enum_ReceivingAnalyticsSupplierMeasure SupplierMeasure { get; set; } =
        Enum_ReceivingAnalyticsSupplierMeasure.Labels;

    /// <summary>
    /// Restores every field to its default, recomputing the date bounds from
    /// today so they never freeze at type-initialisation time.
    /// </summary>
    public void Reset()
    {
        StartDate = DefaultStartDate();
        EndDate = DefaultEndDate();
        Period = Enum_ReceivingAnalyticsPeriod.Day;
        UnitOfMeasure = null;
        SupplierMeasure = Enum_ReceivingAnalyticsSupplierMeasure.Labels;
    }

    private static DateTime DefaultStartDate() => DateTime.Today.AddDays(-29);

    private static DateTime DefaultEndDate() => DateTime.Today.AddDays(1);
}
