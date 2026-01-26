using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to validate a PO number format and check for duplicates
/// Used in Wizard Mode Step 1 before proceeding
/// </summary>
public class QueryRequest_Receiving_Shared_Validate_PONumber : IRequest<Model_Dao_Result<Model_Receiving_DataTransferObjects_POValidationResult>>
{
    /// <summary>
    /// PO number to validate
    /// </summary>
    public string PONumber { get; set; } = string.Empty;
}
