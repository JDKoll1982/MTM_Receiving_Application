using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to format email data for a shipment.
/// </summary>
public record FormatEmailDataQuery : IRequest<Model_Dao_Result<Model_VolvoEmailData>>
{
    /// <summary>
    /// Shipment ID to format email data for.
    /// </summary>
    public int ShipmentId { get; init; }
}
