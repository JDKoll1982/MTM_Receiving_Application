using System;
using System.Collections.Generic;
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
/// Handler for GetReceivingLinesByPOQuery
/// Retrieves all lines associated with a Purchase Order
/// </summary>
public class QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO, Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;

    public QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO(
        Dao_Receiving_Repository_Line dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>> Handle(
        QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Querying receiving lines for PO: {request.PONumber}");

            var result = await _dao.SelectByPOAsync(request.PONumber);

            if (result.Success)
            {
                _logger.LogInfo($"Found {result.Data?.Count ?? 0} lines for PO: {request.PONumber}");
            }
            else
            {
                _logger.LogWarning($"No lines found for PO: {request.PONumber}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetReceivingLinesByPOQueryHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_ReceivingLoad>>(
                $"Error querying lines: {ex.Message}", ex);
        }
    }
}
