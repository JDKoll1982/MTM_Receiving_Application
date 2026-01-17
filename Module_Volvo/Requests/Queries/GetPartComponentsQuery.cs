using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to retrieve BOM components for a Volvo part.
/// </summary>
public record GetPartComponentsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPartComponent>>>
{
    /// <summary>
    /// Parent part number to retrieve components for.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;
}
