using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Suggests where a receipt should be put, from Infor Visual location evidence.
/// The reconciliation methods that used to live here moved to the Receiving
/// Validator module, and this contract keeps only the entry-time recommendation
/// with its signature unchanged (FR-006, SC-02).
/// </summary>
public interface IService_ReceivingLocationReconciliation
{
    /// <summary>Suggest the locations this part has recently occupied.</summary>
    /// <param name="poNumber">The purchase order number.</param>
    /// <param name="partId">The part identifier.</param>
    /// <param name="poLineNumber">The purchase order line, when one is known.</param>
    /// <param name="receivedDate">The receipt date anchor, when one is known.</param>
    Task<Model_Dao_Result<List<Model_ReceivingRecommendedLocation>>> GetRecommendedLocationsAsync(
        string poNumber,
        string partId,
        string? poLineNumber,
        DateTime? receivedDate
    );
}
