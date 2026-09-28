using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Reprint;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Dunnage.Contracts;

namespace MTM_Receiving_Application.Module_Dunnage.Services;

/// <summary>
/// Dunnage implementation of the reprint contract used by the Reprint Labels page.
/// </summary>
public class Service_Reprint_Dunnage : IService_Reprint_Dunnage
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_LoggingUtility _logger;

    public Service_Reprint_Dunnage(
        IService_MySQL_Dunnage dunnageService,
        IService_LoggingUtility logger
    )
    {
        _dunnageService = dunnageService ?? throw new ArgumentNullException(nameof(dunnageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
        Model_ReprintHistoryFilter filter
    ) => _dunnageService.GetReprintHistoryAsync(filter);

    public async Task<Model_Dao_Result<Model_ReprintBatchResult>> ReprintAsync(
        IReadOnlyList<string> loadUuids
    )
    {
        var result = new Model_ReprintBatchResult();

        foreach (var loadUuid in loadUuids.Distinct())
        {
            var daoResult = await _dunnageService.InsertFromHistoryAsync(loadUuid);
            Helper_ReprintBatch.Categorize(result, loadUuid, daoResult);
        }

        _logger.LogInfo(
            $"Dunnage reprint batch: {result.QueuedCount} queued, {result.AlreadyQueuedCount} already queued, {result.FailedCount} failed"
        );
        return Model_Dao_Result_Factory.Success(result);
    }
}
