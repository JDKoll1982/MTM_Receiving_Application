using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Assesses receipts that carry no purchase order, grouped by part, transaction
/// date and vendor. Such a row is never skipped for lacking a purchase order (FR-020).
/// </summary>
public interface IService_ReconciliationNonPurchaseOrderResolver
{
    /// <summary>Groups non-purchase-order rows under the same quantity proof and outcomes.</summary>
    /// <param name="rows">The non-purchase-order rows for one day.</param>
    Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> Resolve(
        IReadOnlyList<Model_ReconciliationRow> rows
    );
}
