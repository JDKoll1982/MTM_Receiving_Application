using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to retrieve shipment history with filters.
/// </summary>
public record GetShipmentHistoryQuery : IRequest<Model_Dao_Result<List<Model_VolvoShipment>>>
{
    /// <summary>
    /// Optional start date filter.
    /// </summary>
    public DateTimeOffset? StartDate { get; init; }

    /// <summary>
    /// Optional end date filter.
    /// </summary>
    public DateTimeOffset? EndDate { get; init; }

    /// <summary>
    /// Optional status filter (All, Pending PO, Completed, or raw status string).
    /// </summary>
    public string StatusFilter { get; init; } = "All";
}
