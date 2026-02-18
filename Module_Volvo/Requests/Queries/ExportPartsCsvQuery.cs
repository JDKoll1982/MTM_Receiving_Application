using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// [STUB] Query to export Volvo parts master data.
/// TODO: Implement database export operation.
/// </summary>
public record ExportPartsQuery : IRequest<Model_Dao_Result<string>>
{
    /// <summary>
    /// Include inactive parts in the export.
    /// </summary>
    public bool IncludeInactive { get; init; } = false;
}
