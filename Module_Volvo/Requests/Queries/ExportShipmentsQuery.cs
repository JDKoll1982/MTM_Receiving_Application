using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to export shipment history to CSV.
/// </summary>
public record ExportShipmentsQuery : IRequest<Model_Dao_Result<string>>
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
