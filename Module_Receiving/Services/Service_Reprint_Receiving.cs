using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Reprint;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Receiving.Contracts;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Receiving implementation of the reprint contract used by the Reprint Labels page.
/// </summary>
public class Service_Reprint_Receiving : IService_Reprint_Receiving
{
    private readonly IService_MySQL_Receiving _receivingService;
    private readonly IService_LoggingUtility _logger;

    public Service_Reprint_Receiving(
        IService_MySQL_Receiving receivingService,
        IService_LoggingUtility logger
    )
    {
        _receivingService =
            receivingService ?? throw new ArgumentNullException(nameof(receivingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
        Model_ReprintHistoryFilter filter
    ) => _receivingService.GetReprintHistoryAsync(filter);

    public async Task<Model_Dao_Result<Model_ReprintBatchResult>> ReprintAsync(
        IReadOnlyList<string> historyIds
    )
    {
        var result = new Model_ReprintBatchResult();

        foreach (var historyId in historyIds.Distinct())
        {
            if (!int.TryParse(historyId, out var id))
            {
                result.Failed.Add(historyId);
                continue;
            }

            var daoResult = await _receivingService.InsertFromHistoryAsync(id);
            Helper_ReprintBatch.Categorize(result, historyId, daoResult);
        }

        _logger.LogInfo(
            $"Receiving reprint batch: {result.QueuedCount} queued, {result.AlreadyQueuedCount} already queued, {result.FailedCount} failed"
        );
        return Model_Dao_Result_Factory.Success(result);
    }
}
