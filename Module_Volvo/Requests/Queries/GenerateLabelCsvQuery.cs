using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// [STUB] Query to generate labels for a shipment.
/// TODO: Implement database-backed label generation.
/// </summary>
public record GenerateLabelQuery : IRequest<Model_Dao_Result<string>>
{
    /// <summary>
    /// Shipment ID to generate labels for.
    /// </summary>
    public int ShipmentId { get; init; }
}
