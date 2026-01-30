using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to retrieve all quality hold records for a specific line
/// Returns quality holds ordered by creation date (newest first)
/// </summary>
public class QueryRequest_Receiving_Shared_Get_QualityHoldsByLine : IRequest<Model_Dao_Result<List<Model_Receiving_TableEntitys_QualityHold>>>
{
    /// <summary>
    /// Line ID to query quality holds for
    /// </summary>
    public string LineId { get; set; } = string.Empty;
}
