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
/// Handler for QueryRequest_Receiving_Shared_Get_QualityHoldsByLine.
/// Retrieves all quality hold records for a specific line.
/// Returns records ordered by creation date (newest first).
/// </summary>
public class QueryHandler_Receiving_Shared_Get_QualityHoldsByLine
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_QualityHoldsByLine, Model_Dao_Result<List<Model_Receiving_TableEntitys_QualityHold>>>
{
    private readonly Dao_Receiving_Repository_QualityHold _qualityHoldDao;
    private readonly IService_LoggingUtility _logger;

    public QueryHandler_Receiving_Shared_Get_QualityHoldsByLine(
        Dao_Receiving_Repository_QualityHold qualityHoldDao,
        IService_LoggingUtility logger)
    {
        _qualityHoldDao = qualityHoldDao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_QualityHold>>> Handle(
        QueryRequest_Receiving_Shared_Get_QualityHoldsByLine request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Retrieving quality holds for LineId={request.LineId}");

            var result = await _qualityHoldDao.GetQualityHoldsByLineIdAsync(request.LineId);

            if (result.Success)
            {
                _logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} quality hold records for LineId={request.LineId}");
                return Model_Dao_Result_Factory.Success(result.Data ?? new List<Model_Receiving_TableEntitys_QualityHold>());
            }

            _logger.LogError($"Failed to retrieve quality holds: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_QualityHold>>(
                result.ErrorMessage ?? "Failed to retrieve quality holds");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in QueryHandler_Receiving_Shared_Get_QualityHoldsByLine: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_QualityHold>>(
                $"Error retrieving quality holds: {ex.Message}", ex);
        }
    }
}
