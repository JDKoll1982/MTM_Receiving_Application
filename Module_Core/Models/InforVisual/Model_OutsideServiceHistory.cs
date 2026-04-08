using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Represents a single service dispatch record from Infor Visual.
/// Returned by IService_InforVisual.GetOutsideServiceHistoryByPartAsync.
/// ⚠️ Read-only Infor Visual data — do not persist or modify.
/// </summary>
public class Model_OutsideServiceHistory
{
    /// <summary>Infor Visual vendor identifier.</summary>
    public string VendorID { get; set; } = string.Empty;

    /// <summary>Human-readable vendor name.</summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>City where the vendor is located.</summary>
    public string? VendorCity { get; set; }

    /// <summary>State/province where the vendor is located.</summary>
    public string? VendorState { get; set; }

    /// <summary>SERVICE_DISPATCH primary key.</summary>
    public string? DispatchID { get; set; }

    /// <summary>Date the dispatch was created in Infor Visual.</summary>
    public DateTime? DispatchDate { get; set; }

    /// <summary>Part number sent for outside service.</summary>
    public string? PartNumber { get; set; }

    /// <summary>Quantity of the part dispatched.</summary>
    public decimal? QuantitySent { get; set; }

    /// <summary>Current dispatch status (e.g., Open, Closed).</summary>
    public string? DispatchStatus { get; set; }

    /// <summary>
    /// True when this row represents multiple raw dispatch records combined for display.
    /// </summary>
    public bool IsCombinedRecord { get; set; }

    /// <summary>
    /// Display-friendly dispatch date text for the history grid.
    /// </summary>
    public string DispatchDateDisplay =>
        IsCombinedRecord ? "Combined Dates" : DispatchDate?.ToString("MM/dd/yyyy") ?? string.Empty;

    /// <summary>
    /// Display-friendly whole-number quantity text for the history grid.
    /// </summary>
    public string QuantitySentDisplay =>
        QuantitySent.HasValue
            ? decimal.Round(QuantitySent.Value, 0, MidpointRounding.AwayFromZero).ToString("0")
            : string.Empty;
}
