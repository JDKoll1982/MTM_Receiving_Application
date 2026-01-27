using MediatR;

namespace MTM_Receiving_Application.Module_Volvo.Requests;

/// <summary>
/// Shared Data Transfer Object representing a shipment line item used across multiple commands.
/// </summary>
/// <remarks>
/// Used by: SavePendingShipmentCommand, CompleteShipmentCommand, UpdateShipmentCommand
/// </remarks>
public record ShipmentLineDataTransferObjects
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
