using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to generate and verify label data for a shipment.
/// Loads the shipment + lines from the database, runs the component explosion,
/// and returns a human-readable summary of what was generated.
/// </summary>
public record GenerateLabelQuery : IRequest<Model_Dao_Result<string>>
{
    /// <summary>
    /// Shipment ID to generate labels for.
    /// </summary>
    public int ShipmentId { get; init; }
}
