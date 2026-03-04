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
}
