using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Request boundary for validating an item against scanner send requirements.
/// </summary>
public sealed class Model_ScannerItemValidationRequest
{
    public Guid SessionId { get; set; }

    public Guid ItemId { get; set; }

    public string PartId { get; set; } = string.Empty;

    public string FromWarehouse { get; set; } = string.Empty;

    public string FromLocation { get; set; } = string.Empty;

    public string ToWarehouse { get; set; } = string.Empty;

    public string ToLocation { get; set; } = string.Empty;

    public string Quantity { get; set; } = string.Empty;
}
