using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Partitions candidate rows into the pools that compete for movement evidence.
/// The methods are synchronous and pure, so they need no database to test.
/// </summary>
public interface IService_ReconciliationGroupResolver
{
    /// <summary>
    /// Partitions candidates into purchase-order groups keyed by purchase order,
    /// part and transaction date, and hands non-purchase-order rows to the
    /// non-purchase-order resolver (FR-015, FR-019).
    /// </summary>
    /// <param name="rows">The candidate rows for one day.</param>
    Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> Resolve(
        IReadOnlyList<Model_ReconciliationRow> rows
    );

    /// <summary>
    /// The purchase-order key alone, exposed so a test can assert the key without
    /// standing up a whole day.
    /// </summary>
    /// <param name="rows">The rows to partition.</param>
    Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> ResolvePurchaseOrderOnly(
        IReadOnlyList<Model_ReconciliationRow> rows
    );
}
