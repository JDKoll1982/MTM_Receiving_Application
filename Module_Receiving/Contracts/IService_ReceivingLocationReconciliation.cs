using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Reconciles saved MTM receiving locations against InforVisual transaction history and current inventory.
/// </summary>
public interface IService_ReceivingLocationReconciliation
{
    Task<Model_Dao_Result<List<Model_ReceivingRecommendedLocation>>> GetRecommendedLocationsAsync(
        string poNumber,
        string partId,
        string? poLineNumber,
        DateTime? receivedDate
    );

    Task<Model_Dao_Result<Model_ReceivingLocationReconciliationSummary>> PreviewLocationsAsync(
        bool includeAllHistory
    );

    Task<Model_Dao_Result<Model_ReceivingLocationReconciliationItem>> ApplyLocationUpdateAsync(
        Model_ReceivingLocationReconciliationItem item
    );

    Task<Model_Dao_Result<Model_ReceivingLocationReconciliationSummary>> ReconcileLocationsAsync(
        bool includeAllHistory
    );
}
