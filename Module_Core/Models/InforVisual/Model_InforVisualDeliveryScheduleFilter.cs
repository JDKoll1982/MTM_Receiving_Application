using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Filter/query options for the Delivery Schedule grid (read-only Infor Visual).
/// Date bounds are inclusive on the line due date; search terms are LIKE patterns
/// (caller wraps in '%term%'). Scope/state flags gate which categories and states
/// are returned (a flag = false means that category/state is excluded).
/// </summary>
public class Model_InforVisualDeliveryScheduleFilter
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string PartSearch { get; set; } = string.Empty;

    public string PoSearch { get; set; } = string.Empty;

    public string SupplierSearch { get; set; } = string.Empty;

    public string CarrierSearch { get; set; } = string.Empty;

    /// <summary>True = "All of the Above": every non-empty search term is applied.</summary>
    public bool SearchAll { get; set; }

    public bool ScopeParts { get; set; } = true;

    public bool ScopeCoils { get; set; } = true;

    public bool ScopeFlat { get; set; } = true;

    public bool ScopeOutside { get; set; } = true;

    /// <summary>True = include Uninventoried (no part number) lines.</summary>
    public bool ScopeUninventoried { get; set; } = true;

    /// <summary>True = only show PARTIAL lines whose received/order percentage is BELOW <see cref="NearFillPct"/>.</summary>
    public bool ShowNearFilled { get; set; }

    /// <summary>Percentage threshold (1-99) for the near-filled filter. Default 90.</summary>
    public int NearFillPct { get; set; } = 90;

    public bool ShowOpen { get; set; } = true;

    public bool ShowClosed { get; set; } = true;

    public bool ShowOnTime { get; set; } = true;

    public bool ShowLate { get; set; } = true;

    /// <summary>Reference "now" used to classify on-time vs late.</summary>
    public DateTime Today { get; set; } = DateTime.Today;

    public int MaxResults { get; set; } = 5000;
}
