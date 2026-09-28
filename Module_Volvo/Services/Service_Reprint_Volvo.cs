using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Reprint;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Volvo implementation of the reprint contract used by the Reprint Labels page. Volvo's label
/// output is the generated-label queue, so this reads volvo_generated_label_history and re-queues
/// into volvo_generated_label_data with an is_reprint marker.
/// </summary>
public class Service_Reprint_Volvo : IService_Reprint_Volvo
{
    private readonly IDao_VolvoGeneratedLabelData _generatedLabelDao;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_LoggingUtility _logger;

    public Service_Reprint_Volvo(
        IDao_VolvoGeneratedLabelData generatedLabelDao,
        IService_UserSessionManager sessionManager,
        IService_LoggingUtility logger
    )
    {
        _generatedLabelDao =
            generatedLabelDao ?? throw new ArgumentNullException(nameof(generatedLabelDao));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
        Model_ReprintHistoryFilter filter
    ) => _generatedLabelDao.GetReprintHistoryAsync(filter);

    public async Task<Model_Dao_Result<Model_ReprintBatchResult>> ReprintAsync(
        IReadOnlyList<string> historyIds
    )
    {
        var user = _sessionManager.CurrentSession?.User;
        var queuedBy = user?.WindowsUsername ?? "SYSTEM";
        var employeeNumber = user?.EmployeeNumber ?? 0;

        var result = new Model_ReprintBatchResult();

        foreach (var historyId in historyIds.Distinct())
        {
            if (!int.TryParse(historyId, out var id))
            {
                result.Failed.Add(historyId);
                continue;
            }

            var daoResult = await _generatedLabelDao.InsertFromHistoryAsync(
                id,
                queuedBy,
                employeeNumber
            );
            Helper_ReprintBatch.Categorize(result, historyId, daoResult);
        }

        _logger.LogInfo(
            $"Volvo reprint batch: {result.QueuedCount} queued, {result.AlreadyQueuedCount} already queued, {result.FailedCount} failed"
        );
        return Model_Dao_Result_Factory.Success(result);
    }
}
