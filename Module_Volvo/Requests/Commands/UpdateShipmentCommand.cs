using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Requests;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to update an existing shipment and its lines.
/// </summary>
public record UpdateShipmentCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Shipment ID to update.
    /// </summary>
    public int ShipmentId { get; init; }

    /// <summary>
    /// Updated shipment date.
    /// </summary>
    public DateTimeOffset ShipmentDate { get; init; }

    /// <summary>
    /// Updated notes.
    /// </summary>
    public string Notes { get; init; } = string.Empty;

    /// <summary>
    /// Updated PO number (if completed).
    /// </summary>
    public string PONumber { get; init; } = string.Empty;

    /// <summary>
    /// Updated receiver number (if completed).
    /// </summary>
    public string ReceiverNumber { get; init; } = string.Empty;

    /// <summary>
    /// Updated shipment line items.
    /// </summary>
    public List<ShipmentLineDataTransferObjects> Parts { get; init; } = new();
}
