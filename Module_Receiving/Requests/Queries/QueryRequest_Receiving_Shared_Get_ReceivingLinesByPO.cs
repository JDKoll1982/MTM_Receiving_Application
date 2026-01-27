using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to retrieve all receiving lines for a given Purchase Order
/// Used in Wizard Mode Step 2 and Manual Mode for load display
/// </summary>
public class QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO : IRequest<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>>
{
    /// <summary>
    /// Purchase Order number to search for
    /// </summary>
    public string PONumber { get; set; } = string.Empty;
}
