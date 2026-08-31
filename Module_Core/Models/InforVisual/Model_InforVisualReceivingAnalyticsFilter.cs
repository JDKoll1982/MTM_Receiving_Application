using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Filter/query options for the Receiving Analytics by-date chart (read-only
/// Infor Visual). Date bounds are inclusive on the activity date. Scope flags
/// gate which categories are returned (a flag = false excludes that category).
/// </summary>
public class Model_InforVisualReceivingAnalyticsFilter
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool ScopeParts { get; set; } = true;

    public bool ScopeCoils { get; set; } = true;

    public bool ScopeFlat { get; set; } = true;

    public bool ScopeOutside { get; set; } = true;

    public bool ScopeUninventoried { get; set; } = true;

    public int MaxResults { get; set; } = 50000;
}
