using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to get shipment detail with line items for editing/viewing.
/// </summary>
public record GetShipmentDetailQuery : IRequest<Model_Dao_Result<ShipmentDetail>>
{
    /// <summary>
    /// Shipment ID to retrieve.
    /// </summary>
    public int ShipmentId { get; init; }
}

/// <summary>
/// Response DataTransferObjects containing complete shipment details including line items.
/// </summary>
public record ShipmentDetail
{
    /// <summary>
    /// Shipment header information.
    /// </summary>
    public Model_VolvoShipment Shipment { get; init; } = new();

    /// <summary>
    /// Shipment line items (parts with quantities and discrepancies).
    /// </summary>
    public List<Model_VolvoShipmentLine> Lines { get; init; } = new();
}
