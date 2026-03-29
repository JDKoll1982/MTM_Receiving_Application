using System;

namespace MTM_Receiving_Application.Module_OutsideService.Models;

/// <summary>
/// Represents one vendor suggestion derived from prior Outside Service history.
/// </summary>
public class Model_OutsideServiceVendorSuggestion
{
    /// <summary>
    /// Gets or sets the vendor identifier.
    /// </summary>
    public string VendorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vendor name.
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vendor location detail.
    /// </summary>
    public string? LocationDetail { get; set; }

    /// <summary>
    /// Gets or sets the most recent dispatch date.
    /// </summary>
    public DateTime? LastDispatchDate { get; set; }

    /// <summary>
    /// Gets or sets the number of historical dispatches for this vendor and part.
    /// </summary>
    public int DispatchCount { get; set; }

    /// <summary>
    /// Gets the summary string shown in Setup.
    /// </summary>
    public string Summary =>
        LastDispatchDate.HasValue
            ? $"{DispatchCount} dispatch(es) — last {LastDispatchDate:MM/dd/yy}"
            : $"{DispatchCount} dispatch(es)";
}
