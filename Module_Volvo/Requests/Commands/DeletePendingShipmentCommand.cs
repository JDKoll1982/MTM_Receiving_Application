using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to delete an existing pending Volvo shipment.
/// </summary>
public record DeletePendingShipmentCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Shipment identifier to delete.
    /// </summary>
    public int ShipmentId { get; init; }
}