using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Shared.Models.Lookup;

/// <summary>
/// Outcome of expanding a typed location range (for example "V-A0-01" to "V-A0-05") into the
/// warehouse locations that actually exist in Infor Visual.
/// </summary>
public sealed class Model_SharedLocationRangeResult
{
    /// <summary>Canonical (formatted, uppercase) start location after any auto-swap.</summary>
    public string StartLocation { get; set; } = string.Empty;

    /// <summary>Canonical (formatted, uppercase) stop location after any auto-swap.</summary>
    public string StopLocation { get; set; } = string.Empty;

    /// <summary>
    /// True when the operator's start location sorted after the stop location and the two were
    /// exchanged automatically.
    /// </summary>
    public bool WasSwapped { get; set; }

    /// <summary>Existing locations in the range, ordered by location ID.</summary>
    public List<string> Locations { get; set; } = [];

    /// <summary>
    /// True when the range contained more locations than the safety cap allows, so
    /// <see cref="Locations"/> holds only the first capped set.
    /// </summary>
    public bool WasTruncated { get; set; }
}
