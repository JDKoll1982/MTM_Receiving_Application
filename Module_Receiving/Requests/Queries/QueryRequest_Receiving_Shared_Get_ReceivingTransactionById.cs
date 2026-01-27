using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to retrieve a receiving transaction by its ID
/// Used in Edit Mode and transaction review
/// </summary>
public class QueryRequest_Receiving_Shared_Get_ReceivingTransactionById : IRequest<Model_Dao_Result<Model_Receiving_TableEntitys_ReceivingTransaction>>
{
    /// <summary>
    /// Transaction ID to retrieve
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
}
