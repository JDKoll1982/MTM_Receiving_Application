using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query for autocomplete search of Volvo parts by part number.
/// </summary>
public record SearchVolvoPartsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPart>>>
{
    /// <summary>
    /// Search text to match against part numbers (partial match supported).
    /// </summary>
    public string SearchText { get; init; } = string.Empty;

    /// <summary>
    /// Maximum number of results to return (default: 10).
    /// </summary>
    public int MaxResults { get; init; } = 10;
}
