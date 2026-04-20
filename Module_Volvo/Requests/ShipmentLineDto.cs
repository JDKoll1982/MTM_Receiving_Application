using MediatR;

namespace MTM_Receiving_Application.Module_Volvo.Requests;

/// <summary>
/// Shared Data Transfer Object representing a shipment line item used across multiple commands.
/// </summary>
/// <remarks>
/// Used by: SavePendingShipmentCommand, CompleteShipmentCommand, UpdateShipmentCommand
/// </remarks>
public record ShipmentLineDto
{
    /// <summary>
    /// Part number from Volvo master data.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>
    /// Number of skids received for this part.
    /// </summary>
    public int ReceivedSkidCount { get; init; }

    /// <summary>
    /// Warehouse location for this part line.
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Quantity per skid persisted for this row.
    /// </summary>
    public int QuantityPerSkid { get; init; }

    /// <summary>
    /// Pending/Received status used by the Volvo queue cards.
    /// </summary>
    public string PoStatus { get; init; } = Models.VolvoLinePoStatus.Pending;

    /// <summary>
    /// Expected number of skids (populated when HasDiscrepancy = true).
    /// </summary>
    public int? ExpectedSkidCount { get; init; }

    /// <summary>
    /// Indicates if there is a discrepancy between received and expected counts.
    /// </summary>
    public bool HasDiscrepancy { get; init; }

    /// <summary>
    /// Note explaining the discrepancy (required when HasDiscrepancy = true).
    /// </summary>
    public string DiscrepancyNote { get; init; } = string.Empty;
}
