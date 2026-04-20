using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to delete a completed archived Volvo shipment from history.
/// </summary>
public record DeleteShipmentHistoryCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Archived shipment identifier to delete.
    /// </summary>
    public int ShipmentId { get; init; }
}
