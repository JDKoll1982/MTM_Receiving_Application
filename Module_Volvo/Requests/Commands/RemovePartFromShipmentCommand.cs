using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to remove a part from the current shipment (in-memory operation).
/// </summary>
public record RemovePartFromShipmentCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Part number to remove from current shipment.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;
}
