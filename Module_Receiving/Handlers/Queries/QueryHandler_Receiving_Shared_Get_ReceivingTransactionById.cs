using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for GetReceivingTransactionByIdQuery
/// Retrieves a single transaction with all details
/// </summary>
public class QueryHandler_Receiving_Shared_Get_ReceivingTransactionById
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_ReceivingTransactionById, Model_Dao_Result<Model_Receiving_TableEntitys_ReceivingTransaction>>
{
    private readonly Dao_Receiving_Repository_Transaction _dao;
    private readonly IService_LoggingUtility _logger;

    public QueryHandler_Receiving_Shared_Get_ReceivingTransactionById(
        Dao_Receiving_Repository_Transaction dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_ReceivingTransaction>> Handle(
        QueryRequest_Receiving_Shared_Get_ReceivingTransactionById request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Querying transaction by ID: {request.TransactionId}");

            var result = await _dao.SelectByIdAsync(request.TransactionId);

            if (result.Success)
            {
                _logger.LogInfo($"Transaction found: {request.TransactionId}");
            }
            else
            {
                _logger.LogWarning($"Transaction not found: {request.TransactionId}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetReceivingTransactionByIdQueryHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_ReceivingTransaction>(
                $"Error querying transaction: {ex.Message}", ex);
        }
    }
}
