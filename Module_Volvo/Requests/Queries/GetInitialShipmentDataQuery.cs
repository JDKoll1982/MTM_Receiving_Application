using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to get initial data for new shipment entry (current date + next shipment number).
/// </summary>
public record GetInitialShipmentDataQuery : IRequest<Model_Dao_Result<InitialShipmentData>>
{
    // No parameters - returns current date and next available shipment number
}

/// <summary>
/// Response DTO containing initial shipment data.
/// </summary>
public record InitialShipmentData
{
    /// <summary>
    /// Current date/time for shipment initialization.
    /// </summary>
    public DateTimeOffset CurrentDate { get; init; }

    /// <summary>
    /// Next available shipment number (auto-incremented from database).
    /// </summary>
    public int NextShipmentNumber { get; init; }
}
