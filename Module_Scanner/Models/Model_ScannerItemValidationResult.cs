using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Validation response boundary for one scanner item.
/// </summary>
public sealed class Model_ScannerItemValidationResult
{
    public Guid SessionId { get; set; }

    public Guid ItemId { get; set; }

    public Enum_ScannerValidationState State { get; set; } = Enum_ScannerValidationState.NotValidated;

    public string Message { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string CanonicalPartId { get; set; } = string.Empty;

    public string CanonicalFromLocation { get; set; } = string.Empty;

    public string CanonicalToLocation { get; set; } = string.Empty;

    /// <summary>
    /// On-hand quantity available at the source location as of validation time. Used to
    /// restore the per-row "quantity cannot exceed on-hand" guard after an app restart
    /// (MaxQuantity itself is intentionally not persisted).
    /// </summary>
    public decimal? MaxQuantity { get; set; }
}
