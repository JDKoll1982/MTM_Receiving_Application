using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to retrieve all Volvo parts for the settings grid.
/// </summary>
public record GetAllVolvoPartsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPart>>>
{
    /// <summary>
    /// Include inactive parts in the result set.
    /// </summary>
    public bool IncludeInactive { get; init; } = false;
}
