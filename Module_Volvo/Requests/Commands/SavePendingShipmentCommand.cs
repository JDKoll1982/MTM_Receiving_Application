using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to save shipment as pending (allows resuming later).
/// </summary>
public record SavePendingShipmentCommand : IRequest<Model_Dao_Result<int>>
{
    /// <summary>
    /// Optional existing shipment ID (null for new pending shipment).
    /// </summary>
    public int? ShipmentId { get; init; }

    /// <summary>
    /// Shipment date.
    /// </summary>
    public DateTimeOffset ShipmentDate { get; init; }

    /// <summary>
    /// Shipment number (auto-generated if not provided).
    /// </summary>
    public int ShipmentNumber { get; init; }

    /// <summary>
    /// Optional notes for this shipment.
    /// </summary>
    public string Notes { get; init; } = string.Empty;

    /// <summary>
    /// List of parts in this shipment.
    /// </summary>
    public List<ShipmentLineDataTransferObjects> Parts { get; init; } = new();
}
