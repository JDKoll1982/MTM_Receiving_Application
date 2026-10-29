using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Both sides of the Receiving Analytics coverage and category-mix panels. The ERP
/// half is produced separately because it can fail on its own while the other three
/// panels keep working; <see cref="ErpAvailable"/> is <see langword="false"/> when it
/// could not be read.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppComparison
{
    /// <summary>One coverage point: a received date and its count.</summary>
    public sealed record CoveragePoint(DateTime Date, int Count);

    /// <summary>One coverage point per date for the app side.</summary>
    public IReadOnlyList<CoveragePoint> AppCoverageSeries { get; set; } = [];

    /// <summary>One coverage point per date for the ERP side.</summary>
    public IReadOnlyList<CoveragePoint> ErpCoverageSeries { get; set; } = [];

    /// <summary>
    /// Category shares for the app side, keyed by the five ERP names from
    /// <c>Translation_ReceivingAnalyticsCategory</c>.
    /// </summary>
    public IReadOnlyDictionary<string, decimal> AppCategorySplit { get; set; } =
        new Dictionary<string, decimal>();

    /// <summary>
    /// Category shares for the ERP side, keyed by the five ERP names from
    /// <c>Translation_ReceivingAnalyticsCategory</c>.
    /// </summary>
    public IReadOnlyDictionary<string, decimal> ErpCategorySplit { get; set; } =
        new Dictionary<string, decimal>();

    /// <summary>
    /// Receipt line counts per ERP category over the same dates, read from the ERP and keyed by
    /// its own category name. The view model turns these into <see cref="ErpCategorySplit"/>:
    /// shares are a presentation concern, and the five names are the axis the panel draws.
    /// </summary>
    public IReadOnlyDictionary<string, int> ErpCategoryTotals { get; set; } =
        new Dictionary<string, int>();

    /// <summary><see langword="false"/> when the ERP half could not be read.</summary>
    public bool ErpAvailable { get; set; } = true;
}
