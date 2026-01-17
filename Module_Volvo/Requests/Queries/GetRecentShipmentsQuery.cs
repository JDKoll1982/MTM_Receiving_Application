using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to retrieve recent Volvo shipments for history view.
/// </summary>
public record GetRecentShipmentsQuery : IRequest<Model_Dao_Result<List<Model_VolvoShipment>>>
{
    /// <summary>
    /// Number of days to look back (default: 30).
    /// </summary>
    public int Days { get; init; } = 30;
}
