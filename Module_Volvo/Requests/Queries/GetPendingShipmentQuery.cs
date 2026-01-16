using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to retrieve pending shipment for current user (to resume entry).
/// </summary>
public record GetPendingShipmentQuery : IRequest<Model_Dao_Result<Model_VolvoShipment>>
{
    /// <summary>
    /// Username of current user to find their pending shipment.
    /// </summary>
    public string UserName { get; init; } = string.Empty;
}
